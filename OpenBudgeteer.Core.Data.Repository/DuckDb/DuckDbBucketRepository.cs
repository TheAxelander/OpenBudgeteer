using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbBucketRepository : IBucketRepository
{
    private readonly DbConnection _connection;

    public DuckDbBucketRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<Bucket> All()
    {
        var sql = @"SELECT
                        BucketId AS Id,
                        Name,
                        BucketGroupId,
                        ColorCode,
                        TextColorCode,
                        ValidFrom,
                        IsInactive,
                        IsInactiveFrom,
                        IsHiddenFromSummaries
                    FROM Bucket";
        return _connection.Query<Bucket>(sql).AsQueryable();
    }

    public IQueryable<Bucket> AllWithVersions()
    {
        var sql = @"SELECT
                        b.BucketId AS Id,
                        b.Name,
                        b.BucketGroupId,
                        b.ColorCode,
                        b.TextColorCode,
                        b.ValidFrom,
                        b.IsInactive,
                        b.IsInactiveFrom,
                        b.IsHiddenFromSummaries,
                        bv.BucketVersionId AS Id,
                        bv.BucketId,
                        bv.Version,
                        bv.BucketType,
                        bv.BucketTypeXParam,
                        bv.BucketTypeYParam,
                        bv.BucketTypeZParam,
                        bv.Notes,
                        bv.ValidFrom
                    FROM Bucket b
                    LEFT JOIN BucketVersion bv ON b.BucketId = bv.BucketId";

        var bucketDict = new Dictionary<Guid, Bucket>();
        _connection.Query<Bucket, BucketVersion?, Bucket>(
            sql,
            (bucket, version) =>
            {
                if (!bucketDict.TryGetValue(bucket.Id, out var existingBucket))
                {
                    existingBucket = bucket;
                    bucketDict.Add(bucket.Id, existingBucket);
                }

                if (version == null) return existingBucket;
                existingBucket.BucketVersions ??= new List<BucketVersion>();
                existingBucket.BucketVersions.Add(version);
                return existingBucket;
            },
            splitOn: "Id");

        return bucketDict.Values.AsQueryable();
    }

    public IQueryable<Bucket> AllWithActivities()
    {
        var sql = @"SELECT
                        b.BucketId AS Id,
                        b.Name,
                        b.BucketGroupId,
                        b.ColorCode,
                        b.TextColorCode,
                        b.ValidFrom,
                        b.IsInactive,
                        b.IsInactiveFrom,
                        b.IsHiddenFromSummaries,
                        bm.BucketMovementId AS Id,
                        bm.BucketId,
                        bm.Amount,
                        bm.MovementDate,
                        bt.BudgetedTransactionId AS Id,
                        bt.TransactionId,
                        bt.BucketId,
                        bt.Amount
                    FROM Bucket b
                    LEFT JOIN BucketMovement bm ON b.BucketId = bm.BucketId
                    LEFT JOIN BudgetedTransaction bt ON b.BucketId = bt.BucketId";

        var bucketDict = new Dictionary<Guid, Bucket>();
        _connection.Query<Bucket, BucketMovement?, BudgetedTransaction?, Bucket>(
            sql,
            (bucket, movement, transaction) =>
            {
                if (!bucketDict.TryGetValue(bucket.Id, out var existingBucket))
                {
                    existingBucket = bucket;
                    bucketDict.Add(bucket.Id, existingBucket);
                }
                if (movement != null)
                {
                    existingBucket.BucketMovements ??= new List<BucketMovement>();
                    if (existingBucket.BucketMovements.All(m => m.Id != movement.Id))
                        existingBucket.BucketMovements.Add(movement);
                }
                if (transaction != null)
                {
                    existingBucket.BudgetedTransactions ??= new List<BudgetedTransaction>();
                    if (existingBucket.BudgetedTransactions.All(t => t.Id != transaction.Id))
                        existingBucket.BudgetedTransactions.Add(transaction);
                }
                return existingBucket;
            },
            splitOn: "Id,Id");

        return bucketDict.Values.AsQueryable();
    }

    public IQueryable<Bucket> AllWithIncludedEntities()
    {
        var sql = @"SELECT
                        b.BucketId AS Id,
                        b.Name,
                        b.BucketGroupId,
                        b.ColorCode,
                        b.TextColorCode,
                        b.ValidFrom,
                        b.IsInactive,
                        b.IsInactiveFrom,
                        b.IsHiddenFromSummaries,
                        bg.BucketGroupId AS Id,
                        bg.Name,
                        bg.Position,
                        bm.BucketMovementId AS Id,
                        bm.BucketId,
                        bm.Amount,
                        bm.MovementDate,
                        bv.BucketVersionId AS Id,
                        bv.BucketId,
                        bv.Version,
                        bv.BucketType,
                        bv.BucketTypeXParam,
                        bv.BucketTypeYParam,
                        bv.BucketTypeZParam,
                        bv.Notes,
                        bv.ValidFrom,
                        bt.BudgetedTransactionId AS Id,
                        bt.TransactionId,
                        bt.BucketId,
                        bt.Amount
                    FROM Bucket b
                    INNER JOIN BucketGroup bg ON b.BucketGroupId = bg.BucketGroupId
                    LEFT JOIN BucketMovement bm ON b.BucketId = bm.BucketId
                    LEFT JOIN BucketVersion bv ON b.BucketId = bv.BucketId
                    LEFT JOIN BudgetedTransaction bt ON b.BucketId = bt.BucketId";

        var bucketDict = new Dictionary<Guid, Bucket>();
        _connection.Query<Bucket, BucketGroup, BucketMovement?, BucketVersion?, BudgetedTransaction?, Bucket>(
            sql,
            (bucket, bucketGroup, movement, version, transaction) =>
            {
                if (!bucketDict.TryGetValue(bucket.Id, out var existingBucket))
                {
                    existingBucket = bucket;
                    existingBucket.BucketGroup = bucketGroup;
                    bucketDict.Add(bucket.Id, existingBucket);
                }
                if (movement != null)
                {
                    existingBucket.BucketMovements ??= new List<BucketMovement>();
                    if (existingBucket.BucketMovements.All(m => m.Id != movement.Id))
                        existingBucket.BucketMovements.Add(movement);
                }
                if (version != null)
                {
                    existingBucket.BucketVersions ??= new List<BucketVersion>();
                    if (existingBucket.BucketVersions.All(v => v.Id != version.Id))
                        existingBucket.BucketVersions.Add(version);
                }
                if (transaction != null)
                {
                    existingBucket.BudgetedTransactions ??= new List<BudgetedTransaction>();
                    if (existingBucket.BudgetedTransactions.All(t => t.Id != transaction.Id))
                        existingBucket.BudgetedTransactions.Add(transaction);
                }
                return existingBucket;
            },
            splitOn: "Id,Id,Id,Id");

        return bucketDict.Values.AsQueryable();
    }

    public Bucket? ById(Guid id)
    {
        var sql = @"SELECT
                        BucketId AS Id,
                        Name,
                        BucketGroupId,
                        ColorCode,
                        TextColorCode,
                        ValidFrom,
                        IsInactive,
                        IsInactiveFrom,
                        IsHiddenFromSummaries
                    FROM Bucket
                    WHERE BucketId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Bucket
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                BucketGroupId = Guid.Parse(reader.GetString(2)),
                ColorCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                TextColorCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(5)),
                IsInactive = reader.GetBoolean(6),
                IsInactiveFrom = DateOnly.FromDateTime(reader.GetDateTime(7)),
                IsHiddenFromSummaries = reader.GetBoolean(8)
            };
        }
        return null;
    }

    public Bucket? ByIdWithVersions(Guid id)
    {
        var sql = @"SELECT
                        b.BucketId AS Id,
                        b.Name,
                        b.BucketGroupId,
                        b.ColorCode,
                        b.TextColorCode,
                        b.ValidFrom,
                        b.IsInactive,
                        b.IsInactiveFrom,
                        b.IsHiddenFromSummaries,
                        bv.BucketVersionId AS Id,
                        bv.BucketId,
                        bv.Version,
                        bv.BucketType,
                        bv.BucketTypeXParam,
                        bv.BucketTypeYParam,
                        bv.BucketTypeZParam,
                        bv.Notes,
                        bv.ValidFrom
                    FROM Bucket b
                    LEFT JOIN BucketVersion bv ON b.BucketId = bv.BucketId
                    WHERE b.BucketId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        Bucket? bucket = null;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            bucket ??= new Bucket
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                BucketGroupId = Guid.Parse(reader.GetString(2)),
                ColorCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                TextColorCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(5)),
                IsInactive = reader.GetBoolean(6),
                IsInactiveFrom = DateOnly.FromDateTime(reader.GetDateTime(7)),
                IsHiddenFromSummaries = reader.GetBoolean(8),
            };

            if (reader.IsDBNull(9)) continue;
            bucket.BucketVersions ??=  new List<BucketVersion>();
            bucket.BucketVersions.Add(new BucketVersion
            {
                Id = Guid.Parse(reader.GetString(9)),
                BucketId = Guid.Parse(reader.GetString(10)),
                Version = reader.GetInt32(11),
                BucketType = reader.GetInt32(12),
                BucketTypeXParam = reader.GetInt32(13),
                BucketTypeYParam = reader.GetDecimal(14),
                BucketTypeZParam = DateOnly.FromDateTime(reader.GetDateTime(15)),
                Notes = reader.IsDBNull(16) ? null : reader.GetString(16),
                ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(17))
            });
        }
        return bucket;
    }

    public Bucket? ByIdWithMovements(Guid id)
    {
        var sql = @"SELECT
                        b.BucketId AS Id,
                        b.Name,
                        b.BucketGroupId,
                        b.ColorCode,
                        b.TextColorCode,
                        b.ValidFrom,
                        b.IsInactive,
                        b.IsInactiveFrom,
                        b.IsHiddenFromSummaries,
                        bm.BucketMovementId AS Id,
                        bm.BucketId,
                        bm.Amount,
                        bm.MovementDate
                    FROM Bucket b
                    LEFT JOIN BucketMovement bm ON b.BucketId = bm.BucketId
                    WHERE b.BucketId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        Bucket? bucket = null;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            bucket ??= new Bucket
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                BucketGroupId = Guid.Parse(reader.GetString(2)),
                ColorCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                TextColorCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(5)),
                IsInactive = reader.GetBoolean(6),
                IsInactiveFrom = DateOnly.FromDateTime(reader.GetDateTime(7)),
                IsHiddenFromSummaries = reader.GetBoolean(8),
            };

            if (reader.IsDBNull(9)) continue;
            bucket.BucketMovements ??=  new List<BucketMovement>();
            bucket.BucketMovements.Add(new BucketMovement
            {
                Id = Guid.Parse(reader.GetString(9)),
                BucketId = Guid.Parse(reader.GetString(10)),
                Amount = reader.GetDecimal(11),
                MovementDate = DateOnly.FromDateTime(reader.GetDateTime(12))
            });
        }
        return bucket;
    }

    public Bucket? ByIdWithTransactions(Guid id)
    {
        var sql = @"SELECT
                        b.BucketId AS Id,
                        b.Name,
                        b.BucketGroupId,
                        b.ColorCode,
                        b.TextColorCode,
                        b.ValidFrom,
                        b.IsInactive,
                        b.IsInactiveFrom,
                        b.IsHiddenFromSummaries,
                        but.BudgetedTransactionId AS Id,
                        but.TransactionId,
                        but.BucketId,
                        but.Amount,
                        bt.TransactionId AS Id,
                        bt.AccountId,
                        bt.TransactionDate,
                        bt.Payee,
                        bt.Memo,
                        bt.Amount
                    FROM Bucket b
                    LEFT JOIN BudgetedTransaction but ON b.BucketId = but.BucketId
                    LEFT JOIN BankTransaction bt ON but.TransactionId = bt.TransactionId
                    WHERE b.BucketId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        Bucket? bucket = null;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            bucket ??= new Bucket
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                BucketGroupId = Guid.Parse(reader.GetString(2)),
                ColorCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                TextColorCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(5)),
                IsInactive = reader.GetBoolean(6),
                IsInactiveFrom = DateOnly.FromDateTime(reader.GetDateTime(7)),
                IsHiddenFromSummaries = reader.GetBoolean(8),
            };

            if (reader.IsDBNull(9)) continue;
            bucket.BudgetedTransactions ??=  new List<BudgetedTransaction>();
            bucket.BudgetedTransactions.Add(new BudgetedTransaction
            {
                Id = Guid.Parse(reader.GetString(9)),
                TransactionId = Guid.Parse(reader.GetString(10)),
                Transaction = new()
                {
                    Id = Guid.Parse(reader.GetString(13)),
                    AccountId = Guid.Parse(reader.GetString(14)),
                    TransactionDate = DateOnly.FromDateTime(reader.GetDateTime(15)),
                    Payee = reader.IsDBNull(16) ? null : reader.GetString(16),
                    Memo = reader.IsDBNull(17) ? null : reader.GetString(17),
                    Amount = reader.GetDecimal(18),
                },
                BucketId = Guid.Parse(reader.GetString(11)),
                Amount = reader.GetDecimal(12)
            });
        }
        return bucket;
    }

    public Bucket? ByIdWithIncludedEntities(Guid id)
    {
        var sql = @"SELECT
                        b.BucketId AS Id,
                        b.Name,
                        b.BucketGroupId,
                        b.ColorCode,
                        b.TextColorCode,
                        b.ValidFrom,
                        b.IsInactive,
                        b.IsInactiveFrom,
                        b.IsHiddenFromSummaries,
                        bg.BucketGroupId AS Id,
                        bg.Name,
                        bg.Position,
                        bm.BucketMovementId AS Id,
                        bm.BucketId,
                        bm.Amount,
                        bm.MovementDate,
                        bv.BucketVersionId AS Id,
                        bv.BucketId,
                        bv.Version,
                        bv.BucketType,
                        bv.BucketTypeXParam,
                        bv.BucketTypeYParam,
                        bv.BucketTypeZParam,
                        bv.Notes,
                        bv.ValidFrom,
                        bt.BudgetedTransactionId AS Id,
                        bt.TransactionId,
                        bt.BucketId,
                        bt.Amount
                    FROM Bucket b
                    INNER JOIN BucketGroup bg ON b.BucketGroupId = bg.BucketGroupId
                    LEFT JOIN BucketMovement bm ON b.BucketId = bm.BucketId
                    LEFT JOIN BucketVersion bv ON b.BucketId = bv.BucketId
                    LEFT JOIN BudgetedTransaction bt ON b.BucketId = bt.BucketId
                    WHERE b.BucketId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        Bucket? bucket = null;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            bucket ??= new Bucket
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                BucketGroupId = Guid.Parse(reader.GetString(2)),
                ColorCode = reader.IsDBNull(3) ? null : reader.GetString(3),
                TextColorCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(5)),
                IsInactive = reader.GetBoolean(6),
                IsInactiveFrom = DateOnly.FromDateTime(reader.GetDateTime(7)),
                IsHiddenFromSummaries = reader.GetBoolean(8),
                BucketGroup = new BucketGroup
                {
                    Id = Guid.Parse(reader.GetString(9)),
                    Name = reader.IsDBNull(10) ? null : reader.GetString(10),
                    Position = reader.GetInt32(11)
                }
            };

            if (!reader.IsDBNull(12))
            {
                var movementId = Guid.Parse(reader.GetString(12));
                bucket.BucketMovements ??= new List<BucketMovement>();
                if (bucket.BucketMovements.All(m => m.Id != movementId))
                {
                    bucket.BucketMovements.Add(new BucketMovement
                    {
                        Id = movementId,
                        BucketId = Guid.Parse(reader.GetString(13)),
                        Amount = reader.GetDecimal(14),
                        MovementDate = DateOnly.FromDateTime(reader.GetDateTime(15))
                    });
                }
            }

            if (!reader.IsDBNull(16))
            {
                var versionId = Guid.Parse(reader.GetString(16));
                bucket.BucketVersions ??= new List<BucketVersion>();
                if (bucket.BucketVersions.All(v => v.Id != versionId))
                {
                    bucket.BucketVersions.Add(new BucketVersion
                    {
                        Id = versionId,
                        BucketId = Guid.Parse(reader.GetString(17)),
                        Version = reader.GetInt32(18),
                        BucketType = reader.GetInt32(19),
                        BucketTypeXParam = reader.GetInt32(20),
                        BucketTypeYParam = reader.GetDecimal(21),
                        BucketTypeZParam = DateOnly.FromDateTime(reader.GetDateTime(22)),
                        Notes = reader.IsDBNull(23) ? null : reader.GetString(23),
                        ValidFrom = DateOnly.FromDateTime(reader.GetDateTime(24))
                    });
                }
            }

            if (!reader.IsDBNull(25))
            {
                var transactionId = Guid.Parse(reader.GetString(25));
                bucket.BudgetedTransactions ??= new List<BudgetedTransaction>();
                if (bucket.BudgetedTransactions.All(t => t.Id != transactionId))
                {
                    bucket.BudgetedTransactions.Add(new BudgetedTransaction
                    {
                        Id = transactionId,
                        TransactionId = Guid.Parse(reader.GetString(26)),
                        BucketId = Guid.Parse(reader.GetString(27)),
                        Amount = reader.GetDecimal(28)
                    });
                }
            }
        }
        return bucket;
    }

    public int Create(Bucket entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

        var sql = @"INSERT INTO Bucket (
                        BucketId,
                        Name,
                        BucketGroupId,
                        ColorCode,
                        TextColorCode,
                        ValidFrom,
                        IsInactive,
                        IsInactiveFrom,
                        IsHiddenFromSummaries)
                    VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = entity.Id.ToString();
        cmd.Parameters.Add(p1);

        var p2 = cmd.CreateParameter();
        p2.Value = (object?)entity.Name ?? DBNull.Value;
        cmd.Parameters.Add(p2);

        var p3 = cmd.CreateParameter();
        p3.Value = entity.BucketGroupId.ToString();
        cmd.Parameters.Add(p3);

        var p4 = cmd.CreateParameter();
        p4.Value = (object?)entity.ColorCode ?? DBNull.Value;
        cmd.Parameters.Add(p4);

        var p5 = cmd.CreateParameter();
        p5.Value = (object?)entity.TextColorCode ?? DBNull.Value;
        cmd.Parameters.Add(p5);

        var p6 = cmd.CreateParameter();
        p6.Value = entity.ValidFrom.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p6);

        var p7 = cmd.CreateParameter();
        p7.Value = entity.IsInactive;
        cmd.Parameters.Add(p7);

        var p8 = cmd.CreateParameter();
        p8.Value = entity.IsInactiveFrom.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p8);

        var p9 = cmd.CreateParameter();
        p9.Value = entity.IsHiddenFromSummaries;
        cmd.Parameters.Add(p9);

        if (cmd.ExecuteNonQuery() != 1) return 0;

        var bucketVersionRepository = new DuckDbBucketVersionRepository(_connection);
        if (entity.BucketVersions is null) return 1;

        var bucketVersion = entity.BucketVersions.First();
        bucketVersion.BucketId = entity.Id;
        var bucketVersionResult = bucketVersionRepository.Create(bucketVersion);

        return bucketVersionResult == 1 ? 2 : 1;
    }

    public int CreateRange(IEnumerable<Bucket> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(Bucket entity)
    {
        var sql = @"UPDATE Bucket
                    SET
                        Name = $1,
                        BucketGroupId = $2,
                        ColorCode = $3,
                        TextColorCode = $4,
                        ValidFrom = $5,
                        IsInactive = $6,
                        IsInactiveFrom = $7,
                        IsHiddenFromSummaries = $8
                    WHERE BucketId = $9";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = (object?)entity.Name ?? DBNull.Value;
        cmd.Parameters.Add(p1);

        var p2 = cmd.CreateParameter();
        p2.Value = entity.BucketGroupId.ToString();
        cmd.Parameters.Add(p2);

        var p3 = cmd.CreateParameter();
        p3.Value = (object?)entity.ColorCode ?? DBNull.Value;
        cmd.Parameters.Add(p3);

        var p4 = cmd.CreateParameter();
        p4.Value = (object?)entity.TextColorCode ?? DBNull.Value;
        cmd.Parameters.Add(p4);

        var p5 = cmd.CreateParameter();
        p5.Value = entity.ValidFrom.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p5);

        var p6 = cmd.CreateParameter();
        p6.Value = entity.IsInactive;
        cmd.Parameters.Add(p6);

        var p7 = cmd.CreateParameter();
        p7.Value = entity.IsInactiveFrom.ToDateTime(TimeOnly.MinValue);
        cmd.Parameters.Add(p7);

        var p8 = cmd.CreateParameter();
        p8.Value = entity.IsHiddenFromSummaries;
        cmd.Parameters.Add(p8);

        var p9 = cmd.CreateParameter();
        p9.Value = entity.Id.ToString();
        cmd.Parameters.Add(p9);

        var result = cmd.ExecuteNonQuery();

        if (entity.BucketVersions is null) return result;
        var bucketVersionRepository = new DuckDbBucketVersionRepository(_connection);
        result += entity.BucketVersions
            .Where(i => i.Id == Guid.Empty)
            .Sum(i => bucketVersionRepository.Create(i));

        return result;
    }

    public int UpdateRange(IEnumerable<Bucket> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        // Prevent deletion of system buckets
        if (id == Guid.Parse("00000000-0000-0000-0000-000000000001") ||
            id == Guid.Parse("00000000-0000-0000-0000-000000000002")) return 0;

        // Consistency checks
        var entity = ByIdWithIncludedEntities(id);
        if (entity is null) throw new Exception($"Bucket with id {id} not found.");
        if (entity.BucketMovements is not null && entity.BucketMovements.Count != 0) throw new Exception($"Cannot delete a Bucket with BucketMovements assigned to it.");
        if (entity.BudgetedTransactions is not null && entity.BudgetedTransactions.Count != 0) throw new Exception($"Cannot delete a Bucket with BudgetedTransactions assigned to it.");

        var result = 0;

        // Delete related entities first
        if (entity.BucketVersions is not null)
        {
            var bucketVersionRepository = new DuckDbBucketVersionRepository(_connection);
            result += bucketVersionRepository.DeleteRange(entity.BucketVersions.Select(i => i.Id));
        }
        if (entity.BucketMovements is not null)
        {
            var bucketMovementRepository = new DuckDbBucketMovementRepository(_connection);
            result += bucketMovementRepository.DeleteRange(entity.BucketMovements.Select(i => i.Id));
        }
        if (entity.BudgetedTransactions != null)
        {
            var budgetedTransactionRepository = new DuckDbBudgetedTransactionRepository(_connection);
            result += budgetedTransactionRepository.DeleteRange(entity.BudgetedTransactions.Select(i => i.Id));
        }

        // Delete the bucket
        var sql = @"DELETE FROM Bucket WHERE BucketId = $1";

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        var p1 = cmd.CreateParameter();
        p1.Value = id.ToString();
        cmd.Parameters.Add(p1);

        result += cmd.ExecuteNonQuery();

        return result;
    }

    public int DeleteRange(IEnumerable<Guid> ids)
    {
        // Prevent deletion of system buckets
        var cleansedEntities = ids
            .Where(i =>
                i != Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                i != Guid.Parse("00000000-0000-0000-0000-000000000002"))
            .ToList();

        // Consistency checks
        var entities = cleansedEntities
            .Select(ByIdWithIncludedEntities)
            .OfType<Bucket>()
            .ToList();
        if (!entities.Any()) throw new Exception($"No Buckets found with passed IDs.");
        if (entities.Any(i => i.BucketMovements != null && i.BucketMovements.Count != 0)) throw new Exception($"Cannot delete a Bucket with BucketMovements assigned to it.");
        if (entities.Any(i => i.BudgetedTransactions != null && i.BudgetedTransactions.Count != 0)) throw new Exception($"Cannot delete a Bucket with BudgetedTransactions assigned to it.");

        return cleansedEntities.Sum(Delete);
    }
}
