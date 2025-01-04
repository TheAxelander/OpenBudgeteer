using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBucketService : GenericBucketService
{
    public MockBucketService(MockDatabase mockDatabase) : base(
            new MockBucketRepository(mockDatabase), 
            new MockBucketVersionRepository(mockDatabase), 
            new MockBudgetedTransactionRepository(mockDatabase), 
            new MockBucketMovementRepository(mockDatabase), 
            new MockBucketRuleSetRepository(mockDatabase))
    {
    }
}