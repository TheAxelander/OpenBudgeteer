using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBudgetedTransactionService : GenericBudgetedTransactionService
{
    public MockBudgetedTransactionService(MockDatabase mockDatabase) : base(new MockBudgetedTransactionRepository(mockDatabase))
    {
    }
}