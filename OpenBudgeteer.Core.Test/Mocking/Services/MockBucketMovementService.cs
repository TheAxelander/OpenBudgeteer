using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBucketMovementService : GenericBucketMovementService
{
    public MockBucketMovementService(MockDatabase mockDatabase) : base(new MockBucketMovementRepository(mockDatabase))
    {
    }
}