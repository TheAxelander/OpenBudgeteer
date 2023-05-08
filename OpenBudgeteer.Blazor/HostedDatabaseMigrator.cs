using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Data;

namespace OpenBudgeteer.Blazor;

public class HostedDatabaseMigrator : IHostedService
{
    private readonly DatabaseContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IDatabaseInitializer _databaseInitializer;

    public HostedDatabaseMigrator(
        DatabaseContext dbContext,
        IConfiguration configuration,
        IDatabaseInitializer databaseInitializer)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _databaseInitializer = databaseInitializer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _databaseInitializer.InitializeDatabase(_configuration);
        await _dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}