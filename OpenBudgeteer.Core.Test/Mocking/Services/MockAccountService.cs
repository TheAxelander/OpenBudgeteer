using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockAccountService : GenericAccountService
{
    public MockAccountService(MockDatabase mockDatabase) : base(
        new MockAccountRepository(mockDatabase), 
        new MockBankTransactionRepository(mockDatabase))
    {
    }
}