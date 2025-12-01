using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Core.Data;
using System;

namespace OpenBudgeteer.Blazor.Common.Extensions;

public static class AuthenticationInitializationExtension
{
    public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AppAuthenticationService>();
        
        var authEnabled = configuration.GetValue(ConfigurationKeyConstants.APPSETTINGS_AUTH_ENABLED, false);
        if (authEnabled)
        {
            // Full authentication setup
            var sessionDays = configuration.GetValue(ConfigurationKeyConstants.APPSETTINGS_AUTH_SESSION_DAYS, 7);

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "OpenBudgeteer.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.ExpireTimeSpan = TimeSpan.FromDays(sessionDays);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.AccessDeniedPath = "/login";
                });
            services.AddAuthorization();
        }
        else
        {
            // Minimal setup - just enough to prevent [Authorize] from breaking
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(null)
                .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true)
                    .Build());
        }

        services.AddCascadingAuthenticationState(); // required for both cases
    }
}
