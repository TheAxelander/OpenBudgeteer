using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Services.Generic;
using OpenBudgeteer.Core.Test.Mocking.Repository;

namespace OpenBudgeteer.Core.Test.Mocking.Services;

public class MockImportProfileService : GenericImportProfileService<MockDatabase>
{
    private readonly MockDatabase _mockDatabase;
    private readonly ILogger<MockImportProfileService> _logger;
    
    public MockImportProfileService(MockDatabase mockDatabase, ILogger<MockImportProfileService> logger) : base(logger)
    {
        _mockDatabase = mockDatabase;
        _logger = logger;
    }
    
    protected override MockDatabase CreateDbConnection() => _mockDatabase;
    protected override IImportProfileRepository CreateBaseRepository(MockDatabase dbConnection)
        => new MockImportProfileRepository(dbConnection);
}