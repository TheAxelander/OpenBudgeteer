using System.Data.Common;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.Contracts.Repositories;
using OpenBudgeteer.Core.Data.Entities.Models;

namespace OpenBudgeteer.Core.Data.Repository.DuckDb;

public class DuckDbImportProfileRepository : IImportProfileRepository
{
    private readonly DbConnection _connection;

    public DuckDbImportProfileRepository(DbConnection connection)
    {
        _connection = connection;
    }

    public IQueryable<ImportProfile> All()
    {
        var sql = """
                  SELECT
                      ImportProfileId AS Id,
                      ProfileName,
                      AccountId,
                      HeaderRow,
                      Delimiter,
                      TextQualifier,
                      DateFormat,
                      NumberFormat,
                      TransactionDateColumnName,
                      PayeeColumnName,
                      MemoColumnName,
                      AmountColumnName,
                      AdditionalSettingCreditValue,
                      CreditColumnName,
                      CreditColumnIdentifierColumnName,
                      CreditColumnIdentifierValue,
                      AdditionalSettingAmountCleanup,
                      AdditionalSettingAmountCleanupValue
                  FROM ImportProfile
                  """;
        return _connection.Query<ImportProfile>(sql).AsQueryable();
    }

    public IQueryable<ImportProfile> AllWithIncludedEntities()
    {
        var sql = """
                  SELECT
                      ip.ImportProfileId AS Id,
                      ip.ProfileName,
                      ip.AccountId,
                      ip.HeaderRow,
                      ip.Delimiter,
                      ip.TextQualifier,
                      ip.DateFormat,
                      ip.NumberFormat,
                      ip.TransactionDateColumnName,
                      ip.PayeeColumnName,
                      ip.MemoColumnName,
                      ip.AmountColumnName,
                      ip.AdditionalSettingCreditValue,
                      ip.CreditColumnName,
                      ip.CreditColumnIdentifierColumnName,
                      ip.CreditColumnIdentifierValue,
                      ip.AdditionalSettingAmountCleanup,
                      ip.AdditionalSettingAmountCleanupValue,
                      a.AccountId AS Id,
                      a.Name,
                      a.IsActive
                  FROM ImportProfile ip
                  INNER JOIN Account a ON ip.AccountId = a.AccountId
                  """;

        var result = _connection.Query<ImportProfile, Account, ImportProfile>(
            sql,
            (importProfile, account) =>
            {
                importProfile.Account = account;
                return importProfile;
            },
            splitOn: "Id");

        return result.AsQueryable();
    }

