using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockBucketRuleSetService : GenericBucketRuleSetService<MockDatabase>
{
    private readonly MockDatabase _mockDatabase;
    private readonly ILogger<MockBucketRuleSetService> _logger;
    
    public MockBucketRuleSetService(MockDatabase mockDatabase, ILogger<MockBucketRuleSetService> logger) : base(logger)
    {
        _mockDatabase = mockDatabase;
        _logger = logger;
    }
    
    protected override MockDatabase CreateDbConnection() => _mockDatabase;
    protected override IBucketRuleSetRepository CreateBaseRepository(MockDatabase dbConnection)
        => new MockBucketRuleSetRepository(dbConnection);
    protected override IMappingRuleRepository CreateMappingRuleRepository(MockDatabase dbConnection)
        => new MockMappingRuleRepository(dbConnection);
}