using System;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Data;

// Provides a mock DB context for EF Core for the scaffolding of the databases
public class DesignTimeDbContext : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var configuration = new ConfigurationBuilder()
            //.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var options = DbContextOptionsFactory.GetContextOptions(configuration);
        return new DatabaseContext(options);
    }
}