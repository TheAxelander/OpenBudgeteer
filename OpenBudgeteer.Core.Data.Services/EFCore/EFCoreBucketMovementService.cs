using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.EFCore;

public class EFCoreBucketMovementService : EFCoreBaseService<BucketMovement>, IBucketMovementService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;
    
    public EFCoreBucketMovementService(DbContextOptions<DatabaseContext> dbContextOptions) : base(dbContextOptions)
    {
        _dbContextOptions = dbContextOptions;
    }

    protected override GenericBucketMovementService CreateBaseService(DatabaseContext dbContext)
    {
        return new GenericBucketMovementService(new BucketMovementRepository(dbContext));
    }

    public IEnumerable<BucketMovement> GetAll(DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAll(periodStart, periodEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }

    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId)
    {
        return GetAllFromBucket(bucketId, DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId, DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(_dbContextOptions);
            var baseService = CreateBaseService(dbContext);
            return baseService.GetAllFromBucket(bucketId, periodStart, periodEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
}