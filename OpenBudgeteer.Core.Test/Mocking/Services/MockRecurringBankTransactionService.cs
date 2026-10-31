using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockRecurringBankTransactionService : GenericRecurringBankTransactionService<MockDatabase>
{
    private readonly MockDatabase _mockDatabase;
    private readonly ILogger<MockRecurringBankTransactionService> _logger;
    
    public MockRecurringBankTransactionService(MockDatabase mockDatabase, ILogger<MockRecurringBankTransactionService> logger) : base(logger)
    {
        _mockDatabase = mockDatabase;
        _logger = logger;
    }
    
    protected override MockDatabase CreateDbConnection() => _mockDatabase;
    protected override IRecurringBankTransactionRepository CreateBaseRepository(MockDatabase dbConnection)
        => new MockRecurringBankTransactionRepository(dbConnection);
    protected override IBankTransactionRepository CreateBankTransactionRepository(MockDatabase dbConnection)
        => new MockBankTransactionRepository(dbConnection);
}