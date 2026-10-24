using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Contracts.Services;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;
using OpenBudgeteer.Core.Data.Services.Exceptions;
using OpenBudgeteer.Core.Data.Services.Generic;

namespace OpenBudgeteer.Core.Data.Services.DuckDb;

public class DuckDbBucketService : GenericBucketService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBucketService> _logger;

    public DuckDbBucketService(
        Func<DbConnection> dbConnectionFactory,
        ILogger<DuckDbBucketService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBucketRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbBucketRepository(dbConnection);
    protected override IBucketVersionRepository CreateBucketVersionRepository(DbConnection dbConnection) => new DuckDbBucketVersionRepository(dbConnection);
    protected override IBudgetedTransactionRepository CreateBudgetedTransactionRepository(DbConnection dbConnection) => new DuckDbBudgetedTransactionRepository(dbConnection);
    protected override IBucketMovementRepository CreateBucketMovementRepository(DbConnection dbConnection) => new DuckDbBucketMovementRepository(dbConnection);
    protected override IBucketRuleSetRepository CreateBucketRuleSetRepository(DbConnection dbConnection) => new DuckDbBucketRuleSetRepository(dbConnection);

    public override IEnumerable<Bucket> GetActiveBuckets(DateOnly validFrom)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            // WHERE explanation
            // b.ValidFrom <= $validFrom --> Only valid Buckets of current month
            // b.IsInactive = false --> Only active Buckets
            // b.IsInactiveFrom > $validFrom --> Alternative: Bucket is inactive as of today, but was valid in current selected month
            var sql = """
                SELECT
                    b.BucketId AS Id
                    ,b.Name
                    ,b.BucketGroupId
                    ,b.ColorCode
                    ,b.TextColorCode
                    ,b.ValidFrom
                    ,b.IsInactive
                    ,b.IsInactiveFrom
                    ,b.IsHiddenFromSummaries
                    ,bv.BucketVersionId AS Id
                    ,bv.BucketId
                    ,bv.Version
                    ,bv.BucketType
                    ,bv.BucketTypeXParam
                    ,bv.BucketTypeYParam
                    ,bv.BucketTypeZParam
                    ,bv.Notes
                    ,bv.ValidFrom
                FROM Bucket b
                LEFT JOIN BucketVersion bv ON b.BucketId = bv.BucketId
                WHERE
                    b.ValidFrom <= $validFrom
                    AND (b.IsInactive = false OR b.IsInactiveFrom > $validFrom)
                ORDER BY b.Name
                """;

            var mapper = new BucketMapper();
            _ = dbConnection
                .Query<Bucket, BucketVersion?, Bucket>(
                    sql,
                    mapper.MapWithVersion,
                    new { validFrom = validFrom.ToDateTime(TimeOnly.MinValue) },
                    splitOn: "Id")
                .ToList();

            // TODO Check if this can be done in SQL directly
            foreach (var bucket in mapper.Results)
            {
                bucket.CurrentVersion = bucket.BucketVersions?
                    .Where(v => v.ValidFrom <= validFrom)
                    .OrderByDescending(v => v.ValidFrom)
                    .FirstOrDefault();
            }

            return mapper.Results.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override BucketVersion GetLatestVersion(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            // Get the latest version for a specific bucket that's valid for the given month
            var sql = """
                SELECT
                    BucketVersionId AS Id
                    ,BucketId
                    ,Version
                    ,BucketType
                    ,BucketTypeXParam
                    ,BucketTypeYParam
                    ,BucketTypeZParam
                    ,Notes
                    ,ValidFrom
                FROM BucketVersion
                WHERE
                    BucketId = $bucketId
                    AND ValidFrom <= $yearMonth
                ORDER BY ValidFrom DESC
                LIMIT 1
                """;

            var result = dbConnection.QueryFirstOrDefault<BucketVersion>(
                sql,
                new
                {
                    bucketId = bucketId.ToString(),
                    yearMonth = yearMonth.ToDateTime(TimeOnly.MinValue)
                });

            if (result is null)
                throw new EntityNotFoundException("Unable to find Bucket with the given id and month.");

            return result;
        }
        catch (EntityNotFoundException e)
        {
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw;
        }
    }

    public override decimal GetBalance(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            var endOfMonth = yearMonth.AddMonths(1);

            var sql = """
                      SELECT
                           COALESCE((SELECT SUM(bt.Amount) FROM BudgetedTransaction bt
                                     INNER JOIN BankTransaction t ON bt.TransactionId = t.TransactionId
                                     WHERE bt.BucketId = $bucketId AND t.TransactionDate < $endOfMonth), 0)
                           +
                           COALESCE((SELECT SUM(Amount) FROM BucketMovement
                                     WHERE BucketId = $bucketId AND MovementDate < $endOfMonth), 0)
                      """;

            return dbConnection.ExecuteScalar<decimal>(
                sql,
                new
                {
                    bucketId = bucketId.ToString(),
                    endOfMonth = endOfMonth.ToDateTime(TimeOnly.MinValue)
                });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override BucketFigures GetFigures(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            var endOfMonth = yearMonth.AddMonths(1);
            var startOfMonth = new DateOnly(yearMonth.Year, yearMonth.Month, 1);

            // Calculate balance
            var balanceSql = """
                SELECT
                    COALESCE((
                        SELECT SUM(bt.Amount)
                        FROM BudgetedTransaction bt
                        INNER JOIN BankTransaction t ON bt.TransactionId = t.TransactionId
                        WHERE bt.BucketId = $bucketId AND t.TransactionDate < $endOfMonth
                    ), 0) +
                    COALESCE((
                        SELECT SUM(Amount)
                        FROM BucketMovement
                        WHERE BucketId = $bucketId AND MovementDate < $endOfMonth
                    ), 0) AS Balance
                """;

            var balance = dbConnection.ExecuteScalar<decimal>(
                balanceSql,
                new
                {
                    bucketId = bucketId.ToString(),
                    endOfMonth = endOfMonth.ToDateTime(TimeOnly.MinValue)
                });

            // Calculate In & Out
            // TODO: Consider replacing with GetInAndOut(Guid bucketId, DateOnly yearMonth)
            var inOutSql = """
                SELECT
                    COALESCE(SUM(CASE WHEN Amount >= 0 THEN Amount ELSE 0 END), 0) AS Input,
                    COALESCE(SUM(CASE WHEN Amount < 0 THEN Amount ELSE 0 END), 0) AS Output
                FROM (
                    SELECT bt.Amount
                    FROM BudgetedTransaction bt
                    INNER JOIN BankTransaction t ON bt.TransactionId = t.TransactionId
                    WHERE bt.BucketId = $bucketId
                        AND t.TransactionDate >= $startOfMonth
                        AND t.TransactionDate < $endOfMonth
                    UNION ALL
                    SELECT Amount
                    FROM BucketMovement
                    WHERE BucketId = $bucketId
                        AND MovementDate >= $startOfMonth
                        AND MovementDate < $endOfMonth
                ) AS combined
                """;

            var inOut = dbConnection.QueryFirstOrDefault<(decimal Input, decimal Output)>(
                inOutSql,
                new
                {
                    bucketId = bucketId.ToString(),
                    startOfMonth = startOfMonth.ToDateTime(TimeOnly.MinValue),
                    endOfMonth = endOfMonth.ToDateTime(TimeOnly.MinValue)
                });

            return new BucketFigures(balance, inOut.Input, inOut.Output);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override BucketFigures GetInAndOut(Guid bucketId, DateOnly yearMonth)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            var endOfMonth = yearMonth.AddMonths(1);
            var startOfMonth = new DateOnly(yearMonth.Year, yearMonth.Month, 1);

            var sql = """
                SELECT
                    COALESCE(SUM(CASE WHEN Amount >= 0 THEN Amount ELSE 0 END), 0) AS Input,
                    COALESCE(SUM(CASE WHEN Amount < 0 THEN Amount ELSE 0 END), 0) AS Output
                FROM (
                    SELECT bt.Amount
                    FROM BudgetedTransaction bt
                    INNER JOIN BankTransaction t ON bt.TransactionId = t.TransactionId
                    WHERE bt.BucketId = $bucketId
                        AND t.TransactionDate >= $startOfMonth
                        AND t.TransactionDate < $endOfMonth
                    UNION ALL
                    SELECT Amount
                    FROM BucketMovement
                    WHERE BucketId = $bucketId
                        AND MovementDate >= $startOfMonth
                        AND MovementDate < $endOfMonth
                ) AS combined
                """;

            var result = dbConnection.QueryFirstOrDefault<(decimal Input, decimal Output)>(
                sql,
                new
                {
                    bucketId = bucketId.ToString(),
                    startOfMonth = startOfMonth.ToDateTime(TimeOnly.MinValue),
                    endOfMonth = endOfMonth.ToDateTime(TimeOnly.MinValue)
                });

            return new BucketFigures(null, result.Input, result.Output);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override Bucket Create(Bucket entity)
    {
        using var dbContext = CreateDbConnection();
        using var transaction = dbContext.BeginTransaction();
        try
        {
            var baseRepository = CreateBaseRepository(dbContext);
            if (entity.CurrentVersion is null) throw new EntityUpdateException("No Bucket Version defined.");

            entity.CurrentVersion.Version = 1;
            entity.BucketVersions = new List<BucketVersion>();
            entity.BucketVersions.Add(entity.CurrentVersion);

            baseRepository.Create(entity);
            transaction.Commit();
            return entity;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to create Bucket: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override Bucket Update(Bucket entity)
    {
        using var dbContext = CreateDbConnection();
        using var transaction = dbContext.BeginTransaction();
        try
        {
            var baseRepository = CreateBaseRepository(dbContext);
            var bucketVersionRepository = CreateBucketVersionRepository(dbContext);

            if (entity.CurrentVersion is not null)
            {
                entity.BucketVersions = new List<BucketVersion>();
                if (entity.Id == Guid.Empty)
                {
                    // New Bucket - Create new Version
                    var newVersion = entity.CurrentVersion;
                    newVersion.Id = Guid.Empty;
                    newVersion.Version = 1;
                    entity.BucketVersions.Add(newVersion);
                }
                else
                {
                    var latestVersion = GetLatestVersion(entity.Id, DateOnly.FromDateTime(DateTime.Today));
                    if (entity.CurrentVersion.ValidFrom == latestVersion.ValidFrom)
                    {
                        // Change in same month, overwrite latest Version
                        latestVersion.BucketType = entity.CurrentVersion.BucketType;
                        latestVersion.BucketTypeXParam = entity.CurrentVersion.BucketTypeXParam;
                        latestVersion.BucketTypeYParam = entity.CurrentVersion.BucketTypeYParam;
                        latestVersion.BucketTypeZParam = entity.CurrentVersion.BucketTypeZParam;
                        latestVersion.Notes = entity.CurrentVersion.Notes;

                        bucketVersionRepository.Update(latestVersion);
                        entity.BucketVersions.Add(latestVersion);
                    }
                    else
                    {
                        // Create new Version
                        var newVersion = entity.CurrentVersion;
                        newVersion.Id = Guid.Empty;
                        newVersion.Version = latestVersion.Version + 1;

                        bucketVersionRepository.Create(newVersion);
                        entity.BucketVersions.Add(newVersion);
                    }
                }
            }

            baseRepository.Update(entity);
            transaction.Commit();
            return entity;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to update Bucket: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }

    public override void Delete(Guid id)
    {
        using var dbContext = CreateDbConnection();
        using var transaction = dbContext.BeginTransaction();
        try
        {
            var baseRepository = CreateBaseRepository(dbContext);
            var bucketRuleSetRepository = CreateBucketRuleSetRepository(dbContext);

            // Use EXISTS check instead of loading all records
            var existsCheckSql = """
                SELECT
                    EXISTS(SELECT 1 FROM BudgetedTransaction WHERE BucketId = $id) OR
                    EXISTS(SELECT 1 FROM BucketMovement WHERE BucketId = $id)
                """;
            var hasAssignments = dbContext.ExecuteScalar<bool>(existsCheckSql, new { id = id.ToString() });

            if (hasAssignments)
            {
                throw new EntityUpdateException("Cannot delete a Bucket with assigned Transactions or Bucket Movements.");
            }

            // Delete Bucket
            baseRepository.Delete(id);

            // Delete all BucketRuleSet which refer to this Bucket using parameterized query
            var ruleSetIdsSql = """
                SELECT BucketRuleSetId AS Id
                FROM BucketRuleSet
                WHERE TargetBucketId = $id
                """;
            var bucketRuleSetIds = dbContext
                .Query<Guid>(
                    ruleSetIdsSql,
                    new { id = id.ToString() })
                .ToList();
            if (bucketRuleSetIds.Count != 0) bucketRuleSetRepository.DeleteRange(bucketRuleSetIds);

            transaction.Commit();
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to delete Bucket: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}
