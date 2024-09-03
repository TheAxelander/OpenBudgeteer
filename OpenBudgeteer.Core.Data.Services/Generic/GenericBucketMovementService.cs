using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Services.Generic;

public class GenericBucketMovementService : GenericBaseService<BucketMovement>, IBucketMovementService
{
    private readonly IBucketMovementRepository _bucketMovementRepository;
    
    public GenericBucketMovementService(
        IBucketMovementRepository bucketMovementRepository) : base(bucketMovementRepository)
    {
        _bucketMovementRepository = bucketMovementRepository;
    }

    public IEnumerable<BucketMovement> GetAll(DateTime periodStart, DateTime periodEnd)
    {
        return _bucketMovementRepository
            .All()
            .Where(i =>
                i.MovementDate >= periodStart &&
                i.MovementDate <= periodEnd)
            .ToList();
    }

    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId)
    {
        return GetAllFromBucket(bucketId, DateTime.MinValue, DateTime.MaxValue);
    }

    public IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId, DateTime periodStart, DateTime periodEnd)
    {
        return _bucketMovementRepository
            .All()
            .Where(i =>
                i.MovementDate >= periodStart &&
                i.MovementDate <= periodEnd &&
                i.BucketId == bucketId)
            .ToList();
    }
}