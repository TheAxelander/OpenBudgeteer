using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBucketService : IBaseService<Bucket>
{
    public Bucket GetWithLatestVersion(Guid id);
    public IEnumerable<Bucket> GetAllWithSystemBuckets();
    public IEnumerable<Bucket> GetSystemBuckets();
    public IEnumerable<Bucket> GetActiveBuckets(DateTime validFrom);
    //public BucketVersion GetLatestVersion(Guid bucketId);
    public BucketVersion GetLatestVersion(Guid bucketId, DateTime yearMonth);
    public BucketFigures GetFigures(Guid bucketId, DateTime yearMonth);
    public decimal GetBalance(Guid bucketId, DateTime yearMonth);
    public BucketFigures GetInAndOut(Guid bucketId, DateTime yearMonth);
    /*public Bucket Create(Bucket bucket, BucketVersion bucketVersion, DateTime yearMonth);
    public Bucket Update(Bucket bucket, BucketVersion bucketVersion, DateTime yearMonth);*/
    //public Bucket Close(Bucket entity, DateTime yearMonth);
    public void Close(Guid id, DateTime yearMonth);
    public BucketMovement CreateMovement(Guid bucketId, decimal amount, DateTime movementDate);
}