    public ImportProfile? ById(Guid id)
    {
        var sql = """
                  SELECT
                      ImportProfileId AS Id,
                      ProfileName,
                      AccountId,
                      HeaderRow,
                      Delimiter,
                      TextQualifier,
                      DateFormat,
                      NumberFormat,
                      TransactionDateColumnName,
                      PayeeColumnName,
                      MemoColumnName,
                      AmountColumnName,
                      AdditionalSettingCreditValue,
                      CreditColumnName,
                      CreditColumnIdentifierColumnName,
                      CreditColumnIdentifierValue,
                      AdditionalSettingAmountCleanup,
                      AdditionalSettingAmountCleanupValue
                  FROM ImportProfile
                  WHERE ImportProfileId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new ImportProfile
            {
                Id = Guid.Parse(reader.GetString(0)),
                ProfileName = reader.IsDBNull(1) ? null : reader.GetString(1),
                AccountId = Guid.Parse(reader.GetString(2)),
                HeaderRow = reader.GetInt32(3),
                Delimiter = reader.GetString(4)[0],
                TextQualifier = reader.GetString(5)[0],
                DateFormat = reader.IsDBNull(6) ? null : reader.GetString(6),
                NumberFormat = reader.IsDBNull(7) ? null : reader.GetString(7),
                TransactionDateColumnName = reader.IsDBNull(8) ? null : reader.GetString(8),
                PayeeColumnName = reader.IsDBNull(9) ? null : reader.GetString(9),
                MemoColumnName = reader.IsDBNull(10) ? null : reader.GetString(10),
                AmountColumnName = reader.IsDBNull(11) ? null : reader.GetString(11),
                AdditionalSettingCreditValue = reader.GetInt32(12),
                CreditColumnName = reader.IsDBNull(13) ? null : reader.GetString(13),
                CreditColumnIdentifierColumnName = reader.IsDBNull(14) ? null : reader.GetString(14),
                CreditColumnIdentifierValue = reader.IsDBNull(15) ? null : reader.GetString(15),
                AdditionalSettingAmountCleanup = reader.GetBoolean(16),
                AdditionalSettingAmountCleanupValue = reader.IsDBNull(17) ? null : reader.GetString(17)
            };
        }
        return null;
    }

    public ImportProfile? ByIdWithIncludedEntities(Guid id)
    {
        var sql = """
                  SELECT
                      ip.ImportProfileId AS Id,
                      ip.ProfileName,
                      ip.AccountId,
                      ip.HeaderRow,
                      ip.Delimiter,
                      ip.TextQualifier,
                      ip.DateFormat,
                      ip.NumberFormat,
                      ip.TransactionDateColumnName,
                      ip.PayeeColumnName,
                      ip.MemoColumnName,
                      ip.AmountColumnName,
                      ip.AdditionalSettingCreditValue,
                      ip.CreditColumnName,
                      ip.CreditColumnIdentifierColumnName,
                      ip.CreditColumnIdentifierValue,
                      ip.AdditionalSettingAmountCleanup,
                      ip.AdditionalSettingAmountCleanupValue,
                      a.AccountId AS Id,
                      a.Name,
                      a.IsActive
                  FROM ImportProfile ip
                  INNER JOIN Account a ON ip.AccountId = a.AccountId
                  WHERE ip.ImportProfileId = $1
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(id.ToString()));

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        var importProfile = new ImportProfile
        {
            Id = Guid.Parse(reader.GetString(0)),
            ProfileName = reader.IsDBNull(1) ? null : reader.GetString(1),
            AccountId = Guid.Parse(reader.GetString(2)),
            HeaderRow = reader.GetInt32(3),
            Delimiter = reader.GetString(4)[0],
            TextQualifier = reader.GetString(5)[0],
            DateFormat = reader.IsDBNull(6) ? null : reader.GetString(6),
            NumberFormat = reader.IsDBNull(7) ? null : reader.GetString(7),
            TransactionDateColumnName = reader.IsDBNull(8) ? null : reader.GetString(8),
            PayeeColumnName = reader.IsDBNull(9) ? null : reader.GetString(9),
            MemoColumnName = reader.IsDBNull(10) ? null : reader.GetString(10),
            AmountColumnName = reader.IsDBNull(11) ? null : reader.GetString(11),
            AdditionalSettingCreditValue = reader.GetInt32(12),
            CreditColumnName = reader.IsDBNull(13) ? null : reader.GetString(13),
            CreditColumnIdentifierColumnName = reader.IsDBNull(14) ? null : reader.GetString(14),
            CreditColumnIdentifierValue = reader.IsDBNull(15) ? null : reader.GetString(15),
            AdditionalSettingAmountCleanup = reader.GetBoolean(16),
            AdditionalSettingAmountCleanupValue = reader.IsDBNull(17) ? null : reader.GetString(17),
            Account = new Account
            {
                Id = Guid.Parse(reader.GetString(18)),
                Name = reader.IsDBNull(19) ? null : reader.GetString(19),
                IsActive = reader.GetInt32(20)
            }
        };
        return importProfile;
    }

    public int Create(ImportProfile entity)
    {
        if (entity.Id == Guid.Empty) entity.Id = Guid.NewGuid();

        var sql = """
                  INSERT INTO ImportProfile (
                      ImportProfileId,
                      ProfileName,
                      AccountId,
                      HeaderRow,
                      Delimiter,
                      TextQualifier,
                      DateFormat,
                      NumberFormat,
                      TransactionDateColumnName,
                      PayeeColumnName,
                      MemoColumnName,
                      AmountColumnName,
                      AdditionalSettingCreditValue,
                      CreditColumnName,
                      CreditColumnIdentifierColumnName,
                      CreditColumnIdentifierValue,
                      AdditionalSettingAmountCleanup,
                      AdditionalSettingAmountCleanupValue)
                  VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16, $17, $18)
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.ProfileName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.AccountId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.HeaderRow));
        cmd.Parameters.Add(new DuckDBParameter(entity.Delimiter.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.TextQualifier.ToString()));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.DateFormat ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.NumberFormat ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.TransactionDateColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.PayeeColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.MemoColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.AmountColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.AdditionalSettingCreditValue));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.CreditColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.CreditColumnIdentifierColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.CreditColumnIdentifierValue ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.AdditionalSettingAmountCleanup));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.AdditionalSettingAmountCleanupValue ?? DBNull.Value));

        return cmd.ExecuteNonQuery();
    }

    public int CreateRange(IEnumerable<ImportProfile> entities)
    {
        return entities.Sum(Create);
    }

    public int Update(ImportProfile entity)
    {
        var sql = """
                  UPDATE ImportProfile
                  SET
                      ProfileName = $1,
                      AccountId = $2,
                      HeaderRow = $3,
                      Delimiter = $4,
                      TextQualifier = $5,
                      DateFormat = $6,
                      NumberFormat = $7,
                      TransactionDateColumnName = $8,
                      PayeeColumnName = $9,
                      MemoColumnName = $10,
                      AmountColumnName = $11,
                      AdditionalSettingCreditValue = $12,
                      CreditColumnName = $13,
                      CreditColumnIdentifierColumnName = $14,
                      CreditColumnIdentifierValue = $15,
                      AdditionalSettingAmountCleanup = $16,
                      AdditionalSettingAmountCleanupValue = $17
                  WHERE ImportProfileId = $18
                  """;

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new DuckDBParameter((object?)entity.ProfileName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.AccountId.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.HeaderRow));
        cmd.Parameters.Add(new DuckDBParameter(entity.Delimiter.ToString()));
        cmd.Parameters.Add(new DuckDBParameter(entity.TextQualifier.ToString()));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.DateFormat ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.NumberFormat ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.TransactionDateColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.PayeeColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.MemoColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.AmountColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.AdditionalSettingCreditValue));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.CreditColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.CreditColumnIdentifierColumnName ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.CreditColumnIdentifierValue ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.AdditionalSettingAmountCleanup));
        cmd.Parameters.Add(new DuckDBParameter((object?)entity.AdditionalSettingAmountCleanupValue ?? DBNull.Value));
        cmd.Parameters.Add(new DuckDBParameter(entity.Id.ToString()));

        return cmd.ExecuteNonQuery();
    }

    public int UpdateRange(IEnumerable<ImportProfile> entities)
    {
        return entities.Sum(Update);
    }

    public int Delete(Guid id)
    {
        // Consistency checks
        var entity = ById(id);
        if (entity is null) throw new Exception($"ImportProfile with id {id} not found.");

        var sql = """
                  DELETE FROM ImportProfile WHERE ImportProfileId = $1
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
        if (entities.Count == 0) throw new Exception("No ImportProfiles found with passed IDs.");

        return scope.Sum(Delete);
    }
}
