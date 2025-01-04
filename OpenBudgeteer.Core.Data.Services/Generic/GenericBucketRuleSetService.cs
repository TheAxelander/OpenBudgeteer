using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericBucketRuleSetService : GenericBaseService<BucketRuleSet>, IBucketRuleSetService
{
    private readonly IBucketRuleSetRepository _bucketRuleSetRepository;
    private readonly IMappingRuleRepository _mappingRuleRepository;
    
    public GenericBucketRuleSetService(
        IBucketRuleSetRepository bucketRuleSetRepository, IMappingRuleRepository mappingRuleRepository) : base(bucketRuleSetRepository)
    {
        _bucketRuleSetRepository = bucketRuleSetRepository;
        _mappingRuleRepository = mappingRuleRepository;
    }

    public override BucketRuleSet Get(Guid id)
    {
        var result = _bucketRuleSetRepository.ByIdWithIncludedEntities(id);
        if (result == null) throw new EntityNotFoundException();
        return result;
    }

    public override IEnumerable<BucketRuleSet> GetAll()
    {
        return _bucketRuleSetRepository
            .AllWithIncludedEntities()
            .OrderBy(i => i.Priority)
            .ToList();
    }

    public IEnumerable<MappingRule> GetMappingRules(Guid bucketRuleSetId)
    {
        return _mappingRuleRepository
            .AllWithIncludedEntities()
            .Where(i => i.BucketRuleSetId == bucketRuleSetId)
            .ToList();
    }

    public override BucketRuleSet Update(BucketRuleSet entity)
    {
        // Check if Mapping Rules need to be deleted
        var deletedIds = 
            // Collect database entities
            _mappingRuleRepository.All()
                .Where(i => i.BucketRuleSetId == entity.Id)
                .ToList()
                // Select which of the database IDs are no longer available in entity
                .Where(i => entity.MappingRules != null && entity.MappingRules
                    .All(j => j.Id != i.Id))
                .Select(i => i.Id)
                .ToList();
        if (deletedIds.Count != 0)
        {
            var result = _mappingRuleRepository.DeleteRange(deletedIds);
            if (result != deletedIds.Count) 
                throw new Exception("Unable to delete old MappingRules of that BucketRuleSet");
        }
            
        // Update BucketRuleSet including MappingRules
        _bucketRuleSetRepository.Update(entity);
            
        return entity;
    }

    public override void Delete(Guid id)
    {
        // Delete all existing Mapping Rules
        _mappingRuleRepository.DeleteRange(_mappingRuleRepository
            .All()
            .Where(i => i.BucketRuleSetId == id)
            .Select(i => i.Id)
            .ToList());
            
        // Delete BucketRuleSet
        _bucketRuleSetRepository.Delete(id);
    }
}