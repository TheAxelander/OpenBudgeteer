using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Reflection;

namespace OpenBudgeteer.Core.Data.DuckDb.Migrations;

public static class DuckDbMigrationRunner
{
    private const string MigrationsTableName = "__DuckDbMigrations";

    // Define all migrations in order
    private static readonly List<string> Migrations = new()
    {
        "20251205000000_InitialCreate"
    };

    public static void ApplyMigrations(DbConnection connection)
    {
        if (connection.State != System.Data.ConnectionState.Open) connection.Open();

        // Ensure migrations table exists
        EnsureMigrationsTableExists(connection);

        // Get already applied migrations
        var appliedMigrations = GetAppliedMigrations(connection);

        // Apply pending migrations
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var migrationId in Migrations)
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
            ExecuteMigration(connection, migrationSql);
            RecordMigration(connection, migrationId);
        }
    }

    private static void EnsureMigrationsTableExists(DbConnection connection)
    {
        var sql = $@"CREATE TABLE IF NOT EXISTS {MigrationsTableName} (
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

        var sql = $"SELECT MigrationId FROM {MigrationsTableName}";

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            appliedMigrations.Add(reader.GetString(0));
        }

        return appliedMigrations;
    }

    private static void RecordMigration(DbConnection connection, string migrationId)
    {
        var sql = $"INSERT INTO {MigrationsTableName} (MigrationId, AppliedOn) VALUES ($1, $2)";

        using var command = connection.CreateCommand();
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

    private static void ExecuteMigration(DbConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}