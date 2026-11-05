using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace OpenBudgeteer.Blazor.Common.Services;

public class AppInitializerHostedService : IHostedService
{
    private readonly AppSettingService _appSettingService;
    private readonly MudThemeService _mudThemeService;
    private readonly AppAuthenticationService _authService;

    public AppInitializerHostedService(
        AppSettingService appSettingService,
        MudThemeService mudThemeService,
        AppAuthenticationService authService)
    {
        _appSettingService = appSettingService;
        _mudThemeService = mudThemeService;
        _authService = authService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _appSettingService.InitializeAsync();
        await _mudThemeService.InitializeAsync();
        await _authService.InitializeAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}