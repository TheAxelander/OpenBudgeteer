using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

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
        var sql = @"SELECT BucketMovementId AS Id, BucketId, Amount, MovementDate
                    FROM BucketMovement";
        return _connection.Query<BucketMovement>(sql).AsQueryable();
    }

    public IQueryable<BucketMovement> AllWithIncludedEntities()
    {
        var sql = @"SELECT
                        bm.BucketMovementId AS Id,
                        bm.BucketId,
                        bm.Amount,
                        bm.MovementDate,
                        b.BucketId AS Id,
                        b.Name,
                        b.BucketGroupId,
                        b.ColorCode,
                        b.TextColorCode,
                        b.ValidFrom,
                        b.IsInactive,
                        b.IsInactiveFrom,
                        b.IsHiddenFromSummaries
                    FROM BucketMovement bm
                    INNER JOIN Bucket b ON bm.BucketId = b.BucketId";

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
        var sql = @"SELECT BucketMovementId AS Id, BucketId, Amount, MovementDate
                    FROM BucketMovement
                    WHERE BucketMovementId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new BucketMovement
            {
                Id = Guid.Parse(reader.GetString(0)),
                BucketId = Guid.Parse(reader.GetString(1)),
                Amount = reader.GetDecimal(2),
                MovementDate = DateOnly.FromDateTime(reader.GetDateTime(3))
            };
        }
        return null;
    }

    public BucketMovement? ByIdWithIncludedEntities(Guid id)
    {
        var sql = @"SELECT
                        bm.BucketMovementId AS Id,
                        bm.BucketId,
                        bm.Amount,
                        bm.MovementDate,
                        b.BucketId AS Id,
                        b.Name,
                        b.BucketGroupId,
                        b.ColorCode,
                        b.TextColorCode,
                        b.ValidFrom,
                        b.IsInactive,
                        b.IsInactiveFrom,
                        b.IsHiddenFromSummaries
                    FROM BucketMovement bm
                    INNER JOIN Bucket b ON bm.BucketId = b.BucketId
                    WHERE bm.BucketMovementId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        
        var bucketMovement = new BucketMovement
        {
            Id = Guid.Parse(reader.GetString(0)),
            BucketId = Guid.Parse(reader.GetString(1)),
            Amount = reader.GetDecimal(2),
            MovementDate = DateOnly.FromDateTime(reader.GetDateTime(3)),
            Bucket = new Bucket
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
        return bucketMovement;
    }

    public int Create(BucketMovement entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

        var sql = @"INSERT INTO BucketMovement (BucketMovementId, BucketId, Amount, MovementDate)
                    VALUES ($1, $2, $3, $4)";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = entity.Id.ToString();
        cmd.Parameters.Add(p1);

        var p2 = cmd.CreateParameter();
        p2.Value = entity.BucketId.ToString();
        cmd.Parameters.Add(p2);

        var p3 = cmd.CreateParameter();
        p3.Value = entity.Amount;
        cmd.Parameters.Add(p3);

        var p4 = cmd.CreateParameter();
        p4.Value = entity.MovementDate.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p4);

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<BucketMovement> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BucketMovement entity)
    {
        var sql = @"UPDATE BucketMovement
                    SET BucketId = $1, Amount = $2, MovementDate = $3
                    WHERE BucketMovementId = $4";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = entity.BucketId.ToString();
        cmd.Parameters.Add(p1);

        var p2 = cmd.CreateParameter();
        p2.Value = entity.Amount;
        cmd.Parameters.Add(p2);

        var p3 = cmd.CreateParameter();
        p3.Value = entity.MovementDate.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p3);

        var p4 = cmd.CreateParameter();
        p4.Value = entity.Id.ToString();
        cmd.Parameters.Add(p4);

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
        
        var sql = @"DELETE FROM BucketMovement WHERE BucketMovementId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

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