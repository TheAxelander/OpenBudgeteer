using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbAccountService : GenericAccountService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbAccountService> _logger;

    public DuckDbAccountService(
        Func<DbConnection> dbConnectionFactory, 
        ILogger<DuckDbAccountService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IAccountRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbAccountRepository(dbConnection);
    protected override IBankTransactionRepository CreateBankTransactionRepository(DbConnection dbConnection) => new DuckDbBankTransactionRepository(dbConnection);
}