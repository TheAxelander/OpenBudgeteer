using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBucketRuleSetService : GenericBucketRuleSetService
{
    public MockBucketRuleSetService(MockDatabase mockDatabase) : base(
        new MockBucketRuleSetRepository(mockDatabase), 
        new MockMappingRuleRepository(mockDatabase))
    {
    }
}