using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Blazor;

public class HostedDatabaseMigrator : IHostedService
{
    private readonly DbContextOptions<DatabaseContext> _dbContextOptions;
    private readonly IConfiguration _configuration;
    private readonly IDatabaseInitializer _databaseInitializer;

    public HostedDatabaseMigrator(
        DbContextOptions<DatabaseContext> dbContextOptions,
        IConfiguration configuration,
        IDatabaseInitializer databaseInitializer)
    {
        _dbContextOptions = dbContextOptions;
        _configuration = configuration;
        _databaseInitializer = databaseInitializer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _databaseInitializer.InitializeDatabase(_configuration);
        await using var context = new DatabaseContext(_dbContextOptions);
        await context.Database.MigrateAsync(cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}