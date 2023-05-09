using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace OpenBudgeteer.Data;

public static partial class DbContextOptionsFactory
{
    private const string CONNECTION_PROVIDER = "CONNECTION_PROVIDER";
    private const string CONNECTION_SERVER = "CONNECTION_SERVER";
    private const string CONNECTION_PORT = "CONNECTION_PORT";
    private const string CONNECTION_DATABASE = "CONNECTION_DATABASE";
    private const string CONNECTION_USER = "CONNECTION_USER";
    private const string CONNECTION_PASSWORD = "CONNECTION_PASSWORD";

    private const string PROVIDER_SQLITE = "SQLITE";
    private const string PROVIDER_MYSQL = "MYSQL";
    private const string PROVIDER_MARIADB = "MARIADB";
    
    public static DbContextOptions<DatabaseContext> GetContextOptions(IConfiguration configuration)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        var provider = configuration.GetValue<string>(CONNECTION_PROVIDER).Trim();

        if (provider.Equals(PROVIDER_SQLITE, StringComparison.OrdinalIgnoreCase))
        {
            SetupSqliteConnection(optionsBuilder, configuration);
        }
        else if (provider.Equals(PROVIDER_MYSQL, StringComparison.OrdinalIgnoreCase))
        {
            SetupMariaDbConnection(optionsBuilder, configuration);
        }
        else if (provider.Equals(PROVIDER_MARIADB, StringComparison.OrdinalIgnoreCase))
        {
            SetupMariaDbConnection(optionsBuilder, configuration);
        }
        else
        {
            throw new ArgumentOutOfRangeException($"Database provider {provider} not supported");
        }

        return optionsBuilder.Options;
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
#if DEBUG
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif
    }
    
    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}