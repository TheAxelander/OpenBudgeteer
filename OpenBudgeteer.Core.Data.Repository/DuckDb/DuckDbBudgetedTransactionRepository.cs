using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

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

        var result = _connection.Query<BudgetedTransaction, Bucket, BankTransaction, BudgetedTransaction>(
            sql,
            (budgetedTransaction, bucket, bankTransaction) =>
            {
                budgetedTransaction.Bucket = bucket;
                budgetedTransaction.Transaction = bankTransaction;
                return budgetedTransaction;
            },
            splitOn: "Id,Id");

        return result.AsQueryable();
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

        var result = _connection.Query<BudgetedTransaction, Bucket, BankTransaction, Account, BudgetedTransaction>(
            sql,
            (budgetedTransaction, bucket, bankTransaction, account) =>
            {
                budgetedTransaction.Bucket = bucket;
                budgetedTransaction.Transaction = bankTransaction;
                budgetedTransaction.Transaction.Account = account;
                return budgetedTransaction;
            },
            splitOn: "Id,Id,Id");

        return result.AsQueryable();
    }

    public BudgetedTransaction? ById(Guid id)
    {
        var sql = """
                  SELECT BudgetedTransactionId AS Id, TransactionId, BucketId, Amount
                  FROM BudgetedTransaction
                  WHERE BudgetedTransactionId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new BudgetedTransaction
            {
                Id = Guid.Parse(reader.GetString(0)),
                TransactionId = Guid.Parse(reader.GetString(1)),
                BucketId = Guid.Parse(reader.GetString(2)),
                Amount = reader.GetDecimal(3)
            };
        }
        return null;
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
                  WHERE bt.BudgetedTransactionId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        var budgetedTransaction = new BudgetedTransaction
        {
            Id = Guid.Parse(reader.GetString(0)),
            TransactionId = Guid.Parse(reader.GetString(1)),
            BucketId = Guid.Parse(reader.GetString(2)),
            Amount = reader.GetDecimal(3),
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
            },
            Transaction = new BankTransaction
            {
                Id = Guid.Parse(reader.GetString(13)),
                AccountId = Guid.Parse(reader.GetString(14)),
                TransactionDate = DateOnly.FromDateTime(reader.GetDateTime(15)),
                Payee = reader.IsDBNull(16) ? null : reader.GetString(16),
                Memo = reader.IsDBNull(17) ? null : reader.GetString(17),
                Amount = reader.GetDecimal(18),
                Account = new Account
                {
                    Id = Guid.Parse(reader.GetString(19)),
                    Name = reader.IsDBNull(20) ? null : reader.GetString(20),
                    IsActive = reader.GetInt32(21)
                }
            }
        };
        return budgetedTransaction;
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
                  WHERE bt.BudgetedTransactionId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        var budgetedTransaction = new BudgetedTransaction
        {
            Id = Guid.Parse(reader.GetString(0)),
            TransactionId = Guid.Parse(reader.GetString(1)),
            BucketId = Guid.Parse(reader.GetString(2)),
            Amount = reader.GetDecimal(3),
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
            },
            Transaction = new BankTransaction
            {
                Id = Guid.Parse(reader.GetString(13)),
                AccountId = Guid.Parse(reader.GetString(14)),
                TransactionDate = DateOnly.FromDateTime(reader.GetDateTime(15)),
                Payee = reader.IsDBNull(16) ? null : reader.GetString(16),
                Memo = reader.IsDBNull(17) ? null : reader.GetString(17),
                Amount = reader.GetDecimal(18)
            }
        };
        return budgetedTransaction;
    }

    public int Create(BudgetedTransaction entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

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
