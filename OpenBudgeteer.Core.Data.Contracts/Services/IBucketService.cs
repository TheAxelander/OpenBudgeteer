using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBucketService : IBaseService<Bucket>
{
    public Bucket GetWithLatestVersion(Guid id);
    public IEnumerable<Bucket> GetAllWithoutSystemBuckets();
    public IEnumerable<Bucket> GetSystemBuckets();
    public IEnumerable<Bucket> GetActiveBuckets(DateOnly validFrom);
    //public BucketVersion GetLatestVersion(Guid bucketId);
    public BucketVersion GetLatestVersion(Guid bucketId, DateOnly yearMonth);
    public BucketFigures GetFigures(Guid bucketId, DateOnly yearMonth);
    public decimal GetBalance(Guid bucketId, DateOnly yearMonth);
    public BucketFigures GetInAndOut(Guid bucketId, DateOnly yearMonth);
    /*public Bucket Create(Bucket bucket, BucketVersion bucketVersion, DateOnly yearMonth);
    public Bucket Update(Bucket bucket, BucketVersion bucketVersion, DateOnly yearMonth);*/
    //public Bucket Close(Bucket entity, DateOnly yearMonth);
    public void Close(Guid id, DateOnly yearMonth);
    public BucketMovement CreateMovement(Guid bucketId, decimal amount, DateOnly movementDate);
}