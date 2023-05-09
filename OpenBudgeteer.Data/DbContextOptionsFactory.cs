using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;

namespace OpenBudgeteer.Data;

public static partial class DbContextOptionsFactory
{
    private const string CONNECTION_PROVIDER = "CONNECTION_PROVIDER";
    private const string CONNECTION_SERVER = "CONNECTION_SERVER";
    private const string CONNECTION_PORT = "CONNECTION_PORT";
    private const string CONNECTION_DATABASE = "CONNECTION_DATABASE";
    private const string CONNECTION_USER = "CONNECTION_USER";
    private const string CONNECTION_PASSWORD = "CONNECTION_PASSWORD";

    private static readonly Dictionary<string, Action<DbContextOptionsBuilder, IConfiguration>> OptionsFactoryLookup = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MEMORY"] = SetupSqliteInMemoryConnection,
        ["TEMPDB"] = SetupSqliteTempDbConnection,
        ["SQLITE"] = SetupSqliteConnection,
        ["MYSQL"] = SetupMariaDbConnection,
        ["MARIADB"] = SetupMariaDbConnection,
        ["POSTGRES"] = SetupPostgresConnection,
        ["POSTGRESQL"] = SetupPostgresConnection,
    };

    public static DbContextOptions<DatabaseContext> GetContextOptions(IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>(CONNECTION_PROVIDER).Trim();
        if (!OptionsFactoryLookup.TryGetValue(provider, out var optionsFactoryMethod))
        {
            throw new NotSupportedException($"Database provider {provider} is not supported.");
        }
        
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsFactoryMethod(optionsBuilder, configuration);
        
#if DEBUG
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif
        
        return optionsBuilder.Options;
    }

    private static void SetupSqliteInMemoryConnection(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        optionsBuilder.UseSqlite(
            "Data Source=:memory:",
            b => b.MigrationsAssembly("OpenBudgeteer.Data.Sqlite.Migrations"));
    }
    
    private static void SetupSqliteTempDbConnection(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        var dbFilePath = Path.GetTempFileName();
        optionsBuilder.UseSqlite(
            $"Data Source={dbFilePath}",
            b => b.MigrationsAssembly("OpenBudgeteer.Data.Sqlite.Migrations"));
    }

    private static void SetupSqliteConnection(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        var dbFilePath = configuration.GetValue<string>(CONNECTION_DATABASE);
        if (string.IsNullOrWhiteSpace(dbFilePath)) dbFilePath = Path.Combine(Directory.GetCurrentDirectory(), "database", "openbudgeteer.db");
        Path.GetFullPath(dbFilePath);

        var connectionString = $"Data Source={dbFilePath}";
        optionsBuilder.UseSqlite(
                connectionString,
                b => b.MigrationsAssembly("OpenBudgeteer.Data.Sqlite.Migrations"));
    }

    private static void SetupMariaDbConnection(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        var databaseName = configuration.GetValue(CONNECTION_DATABASE, "openbudgeteer");
        if (!DatabaseNameRegex().IsMatch(databaseName))
        {
            throw new InvalidOperationException("Database name provided is illegal or SQLi attempt");
        }

        var userName = configuration.GetValue(CONNECTION_USER, databaseName);
        if (!DatabaseNameRegex().IsMatch(userName))
        {
            throw new InvalidOperationException("User name provided is illegal or SQLi attempt");
        }
        
        var builder = new MySqlConnectionStringBuilder
        {
            Server = configuration.GetValue(CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(CONNECTION_PORT, 3306u),
            Database = databaseName,
            UserID = userName,
            Password = configuration.GetValue(CONNECTION_PASSWORD, userName),
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };

        var serverVersion = ServerVersion.AutoDetect(builder.ConnectionString);
        optionsBuilder.UseMySql(
                builder.ConnectionString,
                serverVersion,
                b => b.MigrationsAssembly("OpenBudgeteer.Data.MySql.Migrations"));

    }

    private static void SetupPostgresConnection(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        var databaseName = configuration.GetValue(CONNECTION_DATABASE, "postgres");
        if (!DatabaseNameRegex().IsMatch(databaseName))
        {
            throw new InvalidOperationException("Database name provided is illegal or SQLi attempt");
        }

        var userName = configuration.GetValue(CONNECTION_USER, databaseName);
        if (!DatabaseNameRegex().IsMatch(userName))
        {
            throw new InvalidOperationException("User name provided is illegal or SQLi attempt");
        }
        
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration.GetValue(CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(CONNECTION_PORT, 5432),
            Database = databaseName,
            Username = userName,
            Password = configuration.GetValue<string>(CONNECTION_PASSWORD, null)
        };

        optionsBuilder.UseNpgsql(builder.ConnectionString);
    }

    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}