using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Exceptions;
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

    public override IEnumerable<Account> GetActiveAccounts()
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var sql = """
                      SELECT
                          AccountId AS Id
                          ,Name
                          ,IsActive
                      FROM Account
                      WHERE IsActive = 1
                      ORDER BY Name
                      """;
            return dbConnection.Query<Account>(sql).ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    /// <inheritdoc/>
    public override Account CloseAccount(Guid id)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            var balanceSql = """
                             SELECT COALESCE(SUM(Amount), 0)
                             FROM BankTransaction
                             WHERE AccountId = $id
                             """;
            var balance = dbConnection.ExecuteScalar<decimal>(balanceSql, new { id = id.ToString() });

            if (balance != 0) throw new EntityUpdateException("Balance must be 0 to close an Account");

            var account = Get(id);
            account.IsActive = 0;
            return Update(account);
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to close Account: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}
