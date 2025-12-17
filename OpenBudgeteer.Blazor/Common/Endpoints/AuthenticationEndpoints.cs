using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
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
        AppAuthenticationService authService)
    {
        try
        {
            // Validate credentials
            var isValid = await authService.ValidateCredentialsAsync(username, password);
            if (!isValid) return Results.Redirect($"/login?error=invalid&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");

            // Create Redis session
            var sessionId = await authService.CreateSessionAsync(username);

            // Create authentication cookie
            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, username),
                new ("SessionId", sessionId)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Establish authenticated session
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect to return URL or home
            return Results.Redirect(returnUrl ?? "/");
        }
        catch (Exception)
        {
            return Results.Redirect($"/login?error=system&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }
    }

    private static async Task<IResult> HandleLogout(HttpContext httpContext, AppAuthenticationService authService)
    {
        // Remove session from Redis
        var sessionId = httpContext.User.FindFirst("SessionId")?.Value;
        if (!string.IsNullOrEmpty(sessionId)) await authService.RemoveSessionAsync(sessionId);

        // End authenticated session
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Redirect to Login page
        return Results.Redirect("/login");
    }
}
