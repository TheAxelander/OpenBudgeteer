using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

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
        var sql = """
                  SELECT
                      BucketId AS Id
                      ,Name
                      ,BucketGroupId
                      ,ColorCode
                      ,TextColorCode
                      ,ValidFrom
                      ,IsInactive
                      ,IsInactiveFrom
                      ,IsHiddenFromSummaries
                  FROM Bucket
                  """;
        return _connection.Query<Bucket>(sql).AsQueryable();
    }

    public IQueryable<Bucket> AllWithVersions()
    {
        var sql = """
                  SELECT
                      b.BucketId AS Id
                      ,b.Name
                      ,b.BucketGroupId
                      ,b.ColorCode
                      ,b.TextColorCode
                      ,b.ValidFrom
                      ,b.IsInactive
                      ,b.IsInactiveFrom
                      ,b.IsHiddenFromSummaries
                      ,bv.BucketVersionId AS Id
                      ,bv.BucketId
                      ,bv.Version
                      ,bv.BucketType
                      ,bv.BucketTypeXParam
                      ,bv.BucketTypeYParam
                      ,bv.BucketTypeZParam
                      ,bv.Notes
                      ,bv.ValidFrom
                  FROM Bucket b
                  LEFT JOIN BucketVersion bv ON b.BucketId = bv.BucketId
                  """;

        var mapper = new BucketMapper();
        _ = _connection
            .Query<Bucket, BucketVersion?, Bucket>(
                sql,
                mapper.MapWithVersion,
                splitOn: "Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public IQueryable<Bucket> AllWithActivities()
    {
        var sql = """
                  SELECT
                      b.BucketId AS Id
                      ,b.Name
                      ,b.BucketGroupId
                      ,b.ColorCode
                      ,b.TextColorCode
                      ,b.ValidFrom
                      ,b.IsInactive
                      ,b.IsInactiveFrom
                      ,b.IsHiddenFromSummaries
                      ,bm.BucketMovementId AS Id
                      ,bm.BucketId
                      ,bm.Amount
                      ,bm.MovementDate
                      ,bt.BudgetedTransactionId AS Id
                      ,bt.TransactionId
                      ,bt.BucketId
                      ,bt.Amount
                  FROM Bucket b
                  LEFT JOIN BucketMovement bm ON b.BucketId = bm.BucketId
                  LEFT JOIN BudgetedTransaction bt ON b.BucketId = bt.BucketId
                  """;

        var mapper = new BucketMapper();
        _ = _connection
            .Query<Bucket, BucketMovement?, BudgetedTransaction?, Bucket>(
                sql,
                mapper.MapWithActivities,
                splitOn: "Id,Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public IQueryable<Bucket> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      b.BucketId AS Id
                      ,b.Name
                      ,b.BucketGroupId
                      ,b.ColorCode
                      ,b.TextColorCode
                      ,b.ValidFrom
                      ,b.IsInactive
                      ,b.IsInactiveFrom
                      ,b.IsHiddenFromSummaries
                      ,bg.BucketGroupId AS Id
                      ,bg.Name
                      ,bg.Position
                      ,bm.BucketMovementId AS Id
                      ,bm.BucketId
                      ,bm.Amount
                      ,bm.MovementDate
                      ,bv.BucketVersionId AS Id
                      ,bv.BucketId
                      ,bv.Version
                      ,bv.BucketType
                      ,bv.BucketTypeXParam
                      ,bv.BucketTypeYParam
                      ,bv.BucketTypeZParam
                      ,bv.Notes
                      ,bv.ValidFrom
                      ,bt.BudgetedTransactionId AS Id
                      ,bt.TransactionId
                      ,bt.BucketId
                      ,bt.Amount
                  FROM Bucket b
                  INNER JOIN BucketGroup bg ON b.BucketGroupId = bg.BucketGroupId
                  LEFT JOIN BucketMovement bm ON b.BucketId = bm.BucketId
                  LEFT JOIN BucketVersion bv ON b.BucketId = bv.BucketId
                  LEFT JOIN BudgetedTransaction bt ON b.BucketId = bt.BucketId
                  """;

        var mapper = new BucketMapper();
        _ = _connection
            .Query<Bucket, BucketGroup, BucketMovement?, BucketVersion?, BudgetedTransaction?, Bucket>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id,Id,Id,Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public Bucket? ById(Guid id)
    {
        var sql = """
                  SELECT
                      BucketId AS Id
                      ,Name
                      ,BucketGroupId
                      ,ColorCode
                      ,TextColorCode
                      ,ValidFrom
                      ,IsInactive
                      ,IsInactiveFrom
                      ,IsHiddenFromSummaries
                  FROM Bucket
                  WHERE BucketId = $id
                  """;

        return _connection
            .Query<Bucket>(sql, param: new { id = id.ToString() })
            .FirstOrDefault();
    }

    public Bucket? ByIdWithVersions(Guid id)
    {
        var sql = """
                  SELECT
                      b.BucketId AS Id
                      ,b.Name
                      ,b.BucketGroupId
                      ,b.ColorCode
                      ,b.TextColorCode
                      ,b.ValidFrom
                      ,b.IsInactive
                      ,b.IsInactiveFrom
                      ,b.IsHiddenFromSummaries
                      ,bv.BucketVersionId AS Id
                      ,bv.BucketId
                      ,bv.Version
                      ,bv.BucketType
                      ,bv.BucketTypeXParam
                      ,bv.BucketTypeYParam
                      ,bv.BucketTypeZParam
                      ,bv.Notes
                      ,bv.ValidFrom
                  FROM Bucket b
                  LEFT JOIN BucketVersion bv ON b.BucketId = bv.BucketId
                  WHERE b.BucketId = $id
                  """;

        var mapper = new BucketMapper();
        _ = _connection
            .Query<Bucket, BucketVersion?, Bucket>(
                sql,
                mapper.MapWithVersion,
                splitOn: "Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public Bucket? ByIdWithMovements(Guid id)
    {
        var sql = """
                  SELECT
                      b.BucketId AS Id
                      ,b.Name
                      ,b.BucketGroupId
                      ,b.ColorCode
                      ,b.TextColorCode
                      ,b.ValidFrom
                      ,b.IsInactive
                      ,b.IsInactiveFrom
                      ,b.IsHiddenFromSummaries
                      ,bm.BucketMovementId AS Id
                      ,bm.BucketId
                      ,bm.Amount
                      ,bm.MovementDate
                  FROM Bucket b
                  LEFT JOIN BucketMovement bm ON b.BucketId = bm.BucketId
                  WHERE b.BucketId = $id
                  """;

        var mapper = new BucketMapper();
        _ = _connection
            .Query<Bucket, BucketMovement?, Bucket>(
                sql,
                mapper.MapWithMovement,
                splitOn: "Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public Bucket? ByIdWithTransactions(Guid id)
    {
        var sql = """
                  SELECT
                      b.BucketId AS Id
                      ,b.Name
                      ,b.BucketGroupId
                      ,b.ColorCode
                      ,b.TextColorCode
                      ,b.ValidFrom
                      ,b.IsInactive
                      ,b.IsInactiveFrom
                      ,b.IsHiddenFromSummaries
                      ,but.BudgetedTransactionId AS Id
                      ,but.TransactionId
                      ,but.BucketId
                      ,but.Amount
                      ,bt.TransactionId AS Id
                      ,bt.AccountId
                      ,bt.TransactionDate
                      ,bt.Payee
                      ,bt.Memo
                      ,bt.Amount
                  FROM Bucket b
                  LEFT JOIN BudgetedTransaction but ON b.BucketId = but.BucketId
                  LEFT JOIN BankTransaction bt ON but.TransactionId = bt.TransactionId
                  WHERE b.BucketId = $id
                  """;

        var mapper = new BucketMapper();
        _ = _connection
            .Query<Bucket, BudgetedTransaction?, BankTransaction?, Bucket>(
                sql,
                mapper.MapWithTransactions,
                splitOn: "Id,Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public Bucket? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      b.BucketId AS Id
                      ,b.Name
                      ,b.BucketGroupId
                      ,b.ColorCode
                      ,b.TextColorCode
                      ,b.ValidFrom
                      ,b.IsInactive
                      ,b.IsInactiveFrom
                      ,b.IsHiddenFromSummaries
                      ,bg.BucketGroupId AS Id
                      ,bg.Name
                      ,bg.Position
                      ,bm.BucketMovementId AS Id
                      ,bm.BucketId
                      ,bm.Amount
                      ,bm.MovementDate
                      ,bv.BucketVersionId AS Id
                      ,bv.BucketId
                      ,bv.Version
                      ,bv.BucketType
                      ,bv.BucketTypeXParam
                      ,bv.BucketTypeYParam
                      ,bv.BucketTypeZParam
                      ,bv.Notes
                      ,bv.ValidFrom
                      ,bt.BudgetedTransactionId AS Id
                      ,bt.TransactionId
                      ,bt.BucketId
                      ,bt.Amount
                  FROM Bucket b
                  INNER JOIN BucketGroup bg ON b.BucketGroupId = bg.BucketGroupId
                  LEFT JOIN BucketMovement bm ON b.BucketId = bm.BucketId
                  LEFT JOIN BucketVersion bv ON b.BucketId = bv.BucketId
                  LEFT JOIN BudgetedTransaction bt ON b.BucketId = bt.BucketId
                  WHERE b.BucketId = $id
                  """;

        var mapper = new BucketMapper();
        _ = _connection
            .Query<Bucket, BucketGroup, BucketMovement?, BucketVersion?, BudgetedTransaction?, Bucket>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id,Id,Id,Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public int Create(Bucket entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO Bucket
                      (BucketId
                      ,Name
                      ,BucketGroupId
                      ,ColorCode
                      ,TextColorCode
                      ,ValidFrom
                      ,IsInactive
                      ,IsInactiveFrom
                      ,IsHiddenFromSummaries)
                  VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Name ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketGroupId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.ColorCode ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.TextColorCode ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.ValidFrom.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsInactive));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsInactiveFrom.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsHiddenFromSummaries));

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
        var sql = """
                  UPDATE Bucket
                  SET Name = $1
                      ,BucketGroupId = $2
                      ,ColorCode = $3
                      ,TextColorCode = $4
                      ,ValidFrom = $5
                      ,IsInactive = $6
                      ,IsInactiveFrom = $7
                      ,IsHiddenFromSummaries = $8
                  WHERE BucketId = $9
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Name ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketGroupId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.ColorCode ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.TextColorCode ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.ValidFrom.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsInactive));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsInactiveFrom.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsHiddenFromSummaries));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

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
        if (entity.BucketVersions is not null && entity.BucketVersions.Count > 0)
        {
            var bucketVersionRepository = new DuckDbBucketVersionRepository(_connection);
            result += bucketVersionRepository.DeleteRange(entity.BucketVersions.Select(i => i.Id));
        }
        if (entity.BucketMovements is not null && entity.BucketMovements.Count > 0)
        {
            var bucketMovementRepository = new DuckDbBucketMovementRepository(_connection);
            result += bucketMovementRepository.DeleteRange(entity.BucketMovements.Select(i => i.Id));
        }
        if (entity.BudgetedTransactions != null && entity.BudgetedTransactions.Count > 0)
        {
            var budgetedTransactionRepository = new DuckDbBudgetedTransactionRepository(_connection);
            result += budgetedTransactionRepository.DeleteRange(entity.BudgetedTransactions.Select(i => i.Id));
        }

        // Delete the bucket
        var sql = """
                  DELETE
                  FROM Bucket
                  WHERE BucketId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

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
