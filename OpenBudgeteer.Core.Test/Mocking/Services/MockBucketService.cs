using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBucketService : GenericBucketService<MockDatabase>
{
    private readonly MockDatabase _mockDatabase;
    private readonly ILogger<MockBucketService> _logger;
    
    public MockBucketService(MockDatabase mockDatabase, ILogger<MockBucketService> logger) : base(logger)
    {
        _mockDatabase = mockDatabase;
        _logger = logger;
    }
    
    protected override MockDatabase CreateDbConnection() => _mockDatabase;
    protected override IBucketRepository CreateBaseRepository(MockDatabase dbConnection)
        => new MockBucketRepository(dbConnection);
    protected override IBucketVersionRepository CreateBucketVersionRepository(MockDatabase dbConnection)
        => new MockBucketVersionRepository(dbConnection);
    protected override IBudgetedTransactionRepository CreateBudgetedTransactionRepository(MockDatabase dbConnection)
        => new MockBudgetedTransactionRepository(dbConnection);
    protected override IBucketMovementRepository CreateBucketMovementRepository(MockDatabase dbConnection)
        => new MockBucketMovementRepository(dbConnection);
    protected override IBucketRuleSetRepository CreateBucketRuleSetRepository(MockDatabase dbConnection)
        => new MockBucketRuleSetRepository(dbConnection);
}