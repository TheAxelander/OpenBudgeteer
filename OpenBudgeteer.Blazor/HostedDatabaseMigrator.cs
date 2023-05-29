using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Data;
using OpenBudgeteer.Data.Initialization;
using OpenBudgeteer.Data.OnlineChecker;

namespace OpenBudgeteer.Blazor;

public class HostedDatabaseMigrator : IHostedService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;
    private readonly IConfiguration _configuration;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly IDatabaseOnlineChecker _onlineChecker;

    public HostedDatabaseMigrator(
        DbContextOptions<DatabaseContext> dbContextOptions,
        IConfiguration configuration,
        IDatabaseInitializer databaseInitializer,
        IDatabaseOnlineChecker onlineChecker)
    {
        _dbContextOptions = dbContextOptions;
        _configuration = configuration;
        _databaseInitializer = databaseInitializer;
        _onlineChecker = onlineChecker;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _databaseInitializer.InitializeDatabase(_configuration);
        
        // Wait for DB online
        var isOnline = _onlineChecker.IsDbOnline(_configuration);
        if (!isOnline)
        {
            throw new InvalidOperationException("Target database is not online.");
        }
        
        await using var context = new DatabaseContext(_dbContextOptions);

        await context.Database.MigrateAsync(cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}