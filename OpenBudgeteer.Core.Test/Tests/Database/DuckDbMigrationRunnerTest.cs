using System;
using System.IO;
using Dapper;
using DuckDB.NET.Data;
using OpenBudgeteer.Core.Data.DuckDb.Migrations;
using Xunit;

namespace OpenBudgeteer.Core.Test.Tests.Database;

public class DuckDbMigrationRunnerTest : IDisposable
{
    private const string INITIAL_MIGRATION = "20251205000000_InitialCreate";

    private readonly string _directory;
    private readonly string _databasePath;
    private string BackupPath => $"{_databasePath}.bak";

    public DuckDbMigrationRunnerTest()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"OpenBudgeteer.Test.{Guid.NewGuid()}");
        Directory.CreateDirectory(_directory);
        _databasePath = Path.Combine(_directory, "OpenBudgeteer.duckdb");
    }

    public void Dispose()
    {
        Directory.Delete(_directory, true);
    }

    [Fact]
    public void ApplyMigrations_NewDatabase_CreatesNoBackup()
    {
        DuckDbMigrationRunner.ApplyMigrations(_databasePath);

        Assert.True(File.Exists(_databasePath));
        Assert.False(File.Exists(BackupPath));
    }

    [Fact]
    public void ApplyMigrations_UpToDateDatabase_CreatesNoBackup()
    {
        DuckDbMigrationRunner.ApplyMigrations(_databasePath);
        DuckDbMigrationRunner.ApplyMigrations(_databasePath);

        Assert.False(File.Exists(BackupPath));
    }

    [Fact]
    public void ApplyMigrations_PendingMigration_CreatesBackup()
    {
        DuckDbMigrationRunner.ApplyMigrations(_databasePath);
        using (var connection = new DuckDBConnection($"DataSource={_databasePath}"))
        {
            connection.Open();
            connection.Execute("INSERT INTO Account VALUES (uuid(), 'Backup Test', 1)");
        }

        DuckDbMigrationRunner.ApplyMigrations(_databasePath, [INITIAL_MIGRATION, "99991231000000_PendingMigration"]);

        Assert.True(File.Exists(BackupPath));
        Assert.False(File.Exists($"{BackupPath}.wal")); // Database has been checkpointed before the backup
        using var backupConnection = new DuckDBConnection($"DataSource={BackupPath};ACCESS_MODE=READ_ONLY");
        backupConnection.Open();
        Assert.Equal(1, backupConnection.ExecuteScalar<long>("SELECT count(*) FROM Account"));
        Assert.Equal(INITIAL_MIGRATION, backupConnection.ExecuteScalar<string>("SELECT MigrationId FROM __DuckDbMigrations"));
    }
}
