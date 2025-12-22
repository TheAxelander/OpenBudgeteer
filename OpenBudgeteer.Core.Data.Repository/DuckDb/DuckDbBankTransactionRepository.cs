using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

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

        var mapper = new BankTransactionMapper();
        _ = _connection
            .Query<BankTransaction, Account, BankTransaction>(
                sql,
                mapper.MapWithAccount,
                splitOn: "Id")
            .ToList();

        return mapper.Results.AsQueryable();
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
                  WHERE TransactionId = $id
                  """;

        return _connection
            .Query<BankTransaction>(sql, param: new { id = id.ToString() })
            .FirstOrDefault();
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
                  WHERE bt.TransactionId = $id
                  """;

        var mapper = new BankTransactionMapper();
        _ = _connection
            .Query<BankTransaction, Account, BankTransaction>(
                sql,
                mapper.MapWithAccount,
                splitOn: "Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
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
