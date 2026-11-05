using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common.AppSettings;
using StackExchange.Redis;

namespace OpenBudgeteer.Blazor.Common.Services;

public interface IRedisService : ISettingHandler
{
    public Task<HashEntry[]> GetHashEntriesAsync(string key);
    public Task SetHashValueAsync(string key, Dictionary<string, string> values);
    public Task<bool> SetStringValueWithExpirationAsync(string key, string value, TimeSpan expiration);
    public Task<bool> KeyExpireAsync(string key, TimeSpan expiration);

    /// <summary>
    /// Deletes all keys matching the passed glob-style pattern (prefix is applied automatically)
    /// </summary>
    public Task DeleteKeysByPatternAsync(string pattern);
}
