using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockRecurringBankTransactionService : GenericRecurringBankTransactionService
{
    public MockRecurringBankTransactionService(MockDatabase mockDatabase) : base(
        new MockRecurringBankTransactionRepository(mockDatabase),
        new MockBankTransactionRepository(mockDatabase))
    {
    }
}