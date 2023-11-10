using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class BucketRuleSetService : BaseService<BucketRuleSet>, IBucketRuleSetService
{
    internal BucketRuleSetService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions)
    {
    }
    
    public override IEnumerable<BucketRuleSet> GetAll()
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketRuleSetRepository(dbContext);
            return repository
                .All()
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
                .Where(i => i.BucketRuleSetId == bucketRuleSetId)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public override BucketRuleSet Create(BucketRuleSet entity)
    {
        throw new NotSupportedException(
            "Please use alternative Create procedure as creation of Mapping Rules is mandatory");
    }
    
    public Tuple<BucketRuleSet, List<MappingRule>> Create(BucketRuleSet entity, List<MappingRule> mappingRules)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRuleSetRepository = new BucketRuleSetRepository(dbContext);
            var mappingRuleRepository = new MappingRuleRepository(dbContext);
            
            bucketRuleSetRepository.Create(entity);

            foreach (var mappingRule in mappingRules)
            {
                mappingRule.Id = Guid.Empty;
                mappingRule.BucketRuleSetId = entity.Id;
            }
            mappingRuleRepository.CreateRange(mappingRules);
            
            transaction.Commit();
            return new Tuple<BucketRuleSet, List<MappingRule>>(entity, mappingRules);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override BucketRuleSet Update(BucketRuleSet entity)
    {
        throw new NotSupportedException(
            "Please use alternative Update procedure as update of Mapping Rules is required");
    }
    
    public Tuple<BucketRuleSet, List<MappingRule>> Update(BucketRuleSet entity, List<MappingRule> mappingRules)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRuleSetRepository = new BucketRuleSetRepository(dbContext);
            var mappingRuleRepository = new MappingRuleRepository(dbContext);
            
            // Delete all existing Mapping Rules
            mappingRuleRepository.DeleteRange(mappingRuleRepository
                .Where(i => i.BucketRuleSetId == entity.Id)
                .ToList());
            
            // Update BucketRuleSet
            bucketRuleSetRepository.Update(entity);
            
            // (Re)Create Mapping Rules
            foreach (var mappingRule in mappingRules)
            {
                mappingRule.Id = Guid.Empty;
                mappingRule.BucketRuleSetId = entity.Id;
            }
            mappingRuleRepository.CreateRange(mappingRules);
            
            transaction.Commit();
            return new Tuple<BucketRuleSet, List<MappingRule>>(entity, mappingRules);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Errors during database update: {e.Message}");
        }
    }

    public override BucketRuleSet Delete(BucketRuleSet entity)
    {
        using var dbContext = new DatabaseContext(DbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            var bucketRuleSetRepository = new BucketRuleSetRepository(dbContext);
            var mappingRuleRepository = new MappingRuleRepository(dbContext);
            
            // Delete all existing Mapping Rules
            mappingRuleRepository.DeleteRange(mappingRuleRepository
                .Where(i => i.BucketRuleSetId == entity.Id)
                .ToList());
            
            // Delete BucketRuleSet
            bucketRuleSetRepository.Delete(entity);
            
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
}