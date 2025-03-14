using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Initialization;

namespace OpenBudgeteer.Blazor.Common;

public class HostedDatabaseMigrator : IHostedService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;
    private readonly IConfiguration _configuration;
    
    private const string APPSETTINGS_DEMO_DATA = "APPSETTINGS_DEMO_DATA";

    public HostedDatabaseMigrator(DbContextOptions<DatabaseContext> dbContextOptions, IConfiguration configuration)
    {
        _dbContextOptions = dbContextOptions;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var context = new DatabaseContext(_dbContextOptions);
        await context.Database.MigrateAsync(cancellationToken: cancellationToken);

        var initializeWithDemoData = _configuration.GetValue<bool>(APPSETTINGS_DEMO_DATA);
        if (initializeWithDemoData) new DemoDataGenerator(_dbContextOptions).GenerateDemoData();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}