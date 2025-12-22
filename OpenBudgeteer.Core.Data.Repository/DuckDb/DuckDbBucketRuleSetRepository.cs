using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbBucketRuleSetRepository : IBucketRuleSetRepository
{
    private readonly DbConnection _connection;

    public DuckDbBucketRuleSetRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<BucketRuleSet> All()
    {
        var sql = """
                  SELECT BucketRuleSetId AS Id, Priority, Name, TargetBucketId
                  FROM BucketRuleSet
                  """;
        return _connection.Query<BucketRuleSet>(sql).AsQueryable();
    }

    public IQueryable<BucketRuleSet> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      brs.BucketRuleSetId AS Id,
                      brs.Priority,
                      brs.Name,
                      brs.TargetBucketId,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries,
                      mr.MappingRuleId AS Id,
                      mr.BucketRuleSetId,
                      mr.ComparisonField,
                      mr.ComparisonType,
                      mr.ComparisonValue
                  FROM BucketRuleSet brs
                  INNER JOIN Bucket b ON brs.TargetBucketId = b.BucketId
                  LEFT JOIN MappingRule mr ON brs.BucketRuleSetId = mr.BucketRuleSetId
                  """;

        var mapper = new BucketRuleSetMapper();
        _ = _connection
            .Query<BucketRuleSet, Bucket, MappingRule?, BucketRuleSet>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id,Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public BucketRuleSet? ById(Guid id)
    {
        var sql = """
                  SELECT
                      BucketRuleSetId AS Id,
                      Priority,
                      Name,
                      TargetBucketId
                  FROM BucketRuleSet
                  WHERE BucketRuleSetId = $id
                  """;

        return _connection
            .Query<BucketRuleSet>(sql, param: new { id = id.ToString() })
            .FirstOrDefault();
    }

    public BucketRuleSet? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      brs.BucketRuleSetId AS Id,
                      brs.Priority,
                      brs.Name,
                      brs.TargetBucketId,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries,
                      mr.MappingRuleId AS Id,
                      mr.BucketRuleSetId,
                      mr.ComparisonField,
                      mr.ComparisonType,
                      mr.ComparisonValue
                  FROM BucketRuleSet brs
                  INNER JOIN Bucket b ON brs.TargetBucketId = b.BucketId
                  LEFT JOIN MappingRule mr ON brs.BucketRuleSetId = mr.BucketRuleSetId
                  WHERE brs.BucketRuleSetId = $id
                  """;

        var mapper = new BucketRuleSetMapper();
        _ = _connection
            .Query<BucketRuleSet, Bucket, MappingRule?, BucketRuleSet>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id,Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public int Create(BucketRuleSet entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO BucketRuleSet (BucketRuleSetId, Priority, Name, TargetBucketId)
                  VALUES ($1, $2, $3, $4)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.Priority));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Name ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.TargetBucketId.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<BucketRuleSet> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BucketRuleSet entity)
    {
        var sql = """
                  UPDATE BucketRuleSet
                  SET Priority = $1, Name = $2, TargetBucketId = $3
                  WHERE BucketRuleSetId = $4
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Priority));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Name ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.TargetBucketId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<BucketRuleSet> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        // Consistency checks
        var entity = ByIdWithIncludedEntities(id);
        if (entity is null) throw new Exception($"BucketRuleSet with id {id} not found.");

        var result = 0;

        // Delete related MappingRules
        if (entity.MappingRules is not null && entity.MappingRules.Count > 0)
        {
            var mappingRuleRepository = new DuckDbMappingRuleRepository(_connection);
            result += mappingRuleRepository.DeleteRange(entity.MappingRules.Select(i => i.Id));
        }

        // Delete BucketRuleSet
        var sql = """
                  DELETE FROM BucketRuleSet WHERE BucketRuleSetId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        result += cmd.ExecuteNonQuery();

        return result;
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        // Consistency checks
        var scope = ids.ToList();
        var entities = scope.Select(ById).ToList();
        if (entities.Count == 0) throw new Exception("No BucketRuleSets found with passed IDs.");

        return scope.Sum(Delete);
    }
}
