using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Core.Common.AppSettings;
using StackExchange.Redis;

namespace OpenBudgeteer.Blazor.Common.Services;

public class RedisService : ISettingHandler
{
    private readonly string _prefix;
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer connection, string prefix)
    {
        _prefix = prefix;
        _database = connection.GetDatabase();
    }
    public async Task<string> GetStringValueAsync(string key, string defaultValue)
    {
        var redisValue = await _database.StringGetAsync(string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}");
        return redisValue.HasValue ? redisValue.ToString() : defaultValue;
    }

    public async Task<int> GetIntValueAsync(string key, int defaultValue)
    {
        var stringValue = await GetStringValueAsync(key, string.Empty);
        if (string.IsNullOrEmpty(stringValue)) return defaultValue;

        return (int.TryParse(stringValue, out var convertedValue)) ? convertedValue : defaultValue;
    }

    public async Task<bool> GetBoolValueAsync(string key, bool defaultValue)
    {
        var stringValue = await GetStringValueAsync(key, string.Empty);
        if (string.IsNullOrEmpty(stringValue)) return defaultValue;

        return (bool.TryParse(stringValue, out var convertedValue)) ? convertedValue : defaultValue;
    }

    public async Task<HashEntry[]> GetHashEntriesAsync(string key)
    {
        return await _database.HashGetAllAsync(string.IsNullOrEmpty(_prefix) ? key :$"{_prefix}:{key}");
    }

    public async Task SetStringValueAsync(string key, string value)
    {
        await _database.StringSetAsync(string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}", value);
    }

    public async Task SetStringValueWithExpirationAsync(string key, string value, TimeSpan expiration)
    {
        await _database.StringSetAsync(
            string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}",
            value,
            expiration);
    }

    public async Task SetIntValueAsync(string key, int value)
    {
        await SetStringValueAsync(key, value.ToString());
    }

    public async Task SetBoolValueAsync(string key, bool value)
    {
        await SetStringValueAsync(key, value.ToString());
    }

    public async Task SetHashValueAsync(string key, Dictionary<string, string> values)
    {
        var hashEntries = values
            .Select(value => new HashEntry(value.Key, value.Value))
            .ToArray();
        await _database.HashSetAsync(string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}", hashEntries);
    }

    public async Task DeleteKeyAsync(string key)
    {
        await _database.KeyDeleteAsync(string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}");
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        return await _database.KeyExistsAsync(string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}");
    }
}
