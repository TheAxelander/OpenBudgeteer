using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBankTransactionService : GenericBankTransactionService
{
    public MockBankTransactionService(MockDatabase mockDatabase) : base(
        new MockBankTransactionRepository(mockDatabase),
        new MockBudgetedTransactionRepository(mockDatabase))
    {
    }
}