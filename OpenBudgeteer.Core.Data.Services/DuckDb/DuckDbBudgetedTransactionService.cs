using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbBudgetedTransactionService : GenericBudgetedTransactionService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBudgetedTransactionService> _logger;

    // System bucket IDs
    private readonly string _incomeBucketId = "00000000-0000-0000-0000-000000000001";
    private readonly string _transferBucketId = "00000000-0000-0000-0000-000000000002";

    public DuckDbBudgetedTransactionService(
        Func<DbConnection> dbConnectionFactory,
        ILogger<DuckDbBudgetedTransactionService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBudgetedTransactionRepository CreateBaseRepository(DbConnection dbConnection) =>
        new DuckDbBudgetedTransactionRepository(dbConnection);

    private const string ALL_WITH_TRANSACTIONS_QUERY =
        """
        SELECT
            bt.BudgetedTransactionId AS Id
            ,bt.TransactionId
            ,bt.BucketId
            ,bt.Amount
            ,b.BucketId AS Id
            ,b.Name
            ,b.BucketGroupId
            ,b.ColorCode
            ,b.TextColorCode
            ,b.ValidFrom
            ,b.IsInactive
            ,b.IsInactiveFrom
            ,b.IsHiddenFromSummaries
            ,t.TransactionId AS Id
            ,t.AccountId
            ,t.TransactionDate
            ,t.Payee
            ,t.Memo
            ,t.Amount
            ,a.AccountId AS Id
            ,a.Name
            ,a.IsActive
        FROM BudgetedTransaction bt
        INNER JOIN Bucket b ON bt.BucketId = b.BucketId
        INNER JOIN BankTransaction t ON bt.TransactionId = t.TransactionId
        INNER JOIN Account a ON t.AccountId = a.AccountId
        """;

    private IEnumerable<BudgetedTransaction> ExecuteAllWithTransactionsQuery(
        DbConnection dbConnection,
        string sql,
        object parameters)
    {
        var mapper = new BudgetedTransactionMapper();
        _ = dbConnection
            .Query<BudgetedTransaction, Bucket, BankTransaction, Account, BudgetedTransaction>(
                sql,
                mapper.MapWithEverythingIncludingAccount,
                param: parameters,
                splitOn: "Id,Id,Id")
            .ToList();

        return mapper.Results.ToList();
    }

    public override IEnumerable<BudgetedTransaction> GetAll(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var sql = $"""
                       {ALL_WITH_TRANSACTIONS_QUERY}
                       WHERE t.TransactionDate >= $periodStart AND t.TransactionDate <= $periodEnd
                       """;

            return ExecuteAllWithTransactionsQuery(dbConnection, sql, new
            {
                periodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                periodEnd = periodEnd.ToDateTime(TimeOnly.MinValue)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BudgetedTransaction> GetAllForReporting(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var sql = $"""
                       {ALL_WITH_TRANSACTIONS_QUERY}
                       WHERE
                           t.TransactionDate >= $periodStart AND t.TransactionDate <= $periodEnd
                           AND b.IsHiddenFromSummaries = false
                       """;

            return ExecuteAllWithTransactionsQuery(dbConnection, sql, new
            {
                periodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                periodEnd = periodEnd.ToDateTime(TimeOnly.MinValue)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BudgetedTransaction> GetAllFromTransaction(Guid transactionId, DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var sql = $"""
                       {ALL_WITH_TRANSACTIONS_QUERY}
                       WHERE
                           t.TransactionDate >= $periodStart AND t.TransactionDate <= $periodEnd
                           AND bt.TransactionId = $transactionId
                       """;

            return ExecuteAllWithTransactionsQuery(dbConnection, sql, new
            {
                periodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                periodEnd = periodEnd.ToDateTime(TimeOnly.MinValue),
                transactionId = transactionId.ToString()
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BudgetedTransaction> GetAllFromBucket(Guid bucketId, DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var sql = $"""
                {ALL_WITH_TRANSACTIONS_QUERY}
                WHERE
                    t.TransactionDate >= $periodStart AND t.TransactionDate <= $periodEnd
                    AND bt.BucketId = $bucketId
                ORDER BY t.TransactionDate DESC
                """;

            return ExecuteAllWithTransactionsQuery(dbConnection, sql, new
            {
                periodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                periodEnd = periodEnd.ToDateTime(TimeOnly.MinValue),
                bucketId = bucketId.ToString()
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BudgetedTransaction> GetAllNonTransfer(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var sql = $"""
                       {ALL_WITH_TRANSACTIONS_QUERY}
                       WHERE
                           t.TransactionDate >= $periodStart AND t.TransactionDate <= $periodEnd
                           AND bt.BucketId != $transferBucketId
                       """;

            return ExecuteAllWithTransactionsQuery(dbConnection, sql, new
            {
                periodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                periodEnd = periodEnd.ToDateTime(TimeOnly.MinValue),
                transferBucketId = _transferBucketId
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BudgetedTransaction> GetAllTransfer(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var sql = $"""
                       {ALL_WITH_TRANSACTIONS_QUERY}
                       WHERE
                           t.TransactionDate >= $periodStart AND t.TransactionDate <= $periodEnd
                           AND bt.BucketId = $transferBucketId
                       """;

            return ExecuteAllWithTransactionsQuery(dbConnection, sql, new
            {
                periodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                periodEnd = periodEnd.ToDateTime(TimeOnly.MinValue),
                transferBucketId = _transferBucketId
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BudgetedTransaction> GetAllIncome(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();
            var sql = $"""
                       {ALL_WITH_TRANSACTIONS_QUERY}
                       WHERE
                           t.TransactionDate >= $periodStart AND t.TransactionDate <= $periodEnd
                           AND bt.BucketId = $incomeBucketId
                       """;

            return ExecuteAllWithTransactionsQuery(dbConnection, sql, new
            {
                periodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                periodEnd = periodEnd.ToDateTime(TimeOnly.MinValue),
                incomeBucketId = _incomeBucketId
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }
}
