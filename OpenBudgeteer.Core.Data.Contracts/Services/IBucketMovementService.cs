using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBucketMovementService : IBaseService<BucketMovement>
{
    public IEnumerable<BucketMovement> GetAll(DateTime periodStart, DateTime periodEnd);
    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId);
    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId, DateTime periodStart, DateTime periodEnd);
}