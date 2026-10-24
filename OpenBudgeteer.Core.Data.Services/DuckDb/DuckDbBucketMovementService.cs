using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb;
using OpenBudgeteer.Core.Data.Services.Exceptions;
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

    public override IEnumerable<BucketMovement> GetAll(DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            var sql = """
                SELECT
                    BucketMovementId AS Id
                    ,BucketId
                    ,Amount
                    ,MovementDate
                FROM BucketMovement
                WHERE MovementDate >= $periodStart AND MovementDate <= $periodEnd
                """;

            return dbConnection
                .Query<BucketMovement>(
                    sql,
                    new
                    {
                        periodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                        periodEnd = periodEnd.ToDateTime(TimeOnly.MinValue)
                    })
                .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BucketMovement> GetAllFromBucket(Guid bucketId, DateOnly periodStart, DateOnly periodEnd)
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            var sql = """
                SELECT
                    BucketMovementId AS Id
                    ,BucketId
                    ,Amount
                    ,MovementDate
                FROM BucketMovement
                WHERE MovementDate >= $periodStart
                    AND MovementDate <= $periodEnd
                    AND BucketId = $bucketId
                """;

            return dbConnection
                .Query<BucketMovement>(
                    sql,
                    new
                    {
                        periodStart = periodStart.ToDateTime(TimeOnly.MinValue),
                        periodEnd = periodEnd.ToDateTime(TimeOnly.MinValue),
                        bucketId = bucketId.ToString()
                    })
                .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }
}
