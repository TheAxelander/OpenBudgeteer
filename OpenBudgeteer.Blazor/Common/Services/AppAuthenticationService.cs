using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenBudgeteer.Core.Data;

namespace OpenBudgeteer.Blazor.Common.Services;

public class AppAuthenticationService
{
    private readonly IConfiguration _configuration;
    private readonly IRedisService _redisService;
    private readonly ILogger<AppAuthenticationService> _logger;

    // Redis Keys
    private const string SESSION_KEY_PREFIX = "auth:session";
    private const string PASSWORD_HASH_KEY = "auth:password_hash";

    public AppAuthenticationService(
        IConfiguration configuration,
        IRedisService redisService,
        ILogger<AppAuthenticationService> logger)
    {
        _configuration = configuration;
        _redisService = redisService;
        _logger = logger;
    }

    public bool IsAuthenticationEnabled()
    {
        return _configuration.GetValue(ConfigurationKeyConstants.APPSETTINGS_AUTH_ENABLED, false);
    }

    public async Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        try
        {
            var configUsername = _configuration.GetValue<string>(ConfigurationKeyConstants.APPSETTINGS_AUTH_USERNAME);
            if (string.IsNullOrEmpty(configUsername) || username != configUsername)
            {
                _logger.LogWarning("Authentication failed: Invalid username");
                return false;
            }

            var hasher = new PasswordHasher<string>();
            var passwordHash = await GetPasswordHashAsync();
            var result = hasher.VerifyHashedPassword(username, passwordHash, password);

            switch (result)
            {
                case PasswordVerificationResult.Failed:
                    _logger.LogWarning("Authentication failed: Invalid password");
                    return false;
                case PasswordVerificationResult.Success:
                case PasswordVerificationResult.SuccessRehashNeeded:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validating credentials");
            return false;
        }
    }

    public const string SESSION_ID_CLAIM = "SessionId";

    public async Task<string> CreateSessionAsync(string username)
    {
        var sessionId = Guid.NewGuid().ToString();
        var sessionDays = _configuration.GetValue(ConfigurationKeyConstants.APPSETTINGS_AUTH_SESSION_DAYS, 7);
        var sessionData = new SessionData(username, DateTime.UtcNow, DateTime.UtcNow.AddDays(sessionDays));

        var sessionJson = JsonSerializer.Serialize(sessionData);
        await _redisService.SetStringValueWithExpirationAsync(
            $"{SESSION_KEY_PREFIX}:{sessionId}",
            sessionJson,
            TimeSpan.FromDays(sessionDays));

        _logger.LogInformation("Session created for user: {username}", username);
        return sessionId;
    }

    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        try
        {
            var sessionJson = await _redisService.GetStringValueAsync($"{SESSION_KEY_PREFIX}:{sessionId}", string.Empty);
            if (string.IsNullOrEmpty(sessionJson)) return false;

            // Reject sessions that were created for a username which is no longer configured
            var session = JsonSerializer.Deserialize<SessionData>(sessionJson);
            var configUsername = _configuration.GetValue<string>(ConfigurationKeyConstants.APPSETTINGS_AUTH_USERNAME);
            return session != null && !string.IsNullOrEmpty(configUsername) && session.Username == configUsername;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validating session");
            return false;
        }
    }

    /// <summary>
    /// Extends the lifetime of an existing session, used when the authentication cookie gets renewed
    /// </summary>
    public async Task RefreshSessionAsync(string sessionId)
    {
        try
        {
            var sessionDays = _configuration.GetValue(ConfigurationKeyConstants.APPSETTINGS_AUTH_SESSION_DAYS, 7);
            await _redisService.KeyExpireAsync($"{SESSION_KEY_PREFIX}:{sessionId}", TimeSpan.FromDays(sessionDays));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error refreshing session");
        }
    }

    public async Task RemoveSessionAsync(string sessionId)
    {
        try
        {
            var sessionKey = $"{SESSION_KEY_PREFIX}:{sessionId}";
            await _redisService.DeleteKeyAsync(sessionKey);
            _logger.LogInformation("Session removed: {sessionId}", sessionId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing session");
        }
    }

    /// <summary>
    /// Revokes all existing sessions
    /// </summary>
    public async Task RemoveAllSessionsAsync()
    {
        await _redisService.DeleteKeysByPatternAsync($"{SESSION_KEY_PREFIX}:*");
        _logger.LogInformation("All sessions removed");
    }

    /// <summary>
    /// Ensures that the cached password hash matches the currently configured password. Should be called
    /// on startup so that a changed password takes effect (and revokes existing sessions) immediately.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (!IsAuthenticationEnabled()) return;
        try
        {
            await GetPasswordHashAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error initializing authentication");
        }
    }

    private async Task<string> GetPasswordHashAsync()
    {
        var username = _configuration.GetValue<string>(ConfigurationKeyConstants.APPSETTINGS_AUTH_USERNAME);
        var plainPassword = _configuration.GetValue<string>(ConfigurationKeyConstants.APPSETTINGS_AUTH_PASSWORD);

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(plainPassword))
            throw new InvalidOperationException("Authentication user and/or password not configured");

        var hasher = new PasswordHasher<string>();

        // Check if hash exists in Redis and still matches the configured password
        var existingHash = await _redisService.GetStringValueAsync(PASSWORD_HASH_KEY, string.Empty);
        if (!string.IsNullOrEmpty(existingHash))
        {
            if (hasher.VerifyHashedPassword(username, existingHash, plainPassword) != PasswordVerificationResult.Failed)
                return existingHash;

            // Configured password has been changed, revoke all sessions created with the old password
            _logger.LogWarning("Configured password has changed, existing sessions will be revoked");
            await RemoveAllSessionsAsync();
        }

        // Hash password from configuration and store in Redis
        var hash = hasher.HashPassword(username, plainPassword);
        await _redisService.SetStringValueAsync(PASSWORD_HASH_KEY, hash);

        _logger.LogInformation("Password hash created and cached in Redis");
        return hash;
    }

    private record SessionData(string Username, DateTime CreatedAt, DateTime ExpiresAt);
}
