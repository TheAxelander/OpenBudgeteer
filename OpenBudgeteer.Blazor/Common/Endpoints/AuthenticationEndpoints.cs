using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using OpenBudgeteer.Core.Data;
using OpenBudgeteer.Blazor.Common.Services;

namespace OpenBudgeteer.Blazor.Common.Endpoints;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/login", HandleLogin)
            .AllowAnonymous()
            .DisableAntiforgery();

        endpoints.MapPost("/auth/logout", HandleLogout);
    }

    private static async Task<IResult> HandleLogin(
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] string? returnUrl,
        HttpContext httpContext,
        AppAuthenticationService authService,
        IConfiguration configuration)
    {
        // Only allow redirects within the application to prevent open redirects
        returnUrl = GetSafeReturnUrl(returnUrl);
        try
        {
            // Validate credentials
            var isValid = await authService.ValidateCredentialsAsync(username, password);
            if (!isValid) return Results.Redirect($"/login?error=invalid&returnUrl={Uri.EscapeDataString(returnUrl)}");

            // Create Redis session
            var sessionId = await authService.CreateSessionAsync(username);

            // Create authentication cookie
            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, username),
                new (AppAuthenticationService.SESSION_ID_CLAIM, sessionId)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var sessionDays = configuration.GetValue(ConfigurationKeyConstants.APPSETTINGS_AUTH_SESSION_DAYS, 7);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(sessionDays)
            };

            // Establish authenticated session
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect to return URL or home
            return Results.Redirect(returnUrl);
        }
        catch (Exception)
        {
            return Results.Redirect($"/login?error=system&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
    }

    private static async Task<IResult> HandleLogout(HttpContext httpContext, AppAuthenticationService authService)
    {
        // Remove session from Redis
        var sessionId = httpContext.User.FindFirst(AppAuthenticationService.SESSION_ID_CLAIM)?.Value;
        if (!string.IsNullOrEmpty(sessionId)) await authService.RemoveSessionAsync(sessionId);

        // End authenticated session
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Redirect to Login page
        return Results.Redirect("/login");
    }

    /// <summary>
    /// Returns the passed URL if it is a local application path, otherwise the application root
    /// </summary>
    internal static string GetSafeReturnUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return "/";

        // Same rules as ASP.NET Core's IsLocalUrl: "/path" but not "//host" or "/\host", or "~/path"
        var isLocal = url[0] switch
        {
            '/' => url.Length == 1 || (url[1] != '/' && url[1] != '\\'),
            '~' => url.Length > 1 && url[1] == '/' && (url.Length == 2 || (url[2] != '/' && url[2] != '\\')),
            _ => false
        };
        if (!isLocal) return "/";

        // Reject control characters (e.g. tab/newline) which browsers strip, potentially turning "/\t/host" into "//host"
        if (url.Any(char.IsControl)) return "/";

        return url[0] == '~' ? url[1..] : url;
    }
}
