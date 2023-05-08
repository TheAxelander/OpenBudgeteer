using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace OpenBudgeteer.Data;

public static partial class DatabaseInitializationExtensions
{
    private const string CONNECTION_PROVIDER = "CONNECTION_PROVIDER";
    private const string CONNECTION_SERVER = "CONNECTION_SERVER";
    private const string CONNECTION_PORT = "CONNECTION_PORT";
    private const string CONNECTION_DATABASE = "CONNECTION_DATABASE";
    private const string CONNECTION_USER = "CONNECTION_USER";
    private const string CONNECTION_PASSWORD = "CONNECTION_PASSWORD";
    private const string CONNECTION_MYSQL_ROOT_PASSWORD = "CONNECTION_MYSQL_ROOT_PASSWORD";

    private const string PROVIDER_SQLITE = "SQLITE";
    private const string PROVIDER_MYSQL = "MYSQL";
    private const string PROVIDER_MARIADB = "MARIADB";
    
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>(CONNECTION_PROVIDER).Trim();
        if (provider.Equals(PROVIDER_SQLITE, StringComparison.OrdinalIgnoreCase))
        {
            SetupSqliteConnection(services, configuration);
            services.AddTransient<IDatabaseInitializer, NoOpDatabaseInitializer>();
        }
        else if (provider.Equals(PROVIDER_MYSQL, StringComparison.OrdinalIgnoreCase))
        {
            var dbInitializer = new MariaDbDatabaseInitializer();
            services.AddSingleton<IDatabaseInitializer>(dbInitializer);
            dbInitializer.InitializeDatabase(configuration);
            
            SetupMariaDbConnection(services, configuration);
        }
        else if (provider.Equals(PROVIDER_MARIADB, StringComparison.OrdinalIgnoreCase))
        {
            var dbInitializer = new MariaDbDatabaseInitializer();
            services.AddSingleton<IDatabaseInitializer>(dbInitializer);
            dbInitializer.InitializeDatabase(configuration);
            
            SetupMariaDbConnection(services, configuration);
        }
        else
        {
            throw new ArgumentOutOfRangeException($"Database provider {provider} not supported");
        }

        return services;
    }
    
    private static void SetupSqliteConnection(IServiceCollection services, IConfiguration configuration)
    {
        var dbFilePath = configuration.GetValue<string>(CONNECTION_DATABASE);
        if (string.IsNullOrWhiteSpace(dbFilePath)) dbFilePath = Path.Combine(Directory.GetCurrentDirectory(), "database", "openbudgeteer.db");
        Path.GetFullPath(dbFilePath);

        var connectionString = $"Data Source={dbFilePath}";
        services.AddDbContext<DatabaseContext>(options => options.UseSqlite(
                connectionString,
                b => b.MigrationsAssembly("OpenBudgeteer.Data.Sqlite.Migrations")),
            ServiceLifetime.Transient);
    }

    private static void SetupMariaDbConnection(IServiceCollection services, IConfiguration configuration)
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
            Password = configuration.GetValue<string>(CONNECTION_PASSWORD),
            ConnectionProtocol = MySqlConnectionProtocol.Tcp
        };

        var serverVersion = ServerVersion.AutoDetect(builder.ConnectionString);
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseMySql(
                builder.ConnectionString,
                serverVersion,
                b => b.MigrationsAssembly("OpenBudgeteer.Data.MySql.Migrations"));
#if Debug
            options.LogTo(Console.WriteLine, LogLevel.Information);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        }, ServiceLifetime.Transient);
    }
    
    [GeneratedRegex("^[a-zA-Z][0-9a-zA-Z$_]{0,63}$", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DatabaseNameRegex();
}