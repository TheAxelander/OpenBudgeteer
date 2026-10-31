using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockAccountService : GenericAccountService<MockDatabase>
{
    private readonly MockDatabase _mockDatabase;
    private readonly ILogger<MockAccountService> _logger;
    
    public MockAccountService(MockDatabase mockDatabase, ILogger<MockAccountService> logger) : base(logger)
    {
        _mockDatabase = mockDatabase;
        _logger = logger;
    }

    protected override MockDatabase CreateDbConnection() => _mockDatabase;
    protected override IAccountRepository CreateBaseRepository(MockDatabase dbConnection)
        => new MockAccountRepository(dbConnection);
    protected override IBankTransactionRepository CreateBankTransactionRepository(MockDatabase dbConnection)
        => new MockBankTransactionRepository(dbConnection);
}