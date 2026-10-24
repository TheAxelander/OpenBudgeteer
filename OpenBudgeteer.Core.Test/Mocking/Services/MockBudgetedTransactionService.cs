using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBudgetedTransactionService : GenericBudgetedTransactionService<MockDatabase>
{
    private readonly MockDatabase _mockDatabase;
    private readonly ILogger<MockBudgetedTransactionService> _logger;
    
    public MockBudgetedTransactionService(MockDatabase mockDatabase, ILogger<MockBudgetedTransactionService> logger) : base(logger)
    {
        _mockDatabase = mockDatabase;
        _logger = logger;
    }
    
    protected override MockDatabase CreateDbConnection() => _mockDatabase;
    protected override IBudgetedTransactionRepository CreateBaseRepository(MockDatabase dbConnection)
        => new MockBudgetedTransactionRepository(dbConnection);
}