using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketRuleSetService : EFCoreBaseService<BucketRuleSet>, IBucketRuleSetService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;

    public EFCoreBucketRuleSetService(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected override GenericBucketRuleSetService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBucketRuleSetService(
            new BucketRuleSetRepository(dbContext),
            new MappingRuleRepository(dbContext));
    }

    public override BucketRuleSet Get(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.Get(id);
        }
        catch (EntityNotFoundException e)
        {
            Console.WriteLine(e);
            throw new Exception($"{typeof(BucketRuleSet)} not found in database");
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAll();
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
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetMappingRules(bucketRuleSetId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public override BucketRuleSet Update(BucketRuleSet entity)
    {
        using var dbContext = new DatabaseContext(_dbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        var baseService = CreateBaseService(dbContext);
        try
        {
            var results = baseService.Update(entity);
            transaction.Commit();
            return results;
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
        using var dbContext = new DatabaseContext(_dbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        var baseService = CreateBaseService(dbContext);
        try
        {
            baseService.Delete(id);
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