using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

/// <summary>
/// DuckDB-specific BucketVersion Repository using raw Dapper with positional parameters
/// </summary>
public class DuckDbBucketVersionRepository : IBucketVersionRepository
{
    private readonly DbConnection _connection;

    public DuckDbBucketVersionRepository(DbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public IQueryable<BucketVersion> All()
    {
        var sql = """
                  SELECT BucketVersionId AS Id, BucketId, Version, BucketType, BucketTypeXParam, BucketTypeYParam, BucketTypeZParam, Notes, ValidFrom
                  FROM BucketVersion
                  """;
        return _connection.Query<BucketVersion>(sql).AsQueryable();
    }

    public IQueryable<BucketVersion> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      bv.BucketVersionId AS Id,
                      bv.BucketId,
                      bv.Version,
                      bv.BucketType,
                      bv.BucketTypeXParam,
                      bv.BucketTypeYParam,
                      bv.BucketTypeZParam,
                      bv.Notes,
                      bv.ValidFrom,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries
                  FROM BucketVersion bv
                  INNER JOIN Bucket b ON bv.BucketId = b.BucketId
                  """;

        var mapper = new BucketVersionMapper();
        _ = _connection
            .Query<BucketVersion, Bucket, BucketVersion>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public BucketVersion? ById(Guid id)
    {
        var sql = """
                  SELECT
                      BucketVersionId AS Id,
                      BucketId,
                      Version,
                      BucketType,
                      BucketTypeXParam,
                      BucketTypeYParam,
                      BucketTypeZParam,
                      Notes,
                      ValidFrom
                  FROM BucketVersion
                  WHERE BucketVersionId = $id
                  """;

        return _connection
            .Query<BucketVersion>(sql, param: new { id = id.ToString() })
            .FirstOrDefault();
    }

    public BucketVersion? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      bv.BucketVersionId AS Id,
                      bv.BucketId,
                      bv.Version,
                      bv.BucketType,
                      bv.BucketTypeXParam,
                      bv.BucketTypeYParam,
                      bv.BucketTypeZParam,
                      bv.Notes,
                      bv.ValidFrom,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries
                  FROM BucketVersion bv
                  INNER JOIN Bucket b ON bv.BucketId = b.BucketId
                  WHERE bv.BucketVersionId = $id
                  """;

        var mapper = new BucketVersionMapper();
        _ = _connection
            .Query<BucketVersion, Bucket, BucketVersion>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public int Create(BucketVersion entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO BucketVersion (BucketVersionId, BucketId, Version, BucketType, BucketTypeXParam, BucketTypeYParam, BucketTypeZParam, Notes, ValidFrom)
                  VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.Version));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketType));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketTypeXParam));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketTypeYParam));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketTypeZParam.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Notes ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.ValidFrom.ToDateTime(TimeOnly.MinValue)));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<BucketVersion> entities)
    {
        var count = 0;
        foreach (var entity in entities)
        {
            count += Create(entity);
        }
        return count;
    }

    public int Update(BucketVersion entity)
    {
        var sql = """
                  UPDATE BucketVersion
                  SET BucketId = $1, Version = $2, BucketType = $3, BucketTypeXParam = $4, BucketTypeYParam = $5, BucketTypeZParam = $6, Notes = $7, ValidFrom = $8
                  WHERE BucketVersionId = $9
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.BucketId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.Version));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketType));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketTypeXParam));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketTypeYParam));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketTypeZParam.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Notes ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.ValidFrom.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<BucketVersion> entities)
    {
        var count = 0;
        foreach (var entity in entities)
        {
            count += Update(entity);
        }
        return count;
    }

    public int Delete(Guid id)
    {
        var sql = """
                  DELETE FROM BucketVersion WHERE BucketVersionId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        var count = 0;
        foreach (var id in ids)
        {
            count += Delete(id);
        }
        return count;
    }
}
