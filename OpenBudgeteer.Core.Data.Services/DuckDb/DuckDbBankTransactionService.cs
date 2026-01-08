using System.Data.Common;
using System.Text;
using Dapper;
using DuckDB.NET.Data;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbBankTransactionService : GenericBankTransactionService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBankTransactionService> _logger;

    public DuckDbBankTransactionService(
        Func<DbConnection> dbConnectionFactory,
        ILogger<DuckDbBankTransactionService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBankTransactionRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbBankTransactionRepository(dbConnection);
    protected override IBudgetedTransactionRepository CreateBudgetedTransactionRepository(DbConnection dbConnection) => new DuckDbBudgetedTransactionRepository(dbConnection);

    public override IEnumerable<BankTransaction> GetAll(DateOnly? periodStart, DateOnly? periodEnd, int limit = 0)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            var sql = new StringBuilder(
                """
                SELECT
                    bt.TransactionId AS Id
                    ,bt.AccountId
                    ,bt.TransactionDate
                    ,bt.Payee
                    ,bt.Memo
                    ,bt.Amount
                    ,a.AccountId AS Id
                    ,a.Name
                    ,a.IsActive
                FROM BankTransaction bt
                INNER JOIN Account a ON bt.AccountId = a.AccountId
                WHERE
                    bt.TransactionDate >= $periodStart
                    AND bt.TransactionDate <= $periodEnd
                ORDER BY bt.TransactionDate DESC
                """);

            object parameters;
            if (limit > 0)
            {
                sql.Append("LIMIT $limit");
                parameters = new
                {
                    periodStart = periodStart?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    periodEnd = periodEnd?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MaxValue,
                    limit = limit
                };
            }
            else
            {
                parameters = new
                {
                    periodStart = periodStart?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    periodEnd = periodEnd?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MaxValue
                };
            }

            var mapper = new BankTransactionMapper();
            _ = dbConnection
                .Query<BankTransaction, Account, BankTransaction>(
                    sql.ToString(),
                    mapper.MapWithAccount,
                    param: parameters,
                    splitOn: "Id")
                .ToList();

            return mapper.Results.AsQueryable();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BankTransaction> GetFromAccount(Guid accountId, DateOnly? periodStart, DateOnly? periodEnd, int limit = 0)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            var sql = new StringBuilder(
                """
                SELECT
                    bt.TransactionId AS Id
                    ,bt.AccountId
                    ,bt.TransactionDate
                    ,bt.Payee
                    ,bt.Memo
                    ,bt.Amount
                    ,a.AccountId AS Id
                    ,a.Name
                    ,a.IsActive
                FROM BankTransaction bt
                INNER JOIN Account a ON bt.AccountId = a.AccountId
                WHERE
                    bt.TransactionDate >= $periodStart AND bt.TransactionDate <= $periodEnd
                    AND a.AccountId = $accountId
                ORDER BY bt.TransactionDate DESC
                """);

            object parameters;
            if (limit > 0)
            {
                sql.Append("LIMIT $limit");
                parameters = new
                {
                    periodStart = periodStart?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    periodEnd = periodEnd?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MaxValue,
                    accountId = accountId.ToString(),
                    limit = limit
                };
            }
            else
            {
                parameters = new
                {
                    periodStart = periodStart?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    periodEnd = periodEnd?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MaxValue,
                    accountId = accountId.ToString()
                };
            }

            var mapper = new BankTransactionMapper();
            _ = dbConnection
                .Query<BankTransaction, Account, BankTransaction>(
                    sql.ToString(),
                    mapper.MapWithAccount,
                    param: parameters,
                    splitOn: "Id")
                .ToList();

            return mapper.Results.AsQueryable();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override BankTransaction Update(BankTransaction entity)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var budgetedTransactionRepository = CreateBudgetedTransactionRepository(dbConnection);

            if (entity.BudgetedTransactions is not null && entity.BudgetedTransactions.Any())
            {
                // Delete all existing bucket assignments, as they will be replaced by passed assignments
                var deleteSql = """
                    SELECT BudgetedTransactionId AS Id
                    FROM BudgetedTransaction
                    WHERE TransactionId = $transactionId
                    """;
                var deletedIds = dbConnection
                    .Query<Guid>(deleteSql, new { transactionId = entity.Id.ToString() })
                    .ToList();

                if (deletedIds.Count != 0)
                {
                    var result = budgetedTransactionRepository.DeleteRange(deletedIds);
                    if (result != deletedIds.Count)
                        throw new EntityUpdateException("Unable to delete old Bucket Assignments of that Transaction");
                }

                // Ensure that all BudgetedTransaction Guids of incoming entity are empty to enable their (re)creation
                foreach (var budgetedTransaction in entity.BudgetedTransactions)
                {
                    budgetedTransaction.Id = Guid.Empty;
                }

                // Re-create BudgetedTransaction in DB
                budgetedTransactionRepository.CreateRange(entity.BudgetedTransactions);
            }

            // Update BankTransaction in DB
            bankTransactionRepository.Update(entity);

            return entity;
        }
        catch (EntityUpdateException e)
        {
            throw new ServiceException($"Unable to update Bank Transaction: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override IEnumerable<BankTransaction> ImportTransactions(IEnumerable<BankTransaction> entities)
    {
        using var dbConnection = CreateDbConnection();
        using var transaction = dbConnection.BeginTransaction();
        try
        {
            var bankTransactionRepository = CreateBaseRepository(dbConnection);
            var newTransactions = entities.ToList();
            bankTransactionRepository.CreateRange(newTransactions);
            transaction.Commit();
            return newTransactions;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to import Transactions: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}
