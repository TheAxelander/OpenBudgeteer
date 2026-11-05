using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OpenBudgeteer.Blazor.Common.Services;

/// <summary>
/// Validates on each request that the server-side session referenced by the authentication cookie still
/// exists, so that a cookie can no longer be used once its session has been revoked (e.g. by logout)
/// </summary>
public class SessionValidatingCookieEvents : CookieAuthenticationEvents
{
    private readonly AppAuthenticationService _authService;

    public SessionValidatingCookieEvents(AppAuthenticationService authService)
    {
        _authService = authService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var sessionId = context.Principal?.FindFirst(AppAuthenticationService.SESSION_ID_CLAIM)?.Value;
        if (string.IsNullOrEmpty(sessionId) || !await _authService.ValidateSessionAsync(sessionId))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        // Keep server-side session lifetime in sync with the sliding cookie expiration
        if (context.ShouldRenew) await _authService.RefreshSessionAsync(sessionId);
    }
}
