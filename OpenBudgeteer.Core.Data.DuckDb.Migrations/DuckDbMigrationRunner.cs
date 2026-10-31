using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using DuckDB.NET.Data;

namespace OpenBudgeteer.Core.Data.DuckDb.Migrations;

public static class DuckDbMigrationRunner
{
    private const string MIGRATIONS_TABLE_NAME = "__DuckDbMigrations";

    // Define all migrations in order
    private static readonly List<string> _migrations =
    [
        "20251205000000_InitialCreate"
    ];

    /// <summary>
    /// Applies all pending migrations on the passed database file. If the database already contains applied
    /// migrations, a backup of the file is created as ".bak" before any pending migration is applied.
    /// </summary>
    /// <param name="databasePath">Path to the DuckDB database file</param>
    public static void ApplyMigrations(string databasePath) => ApplyMigrations(databasePath, _migrations);

    public static void ApplyMigrations(DbConnection connection) => ApplyMigrations(connection, _migrations);

    internal static void ApplyMigrations(string databasePath, IReadOnlyList<string> migrations)
    {
        if (File.Exists(databasePath))
        {
            bool isBackupRequired;
            using (var connection = new DuckDBConnection($"DataSource={databasePath}"))
            {
                connection.Open();
                EnsureMigrationsTableExists(connection);
                var appliedMigrations = GetAppliedMigrations(connection);
                isBackupRequired = appliedMigrations.Count > 0 && migrations.Any(i => !appliedMigrations.Contains(i));
            }

            // Connection has been closed so DuckDB has released the file, which allows a plain file copy
            if (isBackupRequired) BackupDatabaseFile(databasePath);
        }

        using var migrationConnection = new DuckDBConnection($"DataSource={databasePath}");
        ApplyMigrations(migrationConnection, migrations);
    }

    internal static void ApplyMigrations(DbConnection connection, IReadOnlyList<string> migrations)
    {
        if (connection.State != System.Data.ConnectionState.Open) connection.Open();

        // Ensure migrations table exists
        EnsureMigrationsTableExists(connection);

        // Get already applied migrations
        var appliedMigrations = GetAppliedMigrations(connection);

        // Apply pending migrations
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var migrationId in migrations)
        {
            if (appliedMigrations.Contains(migrationId)) continue; // Migration already applied

            var resourcePath = $"OpenBudgeteer.Core.Data.DuckDb.Migrations.{migrationId}.sql";
            var migrationSql = GetEmbeddedResource(assembly, resourcePath);

            if (string.IsNullOrEmpty(migrationSql))
            {
                // If not embedded, try to read from file
                var assemblyLocation = Path.GetDirectoryName(assembly.Location);
                var sqlFilePath = Path.Combine(assemblyLocation ?? ".", $"{migrationId}.sql");

                if (File.Exists(sqlFilePath))
                {
                    migrationSql = File.ReadAllText(sqlFilePath);
                }
            }

            if (string.IsNullOrEmpty(migrationSql)) continue;

            using var transaction = connection.BeginTransaction();
            ExecuteMigration(connection, transaction, migrationSql);
            RecordMigration(connection, transaction, migrationId);
            transaction.Commit();
        }
    }

    private static void BackupDatabaseFile(string databasePath)
    {
        var backupPath = $"{databasePath}.bak";
        File.Copy(databasePath, backupPath, true);

        // Include a remaining write-ahead log, otherwise remove a stale one from a previous backup
        var walPath = $"{databasePath}.wal";
        var backupWalPath = $"{backupPath}.wal";
        if (File.Exists(walPath)) File.Copy(walPath, backupWalPath, true);
        else if (File.Exists(backupWalPath)) File.Delete(backupWalPath);
    }

    private static void EnsureMigrationsTableExists(DbConnection connection)
    {
        var sql = $@"CREATE TABLE IF NOT EXISTS {MIGRATIONS_TABLE_NAME} (
            MigrationId VARCHAR PRIMARY KEY,
            AppliedOn TIMESTAMP NOT NULL
        )";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static HashSet<string> GetAppliedMigrations(DbConnection connection)
    {
        var appliedMigrations = new HashSet<string>();

        var sql = $"SELECT MigrationId FROM {MIGRATIONS_TABLE_NAME}";

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            appliedMigrations.Add(reader.GetString(0));
        }

        return appliedMigrations;
    }

    private static void RecordMigration(DbConnection connection, DbTransaction transaction, string migrationId)
    {
        var sql = $"INSERT INTO {MIGRATIONS_TABLE_NAME} (MigrationId, AppliedOn) VALUES ($1, $2)";

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;

        var p1 = command.CreateParameter();
        p1.Value = migrationId;
        command.Parameters.Add(p1);

        var p2 = command.CreateParameter();
        p2.Value = DateTime.UtcNow;
        command.Parameters.Add(p2);

        command.ExecuteNonQuery();
    }

    private static string? GetEmbeddedResource(Assembly assembly, string resourceName)
    {
        try
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    private static void ExecuteMigration(DbConnection connection, DbTransaction transaction, string sql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
