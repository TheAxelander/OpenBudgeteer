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

public class DuckDbBucketGroupService : GenericBucketGroupService<DbConnection>
{
    private readonly Func<DbConnection> _dbConnectionFactory;
    private readonly ILogger<DuckDbBucketGroupService> _logger;

    private readonly string _systemBucketGroupId = "00000000-0000-0000-0000-000000000001";

    public DuckDbBucketGroupService(
        Func<DbConnection> dbConnectionFactory,
        ILogger<DuckDbBucketGroupService> logger) : base(logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override DbConnection CreateDbConnection() => _dbConnectionFactory();
    protected override IBucketGroupRepository CreateBaseRepository(DbConnection dbConnection) => new DuckDbBucketGroupRepository(dbConnection);

    public override IEnumerable<BucketGroup> GetAll()
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            // Exclude system bucket group and order by position in SQL
            var sql = """
                SELECT
                    bg.BucketGroupId AS Id
                    ,bg.Name
                    ,bg.Position
                    ,b.BucketId AS Id
                    ,b.Name
                    ,b.BucketGroupId
                    ,b.ColorCode
                    ,b.TextColorCode
                    ,b.ValidFrom
                    ,b.IsInactive
                    ,b.IsInactiveFrom
                    ,b.IsHiddenFromSummaries
                FROM BucketGroup bg
                LEFT JOIN Bucket b ON bg.BucketGroupId = b.BucketGroupId
                WHERE bg.BucketGroupId != $systemId
                ORDER BY bg.Position
                """;

            var mapper = new BucketGroupMapper();
            _ = dbConnection
                .Query<BucketGroup, Bucket?, BucketGroup>(
                    sql,
                    mapper.MapWithEverything,
                    new { systemId = _systemBucketGroupId },
                    splitOn: "Id")
                .ToList();

            return mapper.Results.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BucketGroup> GetAllFull()
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            var sql = """
                SELECT
                    bg.BucketGroupId AS Id
                    ,bg.Name
                    ,bg.Position
                    ,b.BucketId AS Id
                    ,b.Name
                    ,b.BucketGroupId
                    ,b.ColorCode
                    ,b.TextColorCode
                    ,b.ValidFrom
                    ,b.IsInactive
                    ,b.IsInactiveFrom
                    ,b.IsHiddenFromSummaries
                FROM BucketGroup bg
                LEFT JOIN Bucket b ON bg.BucketGroupId = b.BucketGroupId
                ORDER BY bg.Position
                """;

            var mapper = new BucketGroupMapper();
            _ = dbConnection
                .Query<BucketGroup, Bucket?, BucketGroup>(
                    sql,
                    mapper.MapWithEverything,
                    splitOn: "Id")
                .ToList();

            return mapper.Results.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override IEnumerable<BucketGroup> GetSystemBucketGroups()
    {
        try
        {
            using var dbConnection = CreateDbConnection();

            // ORDER BY bg.Position --> In case in future there are multiple groups
            var sql = """
                SELECT
                    bg.BucketGroupId AS Id
                    ,bg.Name
                    ,bg.Position
                    ,b.BucketId AS Id
                    ,b.Name
                    ,b.BucketGroupId
                    ,b.ColorCode
                    ,b.TextColorCode
                    ,b.ValidFrom
                    ,b.IsInactive
                    ,b.IsInactiveFrom
                    ,b.IsHiddenFromSummaries
                FROM BucketGroup bg
                LEFT JOIN Bucket b ON bg.BucketGroupId = b.BucketGroupId
                WHERE bg.BucketGroupId = $systemId
                ORDER BY bg.Position
                """;

            var mapper = new BucketGroupMapper();
            _ = dbConnection
                .Query<BucketGroup, Bucket?, BucketGroup>(
                    sql,
                    mapper.MapWithEverything,
                    new { systemId = _systemBucketGroupId },
                    splitOn: "Id")
                .ToList();

            return mapper.Results.ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error on querying database.");
            throw new ServiceException($"Error on querying database: {e.Message}", _logger);
        }
    }

    public override BucketGroup Move(Guid bucketGroupId, int positions)
    {
        using var dbConnection = CreateDbConnection();
        using var transaction = dbConnection.BeginTransaction();
        try
        {
            var bucketGroupRepository = CreateBaseRepository(dbConnection);
            var (bucketGroup, updatedBucketGroups) = HandleMovement(bucketGroupId, positions);

            if (updatedBucketGroups.Any()) bucketGroupRepository.UpdateRange(updatedBucketGroups);
            transaction.Commit();
            return bucketGroup;
        }
        catch (EntityUpdateException e)
        {
            transaction.Rollback();
            throw new ServiceException($"Unable to move Bucket Group: {e.Message}", _logger);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Error during database update.");
            throw;
        }
    }
}
