using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBankTransactionService : GenericBankTransactionService<MockDatabase>
{
    private readonly MockDatabase _mockDatabase;
    private readonly ILogger<MockBankTransactionService> _logger;

    public MockBankTransactionService(MockDatabase mockDatabase, ILogger<MockBankTransactionService> logger) : base(logger)
    {
        _mockDatabase = mockDatabase;
        _logger = logger;
    }
    
    protected override MockDatabase CreateDbConnection() => _mockDatabase;
    protected override IBankTransactionRepository CreateBaseRepository(MockDatabase dbConnection)
        => new MockBankTransactionRepository(dbConnection);
    protected override IBudgetedTransactionRepository CreateBudgetedTransactionRepository(MockDatabase dbConnection)
        => new MockBudgetedTransactionRepository(dbConnection);
}