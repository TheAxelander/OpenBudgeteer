using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBucketRuleSetService : IBaseService<BucketRuleSet>
{
    public IEnumerable<MappingRule> GetMappingRules(Guid bucketRuleSetId);
    public Tuple<BucketRuleSet, List<MappingRule>> Create(BucketRuleSet entity, List<MappingRule> mappingRules);
    public Tuple<BucketRuleSet, List<MappingRule>> Update(BucketRuleSet entity, List<MappingRule> mappingRules);
}