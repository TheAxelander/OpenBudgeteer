using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenBudgeteer.Data;

public static class DatabaseInitializationExtensions
{
    private const string CONNECTION_PROVIDER = "CONNECTION_PROVIDER";
    
    private const string PROVIDER_MYSQL = "MYSQL";
    private const string PROVIDER_MARIADB = "MARIADB";
    private const string PROVIDER_POSTGRES = "POSTGRES";
    private const string PROVIDER_POSTGRESQL = "POSTGRESQL";
    
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>(CONNECTION_PROVIDER).Trim().ToUpper();
        IDatabaseInitializer initializer = provider switch
        {
            PROVIDER_MARIADB => new MariaDbDatabaseInitializer(),
            PROVIDER_MYSQL => new MariaDbDatabaseInitializer(),
            PROVIDER_POSTGRES => new PostgresDatabaseInitializer(),
            PROVIDER_POSTGRESQL => new PostgresDatabaseInitializer(),
            _ => new NoOpDatabaseInitializer()
        };
        
        initializer.InitializeDatabase(configuration);

        var dbContextOptions = DbContextOptionsFactory.GetContextOptions(configuration);
        var context = new DatabaseContext(dbContextOptions);

        services.AddSingleton(initializer);
        services.AddSingleton(dbContextOptions);
        services.AddSingleton(context);
    }
}