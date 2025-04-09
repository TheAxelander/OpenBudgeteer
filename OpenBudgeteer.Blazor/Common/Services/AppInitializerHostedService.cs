using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace OpenBudgeteer.Blazor.Common.Services;

public class AppInitializerHostedService : IHostedService
{
    private readonly AppSettingService _appSettingService;
    private readonly MudThemeService _mudThemeService;

    public AppInitializerHostedService(AppSettingService appSettingService, MudThemeService mudThemeService)
    {
        _appSettingService = appSettingService;
        _mudThemeService = mudThemeService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _appSettingService.InitializeAsync();
        await _mudThemeService.InitializeAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}