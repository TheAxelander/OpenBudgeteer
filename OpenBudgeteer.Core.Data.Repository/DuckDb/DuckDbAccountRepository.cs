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
                      AccountId AS Id,
                      Name,
                      IsActive
                  FROM Account
                  """;
        return _connection.Query<Account>(sql).AsQueryable();
    }

    public IQueryable<Account> AllWithIncludedEntities()
    {
        return All();
    }

    public Account? ById(Guid id)
    {
        var sql = """
                  SELECT
                      AccountId AS Id,
                      Name,
                      IsActive
                  FROM Account
                  WHERE AccountId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Account
            {
                Id = Guid.Parse(reader.GetString(0)),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                IsActive = reader.GetInt32(2)
            };
        }
        return null;
    }

    public Account? ByIdWithIncludedEntities(Guid id)
    {
        return ById(id);
    }

    public int Create(Account entity)
    {
        entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO Account (AccountId, Name, IsActive)
                  VALUES ($1, $2, $3)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.Name));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsActive));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<Account> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(Account entity)
    {
        var sql = """
                  UPDATE Account
                  SET
                      Name = $1,
                      IsActive = $2
                  WHERE AccountId = $3
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Name));
        cmd.Parameters.Add(new DuckDBParameter(entity.IsActive));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<Account> entities)
    {
        return entities.Sum(Update);
    }

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
