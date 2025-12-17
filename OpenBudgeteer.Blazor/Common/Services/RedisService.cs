using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace OpenBudgeteer.Blazor.Common.Services;

public class RedisService
{
    private readonly IConnectionMultiplexer _connection;
    private readonly string _prefix;
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer connection, string prefix)
    {
        _connection = connection;
        _prefix = prefix;
        _database = _connection.GetDatabase();
    }
    public async Task<string?> GetStringValueAsync(string key)
    {
        return await _database.StringGetAsync(string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}");
    }

    public async Task<HashEntry[]> GetHashEntriesAsync(string key)
    {
        return await _database.HashGetAllAsync(string.IsNullOrEmpty(_prefix) ? key :$"{_prefix}:{key}");
    }

    public async Task SetStringValueAsync(string key, string value)
    {
        await _database.StringSetAsync(string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}", value);
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

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}");
    }

    public async Task<bool> SetStringValueWithExpirationAsync(string key, string value, TimeSpan expiration)
    {
        return await _database.StringSetAsync(
            string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}",
            value,
            expiration);
    }
}