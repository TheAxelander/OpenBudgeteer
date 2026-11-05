using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace OpenBudgeteer.Blazor.Common.Services;

public interface IRedisService
{
    public Task<string?> GetStringValueAsync(string key);
    public Task<HashEntry[]> GetHashEntriesAsync(string key);
    public Task SetStringValueAsync(string key, string value);
    public Task SetHashValueAsync(string key, Dictionary<string, string> values);
    public Task DeleteKeyAsync(string key);
    public Task<bool> KeyExistsAsync(string key);
    public Task<bool> SetStringValueWithExpirationAsync(string key, string value, TimeSpan expiration);
    public Task<bool> KeyExpireAsync(string key, TimeSpan expiration);

    /// <summary>
    /// Deletes all keys matching the passed glob-style pattern (prefix is applied automatically)
    /// </summary>
    public Task DeleteKeysByPatternAsync(string pattern);
}
