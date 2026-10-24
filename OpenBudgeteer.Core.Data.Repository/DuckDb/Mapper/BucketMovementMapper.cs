using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class BucketMovementMapperExtensions
{
    public static BucketMovement MapWithBucket(this BucketMovement bucketMovement, Bucket bucket)
    {
        bucketMovement.Bucket = bucket;
        return bucketMovement;
    }
}

public class BucketMovementMapper
{
    public IEnumerable<BucketMovement> Results => _cache.Values;

    private readonly Dictionary<Guid, BucketMovement> _cache = new();

    public BucketMovement MapWithEverything(BucketMovement bucketMovement, Bucket bucket)
    {
        var result = GetOrAdd(bucketMovement);
        return result.MapWithBucket(bucket);
    }

    private BucketMovement GetOrAdd(BucketMovement bucketMovement)
    {
        if (_cache.TryGetValue(bucketMovement.Id, out var existing))
            return existing;

        existing = bucketMovement;
        _cache.Add(bucketMovement.Id, existing);

        return existing;
    }
}
