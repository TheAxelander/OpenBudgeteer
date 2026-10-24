using System.Data.Common;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbImportProfileService : GenericImportProfileService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbImportProfileService> _logger;

    public DuckDbImportProfileService(
        Func<DbConnection> dbConnectionFactory, 
        ILogger<DuckDbImportProfileService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IImportProfileRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbImportProfileRepository(dbConnection);
}