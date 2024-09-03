using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketGroupService : EFCoreBaseService<BucketGroup>, IBucketGroupService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;
    
    public EFCoreBucketGroupService(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected override GenericBucketGroupService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBucketGroupService(new BucketGroupRepository(dbContext));
    }

    public BucketGroup GetWithBuckets(Guid id)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetWithBuckets(id);
        }
        catch (EntityNotFoundException e)
        {
            Console.WriteLine(e);
            throw new Exception($"{typeof(BucketGroup)} not found in database");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<BucketGroup> GetAllFull()
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllFull();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<BucketGroup> GetSystemBucketGroups()
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetSystemBucketGroups();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public BucketGroup Move(Guid bucketGroupId, int positions)
    {
        using var dbContext = new DatabaseContext(_dbContextOptions);
        using var transaction = dbContext.Database.BeginTransaction();
        var baseService = CreateBaseService(dbContext);
        try
        {
            var results = baseService.Move(bucketGroupId, positions);
            transaction.Commit();
            return results;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            Console.WriteLine(e);
            throw new Exception($"Error during database update: {e.Message}");
        }
    }
}