using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

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
        var sql = @"SELECT BucketVersionId AS Id, BucketId, Version, BucketType, BucketTypeXParam, BucketTypeYParam, BucketTypeZParam, Notes, ValidFrom
                    FROM BucketVersion";
        return _connection.Query<BucketVersion>(sql).AsQueryable();
    }

    public IQueryable<BucketVersion> AllWithIncludedEntities()
    {
        var sql = @"SELECT
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
                    INNER JOIN Bucket b ON bv.BucketId = b.BucketId";

        var result = _connection.Query<BucketVersion, Bucket, BucketVersion>(
            sql,
            (bucketVersion, bucket) =>
            {
                bucketVersion.Bucket = bucket;
                return bucketVersion;
            },
            splitOn: "Id");

        return result.AsQueryable();
    }

    public BucketVersion? ById(Guid id)
    {
        var sql = @"SELECT BucketVersionId AS Id, BucketId, Version, BucketType, BucketTypeXParam, BucketTypeYParam, BucketTypeZParam, Notes, ValidFrom
                    FROM BucketVersion
                    WHERE BucketVersionId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new BucketVersion
            {
                Id = Guid.Parse(reader.GetString(0)),
                BucketId = Guid.Parse(reader.GetString(1)),
                Version = reader.GetInt32(2),
                BucketType = reader.GetInt32(3),
                BucketTypeXParam = reader.GetInt32(4),
                BucketTypeYParam = reader.GetDecimal(5),
                BucketTypeZParam = DateOnly.FromDateTime(reader.GetDateTime(6)),
                Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(8))
            };
        }
        return null;
    }

    public BucketVersion? ByIdWithIncludedEntities(Guid id)
    {
        var sql = @"SELECT
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
                    WHERE bv.BucketVersionId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var bucketVersion = new BucketVersion
            {
                Id = Guid.Parse(reader.GetString(0)),
                BucketId = Guid.Parse(reader.GetString(1)),
                Version = reader.GetInt32(2),
                BucketType = reader.GetInt32(3),
                BucketTypeXParam = reader.GetInt32(4),
                BucketTypeYParam = reader.GetDecimal(5),
                BucketTypeZParam = DateOnly.FromDateTime(reader.GetDateTime(6)),
                Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(8)),
                Bucket = new Bucket
                {
                    Id = Guid.Parse(reader.GetString(9)),
                    Name = reader.IsDBNull(10) ? null : reader.GetString(10),
                    BucketGroupId = Guid.Parse(reader.GetString(11)),
                    ColorCode = reader.IsDBNull(12) ? null : reader.GetString(12),
                    TextColorCode = reader.IsDBNull(13) ? null : reader.GetString(13),
                    ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(14)),
                    IsInactive = reader.GetBoolean(15),
                    IsInactiveFrom = DateOnly.FromDateTime(reader.GetDateTime(16)),
                    IsHiddenFromSummaries = reader.GetBoolean(17)
                }
            };
            return bucketVersion;
        }
        return null;
    }

    public int Create(BucketVersion entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = @"INSERT INTO BucketVersion (BucketVersionId, BucketId, Version, BucketType, BucketTypeXParam, BucketTypeYParam, BucketTypeZParam, Notes, ValidFrom)
                    VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = entity.Id.ToString();
        cmd.Parameters.Add(p1);

        var p2 = cmd.CreateParameter();
        p2.Value = entity.BucketId.ToString();
        cmd.Parameters.Add(p2);

        var p3 = cmd.CreateParameter();
        p3.Value = entity.Version;
        cmd.Parameters.Add(p3);

        var p4 = cmd.CreateParameter();
        p4.Value = entity.BucketType;
        cmd.Parameters.Add(p4);

        var p5 = cmd.CreateParameter();
        p5.Value = entity.BucketTypeXParam;
        cmd.Parameters.Add(p5);

        var p6 = cmd.CreateParameter();
        p6.Value = entity.BucketTypeYParam;
        cmd.Parameters.Add(p6);

        var p7 = cmd.CreateParameter();
        p7.Value = entity.BucketTypeZParam.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p7);

        var p8 = cmd.CreateParameter();
        p8.Value = (object?)entity.Notes ?? DBNull.Value;
        cmd.Parameters.Add(p8);

        var p9 = cmd.CreateParameter();
        p9.Value = entity.ValidFrom.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p9);

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
        var sql = @"UPDATE BucketVersion
                    SET BucketId = $1, Version = $2, BucketType = $3, BucketTypeXParam = $4, BucketTypeYParam = $5, BucketTypeZParam = $6, Notes = $7, ValidFrom = $8
                    WHERE BucketVersionId = $9";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = entity.BucketId.ToString();
        cmd.Parameters.Add(p1);

        var p2 = cmd.CreateParameter();
        p2.Value = entity.Version;
        cmd.Parameters.Add(p2);

        var p3 = cmd.CreateParameter();
        p3.Value = entity.BucketType;
        cmd.Parameters.Add(p3);

        var p4 = cmd.CreateParameter();
        p4.Value = entity.BucketTypeXParam;
        cmd.Parameters.Add(p4);

        var p5 = cmd.CreateParameter();
        p5.Value = entity.BucketTypeYParam;
        cmd.Parameters.Add(p5);

        var p6 = cmd.CreateParameter();
        p6.Value = entity.BucketTypeZParam.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p6);

        var p7 = cmd.CreateParameter();
        p7.Value = (object?)entity.Notes ?? DBNull.Value;
        cmd.Parameters.Add(p7);

        var p8 = cmd.CreateParameter();
        p8.Value = entity.ValidFrom.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p8);

        var p9 = cmd.CreateParameter();
        p9.Value = entity.Id.ToString();
        cmd.Parameters.Add(p9);

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
        var sql = @"DELETE FROM BucketVersion WHERE BucketVersionId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

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