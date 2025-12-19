using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

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

        var bucketGroupDict = new Dictionary<Guid, BucketGroup>();
        _connection.Query<BucketGroup, Bucket?, BucketGroup>(
            sql,
            (bucketGroup, bucket) =>
            {
                if (!bucketGroupDict.TryGetValue(bucketGroup.Id, out var existingBucketGroup))
                {
                    existingBucketGroup = bucketGroup;
                    bucketGroupDict.Add(bucketGroup.Id, existingBucketGroup);
                }

                if (bucket == null) return existingBucketGroup;
                existingBucketGroup.Buckets ??= new List<Bucket>();
                existingBucketGroup.Buckets.Add(bucket);
                return existingBucketGroup;
            },
            splitOn: "Id");

        return bucketGroupDict.Values.AsQueryable();
    }

    public BucketGroup? ById(Guid id)
    {
        var sql = """
                  SELECT BucketGroupId AS Id, Name, Position
                  FROM BucketGroup
                  WHERE BucketGroupId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new BucketGroup
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                Position = reader.GetInt32(2)
            };
        }
        return null;
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
                  WHERE bg.BucketGroupId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        BucketGroup? bucketGroup = null;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            bucketGroup ??= new BucketGroup
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                Position = reader.GetInt32(2)
            };

            if (reader.IsDBNull(3)) continue;
            bucketGroup.Buckets ??= new List<Bucket>();
            bucketGroup.Buckets.Add(new Bucket
            {
                Id = Guid.Parse(reader.GetString(3)),
                Name = reader.IsDBNull(4) ? null : reader.GetString(4),
                BucketGroupId = Guid.Parse(reader.GetString(5)),
                ColorCode = reader.IsDBNull(6) ? null : reader.GetString(6),
                TextColorCode = reader.IsDBNull(7) ? null : reader.GetString(7),
                ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(8)),
                IsInactive = reader.GetBoolean(9),
                IsInactiveFrom = DateOnly.FromDateTime(reader.GetDateTime(10)),
                IsHiddenFromSummaries = reader.GetBoolean(11)
            });
        }
        return bucketGroup;
    }

    public int Create(BucketGroup entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

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
