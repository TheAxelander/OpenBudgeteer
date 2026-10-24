using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Core.Data.Entities;
using OpenBudgeteer.Core.Data.Initialization;

namespace OpenBudgeteer.Blazor.Common.Services;

public class DatabaseMigratorService : IHostedService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly IConfiguration _configuration;

    public DatabaseMigratorService(IDbContextFactory<DatabaseContext> dbContextFactory, IConfiguration configuration)
    {
        _dbContextFactory = dbContextFactory;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken: cancellationToken);

        var initializeWithDemoData = _configuration.GetValue<bool>(ConfigurationKeyConstants.APPSETTINGS_DEMO_DATA);
        if (initializeWithDemoData) new DemoDataGenerator(_dbContextFactory).GenerateDemoData();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
