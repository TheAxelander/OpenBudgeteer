using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBucketMovementService : IBaseService<BucketMovement>
{
    public IEnumerable<BucketMovement> GetAll(DateOnly periodStart, DateOnly periodEnd);
    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId);
    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId, DateOnly periodStart, DateOnly periodEnd);
}