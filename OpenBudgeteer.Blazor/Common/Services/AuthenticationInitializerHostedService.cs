using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace OpenBudgeteer.Blazor.Common.Services;

/// <summary>
/// Ensures on startup that the cached password hash matches the configured password, so that a changed
/// password takes effect (and revokes existing sessions) immediately. Kept separate from
/// <see cref="AppInitializerHostedService"/> as it is Web App only (Desktop App has no Redis or authentication)
/// </summary>
public class AuthenticationInitializerHostedService : IHostedService
{
    private readonly AppAuthenticationService _authService;

    public AuthenticationInitializerHostedService(AppAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _authService.InitializeAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
