using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class BucketGroupMapperExtensions
{
    public static BucketGroup MapWithBucket(this BucketGroup bucketGroup, Bucket? bucket)
    {
        BaseMapperExtensions.AddWithBackReference(
            bucketGroup.Buckets ??= new List<Bucket>(),
            bucket,
            b => b.Id,
            b => b.BucketGroup = bucketGroup);
        return bucketGroup;
    }
}

public class BucketGroupMapper
{
    public IEnumerable<BucketGroup> Results => _cache.Values;

    private readonly Dictionary<Guid, BucketGroup> _cache = new();

    public BucketGroup MapWithEverything(BucketGroup bucketGroup, Bucket? bucket)
    {
        var result = GetOrAdd(bucketGroup);
        return result.MapWithBucket(bucket);
    }

    private BucketGroup GetOrAdd(BucketGroup bucketGroup)
    {
        if (_cache.TryGetValue(bucketGroup.Id, out var existing))
            return existing;

        existing = bucketGroup;
        _cache.Add(bucketGroup.Id, existing);

        return existing;
    }
}
