using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbBudgetedTransactionRepository : IBudgetedTransactionRepository
{
    private readonly DbConnection _connection;

    public DuckDbBudgetedTransactionRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<BudgetedTransaction> All()
    {
        var sql = """
                  SELECT BudgetedTransactionId AS Id, TransactionId, BucketId, Amount
                  FROM BudgetedTransaction
                  """;
        return _connection.Query<BudgetedTransaction>(sql).AsQueryable();
    }

    public IQueryable<BudgetedTransaction> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      bt.BudgetedTransactionId AS Id,
                      bt.TransactionId,
                      bt.BucketId,
                      bt.Amount,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries,
                      t.TransactionId AS Id,
                      t.AccountId,
                      t.TransactionDate,
                      t.Payee,
                      t.Memo,
                      t.Amount
                  FROM BudgetedTransaction bt
                  INNER JOIN Bucket b ON bt.BucketId = b.BucketId
                  INNER JOIN BankTransaction t ON bt.TransactionId = t.TransactionId
                  """;

        var mapper = new BudgetedTransactionMapper();
        _ = _connection
            .Query<BudgetedTransaction, Bucket, BankTransaction, BudgetedTransaction>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id,Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public IQueryable<BudgetedTransaction> AllWithTransactions()
    {
        var sql = """
                  SELECT
                      bt.BudgetedTransactionId AS Id,
                      bt.TransactionId,
                      bt.BucketId,
                      bt.Amount,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries,
                      t.TransactionId AS Id,
                      t.AccountId,
                      t.TransactionDate,
                      t.Payee,
                      t.Memo,
                      t.Amount,
                      a.AccountId AS Id,
                      a.Name,
                      a.IsActive
                  FROM BudgetedTransaction bt
                  INNER JOIN Bucket b ON bt.BucketId = b.BucketId
                  INNER JOIN BankTransaction t ON bt.TransactionId = t.TransactionId
                  INNER JOIN Account a ON t.AccountId = a.AccountId
                  """;

        var mapper = new BudgetedTransactionMapper();
        _ = _connection
            .Query<BudgetedTransaction, Bucket, BankTransaction, Account, BudgetedTransaction>(
                sql,
                mapper.MapWithEverythingIncludingAccount,
                splitOn: "Id,Id,Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public BudgetedTransaction? ById(Guid id)
    {
        var sql = """
                  SELECT
                      BudgetedTransactionId AS Id,
                      TransactionId,
                      BucketId,
                      Amount
                  FROM BudgetedTransaction
                  WHERE BudgetedTransactionId = $id
                  """;

        return _connection
            .Query<BudgetedTransaction>(sql, param: new { id = id.ToString() })
            .FirstOrDefault();
    }

    public BudgetedTransaction? ByIdWithTransaction(Guid id)
    {
        var sql = """
                  SELECT
                      bt.BudgetedTransactionId AS Id,
                      bt.TransactionId,
                      bt.BucketId,
                      bt.Amount,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries,
                      t.TransactionId AS Id,
                      t.AccountId,
                      t.TransactionDate,
                      t.Payee,
                      t.Memo,
                      t.Amount,
                      a.AccountId AS Id,
                      a.Name,
                      a.IsActive
                  FROM BudgetedTransaction bt
                  INNER JOIN Bucket b ON bt.BucketId = b.BucketId
                  INNER JOIN BankTransaction t ON bt.TransactionId = t.TransactionId
                  INNER JOIN Account a ON t.AccountId = a.AccountId
                  WHERE bt.BudgetedTransactionId = $id
                  """;

        var mapper = new BudgetedTransactionMapper();
        _ = _connection
            .Query<BudgetedTransaction, Bucket, BankTransaction, Account, BudgetedTransaction>(
                sql,
                mapper.MapWithEverythingIncludingAccount,
                splitOn: "Id,Id,Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public BudgetedTransaction? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      bt.BudgetedTransactionId AS Id,
                      bt.TransactionId,
                      bt.BucketId,
                      bt.Amount,
                      b.BucketId AS Id,
                      b.Name,
                      b.BucketGroupId,
                      b.ColorCode,
                      b.TextColorCode,
                      b.ValidFrom,
                      b.IsInactive,
                      b.IsInactiveFrom,
                      b.IsHiddenFromSummaries,
                      t.TransactionId AS Id,
                      t.AccountId,
                      t.TransactionDate,
                      t.Payee,
                      t.Memo,
                      t.Amount
                  FROM BudgetedTransaction bt
                  INNER JOIN Bucket b ON bt.BucketId = b.BucketId
                  INNER JOIN BankTransaction t ON bt.TransactionId = t.TransactionId
                  WHERE bt.BudgetedTransactionId = $id
                  """;

        var mapper = new BudgetedTransactionMapper();
        _ = _connection
            .Query<BudgetedTransaction, Bucket, BankTransaction, BudgetedTransaction>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id,Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public int Create(BudgetedTransaction entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO BudgetedTransaction (BudgetedTransactionId, TransactionId, BucketId, Amount)
                  VALUES ($1, $2, $3, $4)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.TransactionId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.BucketId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.Amount));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<BudgetedTransaction> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BudgetedTransaction entity)
    {
        throw new NotSupportedException(
            $"{typeof(BudgetedTransaction)} should not be updated, instead delete and re-create");
    }

    public int UpdateRange(IEnumerable<BudgetedTransaction> entities)
    {
        throw new NotSupportedException(
            $"{typeof(BudgetedTransaction)} should not be updated, instead delete and re-create");
    }

    public int Delete(Guid id)
    {
        // Consistency checks
        var entity = ById(id);
        if (entity is null) throw new Exception($"BudgetedTransaction with id {id} not found.");

        var sql = """
                  DELETE FROM BudgetedTransaction WHERE BudgetedTransactionId = $1
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
        if (entities.Count == 0) throw new Exception($"No BudgetedTransactions found with passed IDs.");

        return scope.Sum(Delete);
    }
}
