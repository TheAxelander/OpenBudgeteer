using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbMappingRuleRepository : IMappingRuleRepository
{
    private readonly DbConnection _connection;

    public DuckDbMappingRuleRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<MappingRule> All()
    {
        var sql = """
                  SELECT MappingRuleId AS Id, BucketRuleSetId, ComparisonField, ComparisonType, ComparisonValue
                  FROM MappingRule
                  """;
        return _connection.Query<MappingRule>(sql).AsQueryable();
    }

    public IQueryable<MappingRule> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      mr.MappingRuleId AS Id,
                      mr.BucketRuleSetId,
                      mr.ComparisonField,
                      mr.ComparisonType,
                      mr.ComparisonValue,
                      brs.BucketRuleSetId AS Id,
                      brs.Priority,
                      brs.Name,
                      brs.TargetBucketId
                  FROM MappingRule mr
                  INNER JOIN BucketRuleSet brs ON mr.BucketRuleSetId = brs.BucketRuleSetId
                  """;

        var mapper = new MappingRuleMapper();
        _ = _connection
            .Query<MappingRule, BucketRuleSet, MappingRule>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public MappingRule? ById(Guid id)
    {
        var sql = """
                  SELECT
                      MappingRuleId AS Id,
                      BucketRuleSetId,
                      ComparisonField,
                      ComparisonType,
                      ComparisonValue
                  FROM MappingRule
                  WHERE MappingRuleId = $id
                  """;

        return _connection
            .Query<MappingRule>(sql, param: new { id = id.ToString() })
            .FirstOrDefault();
    }

    public MappingRule? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      mr.MappingRuleId AS Id,
                      mr.BucketRuleSetId,
                      mr.ComparisonField,
                      mr.ComparisonType,
                      mr.ComparisonValue,
                      brs.BucketRuleSetId AS Id,
                      brs.Priority,
                      brs.Name,
                      brs.TargetBucketId
                  FROM MappingRule mr
                  INNER JOIN BucketRuleSet brs ON mr.BucketRuleSetId = brs.BucketRuleSetId
                  WHERE mr.MappingRuleId = $id
                  """;

        var mapper = new MappingRuleMapper();
        _ = _connection
            .Query<MappingRule, BucketRuleSet, MappingRule>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public int Create(MappingRule entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO MappingRule (
                      MappingRuleId,
                      BucketRuleSetId,
                      ComparisonField,
                      ComparisonType,
                      ComparisonValue)
                  VALUES ($1, $2, $3, $4, $5)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketRuleSetId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.ComparisonField));
        cmd.Parameters.Add(new DuckDBParameter(entity.ComparisonType));
        cmd.Parameters.Add(new DuckDBParameter(entity.ComparisonValue));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<MappingRule> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(MappingRule entity)
    {
        var sql = """
                  UPDATE MappingRule
                  SET BucketRuleSetId = $1, ComparisonField = $2, ComparisonType = $3, ComparisonValue = $4
                  WHERE MappingRuleId = $5
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.BucketRuleSetId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.ComparisonField));
        cmd.Parameters.Add(new DuckDBParameter(entity.ComparisonType));
        cmd.Parameters.Add(new DuckDBParameter(entity.ComparisonValue));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<MappingRule> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        // Consistency checks
        var entity = ById(id);
        if (entity is null) throw new Exception($"MappingRule with id {id} not found.");

        var sql = """
                  DELETE FROM MappingRule WHERE MappingRuleId = $1
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
        if (entities.Count == 0) throw new Exception("No MappingRules found with passed IDs.");

        return scope.Sum(Delete);
    }
}
