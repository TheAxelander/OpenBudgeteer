using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using OpenBudgeteer.Core.Data.Entities;

namespace OpenBudgeteer.Core.Data;

// Creates the DBContext options for runtime and design-time DBContext initialization
// Generates the connection string from the supplied ConfigMap
// Ensures that there's no SQLi using the DBName parameter in the config map for Postgres and MariaDB
// Ensures directory tree created for SQLite
public static partial class DbContextOptionsFactory
{
    private static readonly Dictionary<string, Action<DbContextOptionsBuilder, IConfiguration>> OptionsFactoryLookup = new(StringComparer.OrdinalIgnoreCase)
    {
        [ConfigurationKeyConstants.PROVIDER_TEMPDB] = SetupSqliteTempDbConnection,
        [ConfigurationKeyConstants.PROVIDER_SQLITE] = SetupSqliteConnection,
        [ConfigurationKeyConstants.PROVIDER_MYSQL] = SetupMariaDbConnection,
        [ConfigurationKeyConstants.PROVIDER_MARIADB] = SetupMariaDbConnection,
        [ConfigurationKeyConstants.PROVIDER_POSTGRES] = SetupPostgresConnection,
        [ConfigurationKeyConstants.PROVIDER_POSTGRESQL] = SetupPostgresConnection,
    };

    public static DbContextOptions<DatabaseContext> GetContextOptions(IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_PROVIDER);
        if (string.IsNullOrEmpty(provider)) throw new Exception("Database provider not defined.");

        provider = provider.Trim();
        if (!OptionsFactoryLookup.TryGetValue(provider, out var optionsFactoryMethod))
            throw new NotSupportedException($"Database provider {provider} is not supported.");
        
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsFactoryMethod(optionsBuilder, configuration);
        
#if DEBUG
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif
        
        return optionsBuilder.Options;
    }
    
    private static void SetupSqliteTempDbConnection(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        var dbFilePath = Path.GetTempFileName();
        optionsBuilder.UseSqlite(
            $"Data Source={dbFilePath}",
            b => b.MigrationsAssembly("OpenBudgeteer.Core.Data.Sqlite.Migrations"));
    }

    private static void SetupSqliteConnection(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        var dbFilePath = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_DATABASE);
        dbFilePath = string.IsNullOrWhiteSpace(dbFilePath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "database", "openbudgeteer.db") 
            : Path.GetFullPath(dbFilePath);

        var directory = Path.GetDirectoryName(dbFilePath);
        if (string.IsNullOrEmpty(directory)) throw new Exception("Unable to operate on provided directory");
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        var connectionString = $"Data Source={dbFilePath}";
        optionsBuilder.UseSqlite(
                connectionString,
                b => b.MigrationsAssembly("OpenBudgeteer.Core.Data.Sqlite.Migrations"));
    }

    private static void SetupMariaDbConnection(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        var databaseName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_DATABASE, "openbudgeteer");
        if (!DatabaseNameRegex().IsMatch(databaseName!))
        {
            throw new InvalidOperationException("Database name provided is illegal or SQLi attempt");
        }

        var userName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_USER, databaseName);
        if (!DatabaseNameRegex().IsMatch(userName!))
        {
            throw new InvalidOperationException("User name provided is illegal or SQLi attempt");
        }
        
        var builder = new MySqlConnectionStringBuilder
        {
            Server = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PORT, 3306u),
            Database = databaseName,
            UserID = userName,
            Password = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PASSWORD, userName),
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };

        var serverVersion = ServerVersion.AutoDetect(builder.ConnectionString);
        optionsBuilder.UseMySql(
                builder.ConnectionString,
                serverVersion,
                b => b.MigrationsAssembly("OpenBudgeteer.Core.Data.MySql.Migrations"));

    }

    private static void SetupPostgresConnection(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
    {
        var databaseName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_DATABASE, "postgres");
        if (!DatabaseNameRegex().IsMatch(databaseName!))
        {
            throw new InvalidOperationException("Database name provided is illegal or SQLi attempt");
        }

        var userName = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_USER, databaseName);
        if (!DatabaseNameRegex().IsMatch(userName!))
        {
            throw new InvalidOperationException("User name provided is illegal or SQLi attempt");
        }

        var password = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PASSWORD, string.Empty);
        var rootPassword = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_ROOT_PASSWORD, string.Empty);
        if (databaseName!.Equals("postgres", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(password))
        {
            password = rootPassword;
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_SERVER, "localhost"),
            Port = configuration.GetValue(ConfigurationKeyConstants.CONNECTION_PORT, 5432),
            Database = databaseName,
            Username = userName,
            Password = password
        };

        optionsBuilder.UseNpgsql(
            builder.ConnectionString,
            b => b.MigrationsAssembly("OpenBudgeteer.Core.Data.Postgres.Migrations"));
    }

    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_-]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}