using System.Data.Common;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Services;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbServiceManager : IServiceManager
{
    public IAccountService AccountService =>
        new DuckDbAccountService(_dbConnectionFactory, new Logger<DuckDbAccountService>(_loggerFactory));

    public IBankTransactionService BankTransactionService =>
        new DuckDbBankTransactionService(_dbConnectionFactory, new Logger<DuckDbBankTransactionService>(_loggerFactory));

    public IBucketGroupService BucketGroupService =>
        new DuckDbBucketGroupService(_dbConnectionFactory, new Logger<DuckDbBucketGroupService>(_loggerFactory));

    public IBucketMovementService BucketMovementService =>
        new DuckDbBucketMovementService(_dbConnectionFactory, new Logger<DuckDbBucketMovementService>(_loggerFactory));

    public IBucketService BucketService =>
        new DuckDbBucketService(_dbConnectionFactory, new Logger<DuckDbBucketService>(_loggerFactory));

    public IBucketRuleSetService BucketRuleSetService =>
        new DuckDbBucketRuleSetService(_dbConnectionFactory, new Logger<DuckDbBucketRuleSetService>(_loggerFactory));

    public IBudgetedTransactionService BudgetedTransactionService =>
        new DuckDbBudgetedTransactionService(_dbConnectionFactory, new Logger<DuckDbBudgetedTransactionService>(_loggerFactory));

    public IImportProfileService ImportProfileService =>
        new DuckDbImportProfileService(_dbConnectionFactory, new Logger<DuckDbImportProfileService>(_loggerFactory));

    public IRecurringBankTransactionService RecurringBankTransactionService =>
        new DuckDbRecurringBankTransactionService(_dbConnectionFactory, new Logger<DuckDbRecurringBankTransactionService>(_loggerFactory));

    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILoggerFactory _loggerFactory;

    public DuckDbServiceManager(Func<DbConnection> dbConnectionFactory, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public ILogger CreateLogger(Type type)
    {
        return _loggerFactory.CreateLogger(type);
    }
}