using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenBudgeteer.Data;

// Inject the required DBContext and the DBContext initializer services into the standardized DI container.
public static class DatabaseInitializationExtensions
{
    private const string CONNECTION_PROVIDER = "CONNECTION_PROVIDER";
    
    private const string PROVIDER_MYSQL = "MYSQL";
    private const string PROVIDER_MARIADB = "MARIADB";
    private const string PROVIDER_POSTGRES = "POSTGRES";
    private const string PROVIDER_POSTGRESQL = "POSTGRESQL";
    private const string CONNECTION_ROOT_PASSWORD = "CONNECTION_ROOT_PASSWORD";
    
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>(CONNECTION_PROVIDER).Trim().ToUpper();
        var rootPasswordProvided = string.IsNullOrWhiteSpace(configuration.GetValue<string>(CONNECTION_ROOT_PASSWORD, null));
        IDatabaseInitializer initializer = provider switch
        {
            _ when !rootPasswordProvided => new NoOpDatabaseInitializer(), // Short circuit when user did not provide root password.
            PROVIDER_MARIADB => new MariaDbDatabaseInitializer(),
            PROVIDER_MYSQL => new MariaDbDatabaseInitializer(),
            PROVIDER_POSTGRES => new PostgresDatabaseInitializer(),
            PROVIDER_POSTGRESQL => new PostgresDatabaseInitializer(),
            _ => new NoOpDatabaseInitializer()
        };
        
        initializer.InitializeDatabase(configuration);

        var dbContextOptions = DbContextOptionsFactory.GetContextOptions(configuration);
        
        services.AddSingleton(initializer);
        services.AddSingleton(dbContextOptions);
        services.AddScoped(x => new DatabaseContext(x.GetRequiredService<DbContextOptions<DatabaseContext>>()));
    }
}