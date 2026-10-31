using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBucketMovementService : GenericBucketMovementService<MockDatabase>
{
    private readonly MockDatabase _mockDatabase;
    private readonly ILogger<MockBucketMovementService> _logger;
    
    public MockBucketMovementService(MockDatabase mockDatabase, ILogger<MockBucketMovementService> logger) : base(logger)
    {
        _mockDatabase = mockDatabase;
        _logger = logger;
    }
    
    protected override MockDatabase CreateDbConnection() => _mockDatabase;
    protected override IBucketMovementRepository CreateBaseRepository(MockDatabase dbConnection)
        => new MockBucketMovementRepository(dbConnection);
}