using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBucketGroupService : IBaseService<BucketGroup>
{
    public IEnumerable<BucketGroup> GetAllFull();
    public IEnumerable<BucketGroup> GetSystemBucketGroups();
    public IEnumerable<Bucket> GetBuckets(Guid bucketGroupId);
    public BucketGroup Move(Guid bucketGroupId, int positions);
}