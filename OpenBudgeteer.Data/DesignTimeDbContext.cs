using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OpenBudgeteer.Data;

public class DesignTimeDbContext : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var options = DbContextOptionsFactory.GetContextOptions(configuration);
        return new DatabaseContext(options);
    }
}