using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Contracts.Services;

public interface IBucketRuleSetService : IBaseService<BucketRuleSet>
{
    public IEnumerable<MappingRule> GetMappingRules(Guid bucketRuleSetId);
}