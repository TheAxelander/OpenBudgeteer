using System.Data.Common;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbRecurringBankTransactionService : GenericRecurringBankTransactionService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbRecurringBankTransactionService> _logger;

    public DuckDbRecurringBankTransactionService(
        Func<DbConnection> dbConnectionFactory, 
        ILogger<DuckDbRecurringBankTransactionService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IRecurringBankTransactionRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbRecurringBankTransactionRepository(dbConnection);
    protected override IBankTransactionRepository CreateBankTransactionRepository(DbConnection dbConnection) => new DuckDbBankTransactionRepository(dbConnection);
}