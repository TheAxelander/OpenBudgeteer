using Microsoft.EntityFrameworkCore;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository;

namespace OpenBudgeteer.Core.Data.Services;

internal class BucketMovementService : BaseService<BucketMovement>, IBucketMovementService
{
    internal BucketMovementService(DbContextOptions<DatabaseContext> dbContextOptions) 
        : base(dbContextOptions, new BucketMovementRepository(new DatabaseContext(dbContextOptions)))
    {
    }
    
    public IEnumerable<BucketMovement> GetAll(DateTime periodStart, DateTime periodEnd)
    {
        try
        {
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketMovementRepository(dbContext);
            return repository
                .All()
                .Where(i =>
                    i.MovementDate >= periodStart &&
                    i.MovementDate <= periodEnd)
                .ToList();
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
            using var dbContext = new DatabaseContext(DbContextOptions);
            var repository = new BucketMovementRepository(dbContext);
            return repository
                .All()
                .Where(i =>
                    i.MovementDate >= periodStart &&
                    i.MovementDate <= periodEnd &&
                    i.BucketId == bucketId)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception($"Error on querying database: {e.Message}");
        }
    }
}