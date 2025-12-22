using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbAccountRepository : IAccountRepository
{
    private readonly DbConnection _connection;

    public DuckDbAccountRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<Account> All()
    {
        var sql = """
                  SELECT
                      AccountId AS Id
                      ,Name
                      ,IsActive
                  FROM Account
                  """;
        return _connection.Query<Account>(sql).AsQueryable();
    }

    public IQueryable<Account> AllWithIncludedEntities() => All();

    public Account? ById(Guid id)
    {
        var sql = """
                  SELECT
                      AccountId AS Id
                      ,Name
                      ,IsActive
                  FROM Account
                  WHERE AccountId = $id
                  """;

        return _connection
            .Query<Account>(sql, new { id = id.ToString() })
            .FirstOrDefault();
    }

    public Account? ByIdWithIncludedEntities(Guid id) => ById(id);

    public int Create(Account entity)
    {
        // TODO [Guid Gen-Check] Check if this is right or the other Guid Gen-Check
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO Account
                      (AccountId
                      ,Name
                      ,IsActive)
                  VALUES ($1, $2, $3)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Name ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsActive));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<Account> entities) => entities.Sum(Create);

    public int Update(Account entity)
    {
        // TODO Work on overall SQL formatting (SET values should wrap automatically to new line here)
        var sql = """
                  UPDATE Account
                  SET Name = $1
                      ,IsActive = $2
                  WHERE AccountId = $3
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter((object?)entity.Name ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsActive));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<Account> entities) => entities.Sum(Update);

    public int Delete(Guid id)
    {
        // Consistency checks
        var entity = ById(id);
        if (entity is null) throw new Exception($"Account with id {id} not found.");

        var sql = """
                  DELETE
                  FROM Account
                  WHERE AccountId = $1
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
        if (entities.Count == 0) throw new Exception($"No Account found with passed IDs.");

        return scope.Sum(Delete);
    }
}
