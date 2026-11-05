using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Blazor.Common.Endpoints;
using OpenBudgeteer.Blazor.Common.Extensions;
using OpenBudgeteer.Blazor.Common.Services;
using OpenBudgeteer.Core.Data;
using Xunit;

namespace OpenBudgeteer.Blazor.Test;

public class AuthenticationTests : IAsyncLifetime
{
    private const string USERNAME = "localadmin";
    private const string PASSWORD = "test-password";
    private const string SESSION_KEY_PREFIX = "auth:session:";

    private readonly InMemoryRedisService _redis = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration[ConfigurationKeyConstants.APPSETTINGS_AUTH_ENABLED] = "true";
        builder.Configuration[ConfigurationKeyConstants.APPSETTINGS_AUTH_USERNAME] = USERNAME;
        builder.Configuration[ConfigurationKeyConstants.APPSETTINGS_AUTH_PASSWORD] = PASSWORD;

        builder.Services.AddSingleton<IRedisService>(_redis);
        builder.Services.AddAuthenticationServices(builder.Configuration);

        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapAuthenticationEndpoints();
        _app.MapGet("/protected", () => "secret").RequireAuthorization();

        await _app.StartAsync();
        _client = _app.GetTestServer().CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    private async Task<HttpResponseMessage> LoginAsync(string password = PASSWORD, string returnUrl = "/")
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = USERNAME,
            ["password"] = password,
            ["returnUrl"] = returnUrl
        });
        return await _client.PostAsync("/auth/login", content);
    }

    private static string GetAuthCookie(HttpResponseMessage response)
    {
        var setCookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("OpenBudgeteer.Auth="));
        return setCookie.Split(';')[0];
    }

    private async Task<HttpResponseMessage> SendWithCookieAsync(HttpMethod method, string url, string cookie)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Cookie", cookie);
        return await _client.SendAsync(request);
    }

    private static void AssertRedirectsToLogin(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/login", response.Headers.Location!.PathAndQuery);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutCookie_RedirectsToLogin()
    {
        AssertRedirectsToLogin(await _client.GetAsync("/protected"));
    }

    [Fact]
    public async Task SavedCookie_ReplayedAfterLogout_IsRejected()
    {
        var login = await LoginAsync();
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/", login.Headers.Location!.OriginalString);
        var savedCookie = GetAuthCookie(login);
        Assert.Single(_redis.KeysStartingWith(SESSION_KEY_PREFIX));

        var beforeLogout = await SendWithCookieAsync(HttpMethod.Get, "/protected", savedCookie);
        Assert.Equal(HttpStatusCode.OK, beforeLogout.StatusCode);

        var logout = await SendWithCookieAsync(HttpMethod.Post, "/auth/logout", savedCookie);
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Empty(_redis.KeysStartingWith(SESSION_KEY_PREFIX));

        AssertRedirectsToLogin(await SendWithCookieAsync(HttpMethod.Get, "/protected", savedCookie));
    }

    [Fact]
    public async Task Cookie_WithRemovedServerSession_IsRejected()
    {
        var cookie = GetAuthCookie(await LoginAsync());
        Assert.Equal(HttpStatusCode.OK, (await SendWithCookieAsync(HttpMethod.Get, "/protected", cookie)).StatusCode);

        foreach (var key in _redis.KeysStartingWith(SESSION_KEY_PREFIX).ToList()) await _redis.DeleteKeyAsync(key);

        AssertRedirectsToLogin(await SendWithCookieAsync(HttpMethod.Get, "/protected", cookie));
    }

    [Fact]
    public async Task Cookie_ForChangedUsername_IsRejected()
    {
        var cookie = GetAuthCookie(await LoginAsync());
        _app.Configuration[ConfigurationKeyConstants.APPSETTINGS_AUTH_USERNAME] = "someone-else";

        AssertRedirectsToLogin(await SendWithCookieAsync(HttpMethod.Get, "/protected", cookie));
    }

    [Fact]
    public async Task ChangedPassword_RejectsOldPassword_AndRevokesSessions()
    {
        var cookie = GetAuthCookie(await LoginAsync());
        Assert.Equal(HttpStatusCode.OK, (await SendWithCookieAsync(HttpMethod.Get, "/protected", cookie)).StatusCode);

        const string newPassword = "rotated-password";
        _app.Configuration[ConfigurationKeyConstants.APPSETTINGS_AUTH_PASSWORD] = newPassword;
        await _app.Services.GetRequiredService<AppAuthenticationService>().InitializeAsync(); // as on startup

        AssertRedirectsToLogin(await SendWithCookieAsync(HttpMethod.Get, "/protected", cookie));

        var oldLogin = await LoginAsync(PASSWORD);
        Assert.StartsWith("/login?error=invalid", oldLogin.Headers.Location!.OriginalString);

        var newLogin = await LoginAsync(newPassword);
        Assert.Equal("/", newLogin.Headers.Location!.OriginalString);
        var newCookie = GetAuthCookie(newLogin);
        Assert.Equal(HttpStatusCode.OK, (await SendWithCookieAsync(HttpMethod.Get, "/protected", newCookie)).StatusCode);
    }

    [Theory]
    [InlineData("//evil.example/x", "/")]
    [InlineData("/\\evil.example", "/")]
    [InlineData("https://evil.example", "/")]
    [InlineData("/\t/evil.example", "/")]
    [InlineData("javascript:alert(1)", "/")]
    [InlineData("", "/")]
    [InlineData("/", "/")]
    [InlineData("/bucket", "/bucket")]
    [InlineData("~/account", "/account")]
    public async Task Login_ReturnUrl_OnlyAllowsLocalRedirects(string returnUrl, string expectedLocation)
    {
        var response = await LoginAsync(returnUrl: returnUrl);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expectedLocation, response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task FailedLogin_ReturnUrl_IsSanitized()
    {
        var response = await LoginAsync("wrong", "//evil.example");
        Assert.Equal("/login?error=invalid&returnUrl=%2F", response.Headers.Location!.OriginalString);
    }
}
