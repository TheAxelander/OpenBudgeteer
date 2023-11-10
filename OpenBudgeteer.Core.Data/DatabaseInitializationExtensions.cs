using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Initialization;
using OpenBudgeteer.Core.Data.OnlineChecker;

namespace OpenBudgeteer.Core.Data;

// Inject the required DBContext and the DBContext initializer services into the standardized DI container.
public static class DatabaseInitializationExtensions
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_PROVIDER).Trim().ToUpper();
        var rootPasswordEmpty = string.IsNullOrWhiteSpace(configuration.GetValue<string>(ConfigurationKeyConstants.CONNECTION_ROOT_PASSWORD, null));
        IDatabaseInitializer initializer = provider switch
        {
            _ when rootPasswordEmpty => new NoOpDatabaseInitializer(), // Short circuit when user did not provide root password.
            ConfigurationKeyConstants.PROVIDER_MARIADB => new MariaDbDatabaseInitializer(),
            ConfigurationKeyConstants.PROVIDER_MYSQL => new MariaDbDatabaseInitializer(),
            _ => new NoOpDatabaseInitializer()
        };
        
        IDatabaseOnlineChecker onlineChecker = provider switch
        {
            ConfigurationKeyConstants.PROVIDER_MARIADB => new PingPortOnlineChecker(),
            ConfigurationKeyConstants.PROVIDER_MYSQL => new PingPortOnlineChecker(),
            ConfigurationKeyConstants.PROVIDER_POSTGRES => new PostgresOnlineChecker(),
            ConfigurationKeyConstants.PROVIDER_POSTGRESQL => new PostgresOnlineChecker(),
            _ => new NoopOnlineChecker()
        };

        var isOnline = onlineChecker.IsDbOnline(configuration);
        if (!isOnline)
        {
            throw new InvalidOperationException("Target database is not online.");
        }

        initializer.InitializeDatabase(configuration);

        var dbContextOptions = DbContextOptionsFactory.GetContextOptions(configuration);
        
        services.AddSingleton(initializer);
        services.AddSingleton(onlineChecker);
        services.AddSingleton(dbContextOptions);
        services.AddScoped(x => new DatabaseContext(x.GetRequiredService<DbContextOptions<DatabaseContext>>()));
    }
}