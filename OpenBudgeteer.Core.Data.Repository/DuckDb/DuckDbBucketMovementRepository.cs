using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbBucketMovementRepository : IBucketMovementRepository
{
    private readonly DbConnection _connection;

    public DuckDbBucketMovementRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<BucketMovement> All()
    {
        var sql = """
                  SELECT
                      BucketMovementId AS Id
                      ,BucketId
                      ,Amount
                      ,MovementDate
                  FROM BucketMovement
                  """;
        return _connection.Query<BucketMovement>(sql).AsQueryable();
    }

    public IQueryable<BucketMovement> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      bm.BucketMovementId AS Id
                      ,bm.BucketId
                      ,bm.Amount
                      ,bm.MovementDate
                      ,b.BucketId AS Id
                      ,b.Name
                      ,b.BucketGroupId
                      ,b.ColorCode
                      ,b.TextColorCode
                      ,b.ValidFrom
                      ,b.IsInactive
                      ,b.IsInactiveFrom
                      ,b.IsHiddenFromSummaries
                  FROM BucketMovement bm
                  INNER JOIN Bucket b ON bm.BucketId = b.BucketId
                  """;

        var result = _connection.Query<BucketMovement, Bucket, BucketMovement>(
            sql,
            (bucketMovement, bucket) =>
            {
                bucketMovement.Bucket = bucket;
                return bucketMovement;
            },
            splitOn: "Id");

        return result.AsQueryable();
    }

    public BucketMovement? ById(Guid id)
    {
        var sql = """
                  SELECT
                      BucketMovementId AS Id
                      ,BucketId
                      ,Amount
                      ,MovementDate
                  FROM BucketMovement
                  WHERE BucketMovementId = $id
                  """;

        return _connection
            .Query<BucketMovement>(sql, param: new { id = id.ToString() })
            .FirstOrDefault();
    }

    public BucketMovement? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      bm.BucketMovementId AS Id
                      ,bm.BucketId
                      ,bm.Amount
                      ,bm.MovementDate
                      ,b.BucketId AS Id
                      ,b.Name
                      ,b.BucketGroupId
                      ,b.ColorCode
                      ,b.TextColorCode
                      ,b.ValidFrom
                      ,b.IsInactive
                      ,b.IsInactiveFrom
                      ,b.IsHiddenFromSummaries
                  FROM BucketMovement bm
                  INNER JOIN Bucket b ON bm.BucketId = b.BucketId
                  WHERE bm.BucketMovementId = $id
                  """;

        var mapper = new BucketMovementMapper();
        _ = _connection
            .Query<BucketMovement, Bucket, BucketMovement>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public int Create(BucketMovement entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO BucketMovement
                      (BucketMovementId
                       ,BucketId
                       ,Amount
                       ,MovementDate)
                  VALUES ($1, $2, $3, $4)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.Amount));
        cmd.Parameters.Add(new DuckDBParameter(entity.MovementDate.ToDateTime(TimeOnly.MinValue)));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<BucketMovement> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BucketMovement entity)
    {
        var sql = """
                  UPDATE BucketMovement
                  SET
                      BucketId = $1
                      ,Amount = $2
                      ,MovementDate = $3
                  WHERE BucketMovementId = $4
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.BucketId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.Amount));
        cmd.Parameters.Add(new DuckDBParameter(entity.MovementDate.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<BucketMovement> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        // Consistency checks
        var entity = ById(id);
        if (entity is null) throw new Exception($"BucketMovement with id {id} not found.");

        var sql = """
                  DELETE
                  FROM BucketMovement
                  WHERE BucketMovementId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        // Consistency checks
        var scope = ids.ToList();
        var entities = scope.Select(ById).ToList();
        if (entities.Count == 0) throw new Exception($"No BucketMovement found with passed IDs.");

        return scope.Sum(Delete);
    }
}
