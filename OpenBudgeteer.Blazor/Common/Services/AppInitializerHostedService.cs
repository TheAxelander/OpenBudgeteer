using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenBudgeteer.Core.Common.AppSettings;

namespace OpenBudgeteer.Blazor.Common.Services;

public class AppInitializerHostedService : IHostedService
{
    private readonly IAppSettingService _appSettingService;
    private readonly IMudThemeService _mudThemeService;

    public AppInitializerHostedService(IAppSettingService appSettingService, IMudThemeService mudThemeService)
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
