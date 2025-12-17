using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Services.Exceptions;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public abstract class GenericBucketRuleSetService<TDatabase> : GenericBaseService<BucketRuleSet, TDatabase>, IBucketRuleSetService
    where TDatabase : class, IDisposable
{
    private readonly ILogger _logger;
    
    public GenericBucketRuleSetService(ILogger logger) : base(logger)
    {
        _logger = logger;
    }
    
    protected abstract override IBucketRuleSetRepository CreateBaseRepository(TDatabase dbConnection);
    protected abstract IMappingRuleRepository CreateMappingRuleRepository(TDatabase dbConnection);

    public override BucketRuleSet Get(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketRuleSetRepository = CreateBaseRepository(dbConnection);
            var result = bucketRuleSetRepository.ByIdWithIncludedEntities(id);
            if (result is null) throw new EntityNotFoundException("Unable to find Rule Set with the given id.");
            return result;
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public override IEnumerable<BucketRuleSet> GetAll()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketRuleSetRepository = CreateBaseRepository(dbConnection);
            return bucketRuleSetRepository
                .AllWithIncludedEntities()
                .OrderBy(i => i.Priority)
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public virtual IEnumerable<MappingRule> GetMappingRules(Guid bucketRuleSetId)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var mappingRuleRepository = CreateMappingRuleRepository(dbConnection);
            return mappingRuleRepository
                .AllWithIncludedEntities()
                .Where(i => i.BucketRuleSetId == bucketRuleSetId)
                .ToList();
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public override BucketRuleSet Update(BucketRuleSet entity)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketRuleSetRepository = CreateBaseRepository(dbConnection);
            var mappingRuleRepository = CreateMappingRuleRepository(dbConnection);
            
            // Check if Mapping Rules need to be deleted
            var deletedIds = 
                // Collect database entities
                mappingRuleRepository.All()
                    .Where(i => i.BucketRuleSetId == entity.Id)
                    .ToList()
                    // Select which of the database IDs are no longer available in entity
                    .Where(i => entity.MappingRules is not null && entity.MappingRules
                        .All(j => j.Id != i.Id))
                    .Select(i => i.Id)
                    .ToList();
            if (deletedIds.Count != 0)
            {
                var result = mappingRuleRepository.DeleteRange(deletedIds);
                if (result != deletedIds.Count) 
                    throw new EntityUpdateException("Unable to delete old Mapping Rules of that Rule Set");
            }
            
            // Update BucketRuleSet including MappingRules
            bucketRuleSetRepository.Update(entity);
            
            return entity;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to update Rule Set in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override void Delete(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bucketRuleSetRepository = CreateBaseRepository(dbConnection);
            var mappingRuleRepository = CreateMappingRuleRepository(dbConnection);
            
            // Delete all existing Mapping Rules
            mappingRuleRepository.DeleteRange(mappingRuleRepository
                .All()
                .Where(i => i.BucketRuleSetId == id)
                .Select(i => i.Id)
                .ToList());
            
            // Delete BucketRuleSet
            bucketRuleSetRepository.Delete(id);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to delete Rule Set in database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}