using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class BucketVersionMapperExtensions
{
    public static BucketVersion MapWithBucket(this BucketVersion bucketVersion, Bucket bucket)
    {
        bucketVersion.Bucket = bucket;
        return bucketVersion;
    }
}

public class BucketVersionMapper
{
    public IEnumerable<BucketVersion> Results => _cache.Values;

    private readonly Dictionary<Guid, BucketVersion> _cache = new();

    public BucketVersion MapWithEverything(BucketVersion bucketVersion, Bucket bucket)
    {
        var result = GetOrAdd(bucketVersion);
        return result.MapWithBucket(bucket);
    }

    private BucketVersion GetOrAdd(BucketVersion bucketVersion)
    {
        if (_cache.TryGetValue(bucketVersion.Id, out var existing))
            return existing;

        existing = bucketVersion;
        _cache.Add(bucketVersion.Id, existing);

        return existing;
    }
}
