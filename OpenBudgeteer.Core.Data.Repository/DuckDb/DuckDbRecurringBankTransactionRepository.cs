using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;
using OpenBudgeteer.Core.Data.Repository.DuckDb.Mapper;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbRecurringBankTransactionRepository : IRecurringBankTransactionRepository
{
    private readonly DbConnection _connection;

    public DuckDbRecurringBankTransactionRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<RecurringBankTransaction> All()
    {
        var sql = """
                  SELECT
                      TransactionId AS Id
                      ,AccountId
                      ,RecurrenceType
                      ,RecurrenceAmount
                      ,FirstOccurrenceDate
                      ,Payee
                      ,Memo
                      ,Amount
                  FROM RecurringBankTransaction
                  """;
        return _connection.Query<RecurringBankTransaction>(sql).AsQueryable();
    }

    public IQueryable<RecurringBankTransaction> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      rbt.TransactionId AS Id
                      ,rbt.AccountId
                      ,rbt.RecurrenceType
                      ,rbt.RecurrenceAmount
                      ,rbt.FirstOccurrenceDate
                      ,rbt.Payee
                      ,rbt.Memo
                      ,rbt.Amount
                      ,a.AccountId AS Id
                      ,a.Name
                      ,a.IsActive
                  FROM RecurringBankTransaction rbt
                  INNER JOIN Account a ON rbt.AccountId = a.AccountId
                  """;

        var mapper = new RecurringBankTransactionMapper();
        _ = _connection
            .Query<RecurringBankTransaction, Account, RecurringBankTransaction>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id")
            .ToList();

        return mapper.Results.AsQueryable();
    }

    public RecurringBankTransaction? ById(Guid id)
    {
        var sql = """
                  SELECT
                      TransactionId AS Id
                      ,AccountId
                      ,RecurrenceType
                      ,RecurrenceAmount
                      ,FirstOccurrenceDate
                      ,Payee
                      ,Memo
                      ,Amount
                  FROM RecurringBankTransaction
                  WHERE TransactionId = $id
                  """;

        return _connection
            .Query<RecurringBankTransaction>(sql, param: new { id = id.ToString() })
            .FirstOrDefault();
    }

    public RecurringBankTransaction? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      rbt.TransactionId AS Id
                      ,rbt.AccountId
                      ,rbt.RecurrenceType
                      ,rbt.RecurrenceAmount
                      ,rbt.FirstOccurrenceDate
                      ,rbt.Payee
                      ,rbt.Memo
                      ,rbt.Amount
                      ,a.AccountId AS Id
                      ,a.Name
                      ,a.IsActive
                  FROM RecurringBankTransaction rbt
                  INNER JOIN Account a ON rbt.AccountId = a.AccountId
                  WHERE rbt.TransactionId = $id
                  """;

        var mapper = new RecurringBankTransactionMapper();
        _ = _connection
            .Query<RecurringBankTransaction, Account, RecurringBankTransaction>(
                sql,
                mapper.MapWithEverything,
                splitOn: "Id",
                param: new { id = id.ToString() })
            .ToList();

        return mapper.Results.FirstOrDefault();
    }

    public int Create(RecurringBankTransaction entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO RecurringBankTransaction (
                      TransactionId
                      ,AccountId
                      ,RecurrenceType
                      ,RecurrenceAmount
                      ,FirstOccurrenceDate
                      ,Payee
                      ,Memo
                      ,Amount)
                  VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.AccountId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.RecurrenceType));
        cmd.Parameters.Add(new DuckDBParameter(entity.RecurrenceAmount));
        cmd.Parameters.Add(new DuckDBParameter(entity.FirstOccurrenceDate.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Payee ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Memo ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.Amount));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<RecurringBankTransaction> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(RecurringBankTransaction entity)
    {
        var sql = """
                  UPDATE RecurringBankTransaction
                  SET
                      AccountId = $1
                      ,RecurrenceType = $2
                      ,RecurrenceAmount = $3
                      ,FirstOccurrenceDate = $4
                      ,Payee = $5
                      ,Memo = $6
                      ,Amount = $7
                  WHERE TransactionId = $8
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.AccountId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.RecurrenceType));
        cmd.Parameters.Add(new DuckDBParameter(entity.RecurrenceAmount));
        cmd.Parameters.Add(new DuckDBParameter(entity.FirstOccurrenceDate.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Payee ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Memo ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.Amount));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<RecurringBankTransaction> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        // Consistency checks
        var entity = ById(id);
        if (entity is null) throw new Exception($"RecurringBankTransaction with id {id} not found.");

        var sql = """
                  DELETE
                  FROM RecurringBankTransaction
                  WHERE TransactionId = $1
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
        if (entities.Count == 0) throw new Exception($"No RecurringBankTransactions found with passed IDs.");

        return scope.Sum(Delete);
    }
}
