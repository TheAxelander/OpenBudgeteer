using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

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

        var ruleSetDict = new Dictionary<Guid, BucketRuleSet>();
        _connection.Query<BucketRuleSet, Bucket, MappingRule?, BucketRuleSet>(
            sql,
            (ruleSet, bucket, mappingRule) =>
            {
                if (!ruleSetDict.TryGetValue(ruleSet.Id, out var existingRuleSet))
                {
                    existingRuleSet = ruleSet;
                    existingRuleSet.TargetBucket = bucket;
                    ruleSetDict.Add(ruleSet.Id, existingRuleSet);
                }

                if (mappingRule == null) return existingRuleSet;
                existingRuleSet.MappingRules ??= new List<MappingRule>();
                existingRuleSet.MappingRules.Add(mappingRule);
                return existingRuleSet;
            },
            splitOn: "Id,Id");

        return ruleSetDict.Values.AsQueryable();
    }

    public BucketRuleSet? ById(Guid id)
    {
        var sql = """
                  SELECT BucketRuleSetId AS Id, Priority, Name, TargetBucketId
                  FROM BucketRuleSet
                  WHERE BucketRuleSetId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new BucketRuleSet
            {
                Id = Guid.Parse(reader.GetString(0)),
                Priority = reader.GetInt32(1),
                Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                TargetBucketId = Guid.Parse(reader.GetString(3))
            };
        }
        return null;
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
                  WHERE brs.BucketRuleSetId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        BucketRuleSet? ruleSet = null;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            ruleSet ??= new BucketRuleSet
            {
                Id = Guid.Parse(reader.GetString(0)),
                Priority = reader.GetInt32(1),
                Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                TargetBucketId = Guid.Parse(reader.GetString(3)),
                TargetBucket = new Bucket
                {
                    Id = Guid.Parse(reader.GetString(4)),
                    Name = reader.IsDBNull(5) ? null : reader.GetString(5),
                    BucketGroupId = Guid.Parse(reader.GetString(6)),
                    ColorCode = reader.IsDBNull(7) ? null : reader.GetString(7),
                    TextColorCode = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(9)),
                    IsInactive = reader.GetBoolean(10),
                    IsInactiveFrom = DateOnly.FromDateTime(reader.GetDateTime(11)),
                    IsHiddenFromSummaries = reader.GetBoolean(12)
                }
            };

            if (reader.IsDBNull(13)) continue;
            ruleSet.MappingRules ??= new List<MappingRule>();
            ruleSet.MappingRules.Add(new MappingRule
            {
                Id = Guid.Parse(reader.GetString(13)),
                BucketRuleSetId = Guid.Parse(reader.GetString(14)),
                ComparisonField = reader.GetInt32(15),
                ComparisonType = reader.GetInt32(16),
                ComparisonValue = reader.GetString(17)
            });
        }
        return ruleSet;
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
        if (entity.MappingRules is not null)
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
