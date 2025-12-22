using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

internal static class BucketRuleSetMapperExtensions
{
    public static BucketRuleSet MapWithTargetBucket(this BucketRuleSet bucketRuleSet, Bucket bucket)
    {
        bucketRuleSet.TargetBucket = bucket;
        return bucketRuleSet;
    }

    public static BucketRuleSet MapWithMappingRule(this BucketRuleSet bucketRuleSet, MappingRule? mappingRule)
    {
        BaseMapperExtensions.AddIfNotNull(
            bucketRuleSet.MappingRules ??= new List<MappingRule>(),
            mappingRule,
            m => m.Id);
        return bucketRuleSet;
    }
}

internal class BucketRuleSetMapper
{
    public IEnumerable<BucketRuleSet> Results => _cache.Values;

    private readonly Dictionary<Guid, BucketRuleSet> _cache = new();

    public BucketRuleSet MapWithEverything(BucketRuleSet bucketRuleSet, Bucket bucket, MappingRule? mappingRule)
    {
        var result = GetOrAdd(bucketRuleSet);
        return result
            .MapWithTargetBucket(bucket)
            .MapWithMappingRule(mappingRule);
    }

    private BucketRuleSet GetOrAdd(BucketRuleSet bucketRuleSet)
    {
        if (_cache.TryGetValue(bucketRuleSet.Id, out var existing))
            return existing;

        existing = bucketRuleSet;
        _cache.Add(bucketRuleSet.Id, existing);

        return existing;
    }
}
