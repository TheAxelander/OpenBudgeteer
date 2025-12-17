using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbBudgetedTransactionService : GenericBudgetedTransactionService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBudgetedTransactionService> _logger;

    public DuckDbBudgetedTransactionService(
        Func<DbConnection> dbConnectionFactory, 
        ILogger<DuckDbBudgetedTransactionService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBudgetedTransactionRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbBudgetedTransactionRepository(dbConnection);
}