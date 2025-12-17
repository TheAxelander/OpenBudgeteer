using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbBucketMovementService : GenericBucketMovementService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBucketMovementService> _logger;

    public DuckDbBucketMovementService(
        Func<DbConnection> dbConnectionFactory, 
        ILogger<DuckDbBucketMovementService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBucketMovementRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbBucketMovementRepository(dbConnection);
}