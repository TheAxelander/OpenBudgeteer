using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

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

        var result = _connection.Query<MappingRule, BucketRuleSet, MappingRule>(
            sql,
            (mappingRule, bucketRuleSet) =>
            {
                mappingRule.BucketRuleSet = bucketRuleSet;
                return mappingRule;
            },
            splitOn: "Id");

        return result.AsQueryable();
    }

    public MappingRule? ById(Guid id)
    {
        var sql = """
                  SELECT MappingRuleId AS Id, BucketRuleSetId, ComparisonField, ComparisonType, ComparisonValue
                  FROM MappingRule
                  WHERE MappingRuleId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new MappingRule
            {
                Id = Guid.Parse(reader.GetString(0)),
                BucketRuleSetId = Guid.Parse(reader.GetString(1)),
                ComparisonField = reader.GetInt32(2),
                ComparisonType = reader.GetInt32(3),
                ComparisonValue = reader.GetString(4)
            };
        }
        return null;
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
                  WHERE mr.MappingRuleId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        var mappingRule = new MappingRule
        {
            Id = Guid.Parse(reader.GetString(0)),
            BucketRuleSetId = Guid.Parse(reader.GetString(1)),
            ComparisonField = reader.GetInt32(2),
            ComparisonType = reader.GetInt32(3),
            ComparisonValue = reader.GetString(4),
            BucketRuleSet = new BucketRuleSet
            {
                Id = Guid.Parse(reader.GetString(5)),
                Priority = reader.GetInt32(6),
                Name = reader.IsDBNull(7) ? null : reader.GetString(7),
                TargetBucketId = Guid.Parse(reader.GetString(8))
            }
        };
        return mappingRule;
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
