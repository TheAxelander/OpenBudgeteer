using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenBudgeteer.Core.Data.Connection;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Initialization;

namespace OpenBudgeteer.Core.Data;

// Inject the required DBContext and the DBContext initializer services into the standardized DI container.
public static class DatabaseInitializationExtensions
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Identify Database provider
        var provider = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_PROVIDER);
        if (string.IsNullOrEmpty(provider)) throw new Exception("Database provider not defined.");
        provider = provider.Trim().ToUpper();
        
        IDatabaseConnector<DbConnectionStringBuilder> databaseConnector = provider switch
        {
            ConfigurationKeyConstants.PROVIDER_MARIADB => new MariaDbConnector(configuration),
            ConfigurationKeyConstants.PROVIDER_MYSQL => new MariaDbConnector(configuration),
            ConfigurationKeyConstants.PROVIDER_POSTGRES => new PostgresConnector(configuration),
            ConfigurationKeyConstants.PROVIDER_POSTGRESQL => new PostgresConnector(configuration),
            _ => throw new NotSupportedException("Database provider not supported.")
        };

        // Check connectivity
        var isOnline = databaseConnector.IsDatabaseOnline();
        if (!isOnline)
        {
            throw new InvalidOperationException("Target database is not online.");
        }

        var isRootPasswordEmpty = string.IsNullOrWhiteSpace(configuration.GetValue(ConfigurationKeyConstants.CONNECTION_ROOT_PASSWORD, string.Empty));
        var isAccessible = databaseConnector.IsDatabaseAccessible(!isRootPasswordEmpty);
        if (!isAccessible)
        {
            throw new InvalidOperationException("Target database is not accessible.");
        }
        
        // (Optionally) Create Database & User
        IDatabaseInitializer initializer = provider switch
        {
            _ when isRootPasswordEmpty => new NoOpDatabaseInitializer(), // Short circuit when user did not provide root password.
            ConfigurationKeyConstants.PROVIDER_MARIADB => new MariaDbDatabaseInitializer(),
            ConfigurationKeyConstants.PROVIDER_MYSQL => new MariaDbDatabaseInitializer(),
            ConfigurationKeyConstants.PROVIDER_POSTGRES => new PostgresDatabaseInitializer(),
            ConfigurationKeyConstants.PROVIDER_POSTGRESQL => new PostgresDatabaseInitializer(),
            _ => new NoOpDatabaseInitializer()
        };
        
        initializer.InitializeDatabase(configuration);
        
        // Register database
        services.AddSingleton(databaseConnector);
        services.AddSingleton(databaseConnector.GetDbContextOptions());
        services.AddScoped(x => new DatabaseContext(x.GetRequiredService<DbContextOptions<DatabaseContext>>()));
    }
}