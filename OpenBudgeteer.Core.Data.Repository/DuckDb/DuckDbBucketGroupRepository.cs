using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbBucketGroupRepository : IBucketGroupRepository
{
    private readonly DbConnection _connection;

    public DuckDbBucketGroupRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<BucketGroup> All()
    {
        var sql = """
                  SELECT BucketGroupId AS Id, Name, Position FROM BucketGroup
                  """;
        return _connection.Query<BucketGroup>(sql).AsQueryable();
    }

    public IQueryable<BucketGroup> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      bg.BucketGroupId AS Id,
                      bg.Name,
                      bg.Position,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries
                  FROM BucketGroup bg
                  LEFT JOIN Bucket b ON bg.BucketGroupId = b.BucketGroupId
                  """;

        var mapper = new BucketGroupMapper();
        _ = _connection
            .Query<BucketGroup, Bucket?, BucketGroup>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public BucketGroup? ById(Guid id)
    {
        var sql = """
                  SELECT
                      BucketGroupId AS Id,
                      Name,
                      Position
                  FROM BucketGroup
                  WHERE BucketGroupId = $id
                  """;

        return _connection
            .Query<BucketGroup>(sql, param: new { id = id.ToString() })
            .FirstOrDefault();
    }

    public BucketGroup? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      bg.BucketGroupId AS Id,
                      bg.Name,
                      bg.Position,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries
                  FROM BucketGroup bg
                  LEFT JOIN Bucket b ON bg.BucketGroupId = b.BucketGroupId
                  WHERE bg.BucketGroupId = $id
                  """;

        var mapper = new BucketGroupMapper();
        _ = _connection
            .Query<BucketGroup, Bucket?, BucketGroup>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public int Create(BucketGroup entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO BucketGroup (BucketGroupId, Name, Position)
                  VALUES ($1, $2, $3)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Name ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.Position));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<BucketGroup> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BucketGroup entity)
    {
        var sql = """
                  UPDATE BucketGroup
                  SET Name = $1, Position = $2
                  WHERE BucketGroupId = $3
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Name ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.Position));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<BucketGroup> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        // Prevent deletion of system bucket group
        if (id == Guid.Parse("00000000-0000-0000-0000-000000000001")) return 0;

        // Consistency checks
        var entity = ByIdWithIncludedEntities(id);
        if (entity is null) throw new Exception($"BucketGroup with id {id} not found.");
        if (entity.Buckets is not null && entity.Buckets.Count != 0) throw new Exception($"Cannot delete a BucketGroup with Buckets assigned to it.");

        var sql = """
                  DELETE FROM BucketGroup WHERE BucketGroupId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        // Prevent deletion of system bucket group
        var cleansedEntities = ids
            .Where(i => i != Guid.Parse("00000000-0000-0000-0000-000000000001"))
            .ToList();

        // Consistency checks
        var entities = cleansedEntities.
            Select(ByIdWithIncludedEntities)
            .OfType<BucketGroup>().ToList();
        if (entities.Count == 0) throw new Exception($"No BucketGroup found with passed IDs.");
        if (entities.Any(i => i.Buckets != null && i.Buckets.Count != 0)) throw new Exception($"Cannot delete a BucketGroup with Buckets assigned to it.");

        return cleansedEntities.Sum(Delete);
    }
}
