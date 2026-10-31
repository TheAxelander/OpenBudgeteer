using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

public static class MappingRuleMapperExtensions
{
    public static MappingRule MapWithBucketRuleSet(this MappingRule mappingRule, BucketRuleSet bucketRuleSet)
    {
        mappingRule.BucketRuleSet = bucketRuleSet;
        return mappingRule;
    }
}

public class MappingRuleMapper
{
    public IEnumerable<MappingRule> Results => _cache.Values;

    private readonly Dictionary<Guid, MappingRule> _cache = new();

    public MappingRule MapWithEverything(MappingRule mappingRule, BucketRuleSet bucketRuleSet)
    {
        var result = GetOrAdd(mappingRule);
        return result.MapWithBucketRuleSet(bucketRuleSet);
    }

    private MappingRule GetOrAdd(MappingRule mappingRule)
    {
        if (_cache.TryGetValue(mappingRule.Id, out var existing))
            return existing;

        existing = mappingRule;
        _cache.Add(mappingRule.Id, existing);

        return existing;
    }
}
