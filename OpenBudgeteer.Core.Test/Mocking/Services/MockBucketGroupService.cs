using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBucketGroupService : GenericBucketGroupService<MockDatabase>
{
    private readonly MockDatabase _mockDatabase;
    private readonly ILogger<MockBucketGroupService> _logger;
    
    public MockBucketGroupService(MockDatabase mockDatabase, ILogger<MockBucketGroupService> logger) : base(logger)
    {
        _mockDatabase = mockDatabase;
        _logger = logger;
    }
    
    protected override MockDatabase CreateDbConnection() => _mockDatabase;
    protected override IBucketGroupRepository CreateBaseRepository(MockDatabase dbConnection)
        => new MockBucketGroupRepository(dbConnection);
}