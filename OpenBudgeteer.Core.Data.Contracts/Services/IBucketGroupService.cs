using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBucketGroupService : IBaseService<BucketGroup>
{
    public BucketGroup GetWithBuckets(Guid id);
    public IEnumerable<BucketGroup> GetAllFull();
    public IEnumerable<BucketGroup> GetSystemBucketGroups();
    public BucketGroup Move(Guid bucketGroupId, int positions);
}