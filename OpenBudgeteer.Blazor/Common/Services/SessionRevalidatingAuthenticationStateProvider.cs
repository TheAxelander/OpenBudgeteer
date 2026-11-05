using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Logging;

namespace OpenBudgeteer.Blazor.Common.Services;

/// <summary>
/// Periodically re-checks the server-side session of an active Blazor circuit, so that already
/// connected circuits lose access once their session has been revoked
/// </summary>
public class SessionRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly AppAuthenticationService _authService;

    public SessionRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        AppAuthenticationService authService) : base(loggerFactory)
    {
        _authService = authService;
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(1);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var sessionId = authenticationState.User.FindFirst(AppAuthenticationService.SESSION_ID_CLAIM)?.Value;
        return !string.IsNullOrEmpty(sessionId) && await _authService.ValidateSessionAsync(sessionId);
    }
}
