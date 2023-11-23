using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class BucketRuleSetService : BaseService<BucketRuleSet>, IBucketRuleSetService
{
    internal BucketRuleSetService(DbContextOptions<DatabaseContext> dbContextOptions)  
        : base(dbContextOptions, new BucketRuleSetRepository(new DatabaseContext(dbContextOptions)))
    {
    }

    public override BucketRuleSet Get(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRuleSetRepository(dbContext);
            
            var result = repository.ByIdWithIncludedEntities(id);
            if (result == null) throw new Exception($"{typeof(BucketRuleSet)} not found in database");
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public override IEnumerable<BucketRuleSet> GetAll()
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRuleSetRepository(dbContext);
            return repository
                .AllWithIncludedEntities()
                .OrderBy(i => i.Priority)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<MappingRule> GetMappingRules(Guid bucketRuleSetId)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new MappingRuleRepository(dbContext);
            return repository
                .AllWithIncludedEntities()
                .Where(i => i.BucketRuleSetId == bucketRuleSetId)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public override BucketRuleSet Update(BucketRuleSet entity)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRuleSetRepository = new BucketRuleSetRepository(dbContext);
            var mappingRuleRepository = new MappingRuleRepository(dbContext);
            
            // Check if Mapping Rules need to be deleted
            var deletedIds = 
                // Collect database entities
                mappingRuleRepository.All()
                .Where(i => i.BucketRuleSetId == entity.Id)
                .ToList()
                // Select which of the database IDs are no longer available in entity
                .Where(i => entity.MappingRules
                    .All(j => j.Id != i.Id))
                .Select(i => i.Id)
                .ToList();
            if (deletedIds.Any())
            {
                var result = mappingRuleRepository.DeleteRange(deletedIds);
                if (result != deletedIds.Count) 
                    throw new Exception("Unable to delete old MappingRules of that BucketRuleSet");
            }
            
            // Update BucketRuleSet including MappingRules
            bucketRuleSetRepository.Update(entity);
            
            transaction.Commit();
            return entity;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override void Delete(Guid id)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRuleSetRepository = new BucketRuleSetRepository(dbContext);
            var mappingRuleRepository = new MappingRuleRepository(dbContext);
            
            // Delete all existing Mapping Rules
            mappingRuleRepository.DeleteRange(mappingRuleRepository
                .All()
                .Where(i => i.BucketRuleSetId == id)
                .Select(i => i.Id)
                .ToList());
            
            // Delete BucketRuleSet
            bucketRuleSetRepository.Delete(id);
            
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }
}