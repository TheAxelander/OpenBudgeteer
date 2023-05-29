using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenBudgeteer.Data;

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
            ConfigurationKeyConstants.PROVIDER_POSTGRES => new PostgresDatabaseInitializer(),
            ConfigurationKeyConstants.PROVIDER_POSTGRESQL => new PostgresDatabaseInitializer(),
            _ => new NoOpDatabaseInitializer()
        };
        
        initializer.InitializeDatabase(configuration);

        var dbContextOptions = DbContextOptionsFactory.GetContextOptions(configuration);
        
        services.AddSingleton(initializer);
        services.AddSingleton(dbContextOptions);
        services.AddScoped(x => new DatabaseContext(x.GetRequiredService<DbContextOptions<DatabaseContext>>()));
    }
}