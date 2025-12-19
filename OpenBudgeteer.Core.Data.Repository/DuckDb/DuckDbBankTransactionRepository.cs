using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbBankTransactionRepository : IBankTransactionRepository
{
    private readonly DbConnection _connection;

    public DuckDbBankTransactionRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<BankTransaction> All()
    {
        var sql = """
                  SELECT
                      TransactionId AS Id,
                      AccountId,
                      TransactionDate,
                      Payee,
                      Memo,
                      Amount
                  FROM BankTransaction
                  """;
        return _connection.Query<BankTransaction>(sql).AsQueryable();
    }

    public IQueryable<BankTransaction> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      bt.TransactionId AS Id,
                      bt.AccountId,
                      bt.TransactionDate,
                      bt.Payee,
                      bt.Memo,
                      bt.Amount,
                      a.AccountId AS Id,
                      a.Name,
                      a.IsActive
                  FROM BankTransaction bt
                  INNER JOIN Account a ON bt.AccountId = a.AccountId
                  """;

        var result = _connection.Query<BankTransaction, Account, BankTransaction>(
            sql,
            (transaction, account) =>
            {
                transaction.Account = account;
                return transaction;
            },
            splitOn: "Id");

        return result.AsQueryable();
    }

    public BankTransaction? ById(Guid id)
    {
        var sql = """
                  SELECT
                      TransactionId AS Id,
                      AccountId,
                      TransactionDate,
                      Payee,
                      Memo,
                      Amount
                  FROM BankTransaction
                  WHERE TransactionId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new BankTransaction
            {
                Id = Guid.Parse(reader.GetString(0)),
                AccountId = Guid.Parse(reader.GetString(1)),
                TransactionDate = DateOnly.FromDateTime(reader.GetDateTime(2)),
                Payee = reader.IsDBNull(3) ? null : reader.GetString(3),
                Memo = reader.IsDBNull(4) ? null : reader.GetString(4),
                Amount = reader.GetDecimal(5)
            };
        }
        return null;
    }

    public BankTransaction? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      bt.TransactionId AS Id,
                      bt.AccountId,
                      bt.TransactionDate,
                      bt.Payee,
                      bt.Memo,
                      bt.Amount,
                      a.AccountId AS Id,
                      a.Name,
                      a.IsActive
                  FROM BankTransaction bt
                  INNER JOIN Account a ON bt.AccountId = a.AccountId
                  WHERE bt.TransactionId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;

        var transaction = new BankTransaction
        {
            Id = Guid.Parse(reader.GetString(0)),
            AccountId = Guid.Parse(reader.GetString(1)),
            TransactionDate = DateOnly.FromDateTime(reader.GetDateTime(2)),
            Payee = reader.IsDBNull(3) ? null : reader.GetString(3),
            Memo = reader.IsDBNull(4) ? null : reader.GetString(4),
            Amount = reader.GetDecimal(5),
            Account = new Account
            {
                Id = Guid.Parse(reader.GetString(6)),
                Name = reader.IsDBNull(7) ? null : reader.GetString(7),
                IsActive = reader.GetInt32(8)
            }
        };
        return transaction;
    }

    public int Create(BankTransaction entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO BankTransaction (TransactionId, AccountId, TransactionDate, Payee, Memo, Amount)
                  VALUES ($1, $2, $3, $4, $5, $6)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.AccountId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.TransactionDate.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Payee ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Memo ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.Amount));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<BankTransaction> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(BankTransaction entity)
    {
        var sql = """
                  UPDATE BankTransaction
                  SET AccountId = $1, TransactionDate = $2, Payee = $3, Memo = $4, Amount = $5
                  WHERE TransactionId = $6
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.AccountId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.TransactionDate.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Payee ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Memo ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.Amount));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<BankTransaction> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        // Consistency checks
        var entity = ByIdWithIncludedEntities(id);
        if (entity is null) throw new Exception($"BankTransaction with id {id} not found.");

        var result = 0;

        // Delete related BudgetedTransactions
        if (entity.BudgetedTransactions is not null)
        {
            var budgetedTransactionRepository = new DuckDbBudgetedTransactionRepository(_connection);
            result += budgetedTransactionRepository.DeleteRange(entity.BudgetedTransactions.Select(i  => i.Id));
        }

        // Delete BankTransaction
        var sql = """
                  DELETE FROM BankTransaction WHERE TransactionId = $1
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
        if (entities.Count == 0) throw new Exception("No BankTransactions found with passed IDs.");

        return scope.Sum(Delete);
    }
}
