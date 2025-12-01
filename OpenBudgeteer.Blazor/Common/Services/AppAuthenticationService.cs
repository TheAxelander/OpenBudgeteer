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
    private readonly RedisService _redisService;
    private readonly ILogger<AppAuthenticationService> _logger;

    // Redis Keys
    private const string SESSION_KEY_PREFIX = "auth:session";
    private const string PASSWORD_HASH_KEY = "auth:password_hash";

    public AppAuthenticationService(
        IConfiguration configuration,
        RedisService redisService,
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

    public async Task<string> CreateSessionAsync(string username)
    {
        var sessionId = Guid.NewGuid().ToString();
        var sessionDays = _configuration.GetValue(ConfigurationKeyConstants.APPSETTINGS_AUTH_SESSION_DAYS, 7);
        var sessionData = new
        {
            Username = username,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(sessionDays)
        };

        var sessionJson = JsonSerializer.Serialize(sessionData);
        await _redisService.SetStringValueWithExpirationAsync(
            $"{SESSION_KEY_PREFIX}:{sessionId}",
            sessionJson,
            TimeSpan.FromDays(sessionDays));

        _logger.LogInformation($"Session created for user: {username}");
        return sessionId;
    }

    public async Task<bool> ValidateSessionAsync(string sessionId)
    {
        try
        {
            var sessionKey = $"{SESSION_KEY_PREFIX}:{sessionId}";
            return await _redisService.KeyExistsAsync(sessionKey);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validating session");
            return false;
        }
    }

    public async Task RemoveSessionAsync(string sessionId)
    {
        try
        {
            var sessionKey = $"{SESSION_KEY_PREFIX}:{sessionId}";
            await _redisService.DeleteKeyAsync(sessionKey);
            _logger.LogInformation($"Session removed: {sessionId}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing session");
        }
    }

    private async Task<string> GetPasswordHashAsync()
    {
        // Check if hash exists in Redis
        var existingHash = await _redisService.GetStringValueAsync(PASSWORD_HASH_KEY);
        if (!string.IsNullOrEmpty(existingHash)) return existingHash;

        // Hash user and password from .env and store in Redis
        var username = _configuration.GetValue<string>(ConfigurationKeyConstants.APPSETTINGS_AUTH_USERNAME);
        var plainPassword = _configuration.GetValue<string>(ConfigurationKeyConstants.APPSETTINGS_AUTH_PASSWORD);

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(plainPassword))
            throw new InvalidOperationException("Authentication user and/or password not configured");

        var hasher = new PasswordHasher<string>();
        var hash = hasher.HashPassword(username, plainPassword);
        await _redisService.SetStringValueAsync(PASSWORD_HASH_KEY, hash);

        _logger.LogInformation("Password hash created and cached in Redis");
        return hash;
    }
}
