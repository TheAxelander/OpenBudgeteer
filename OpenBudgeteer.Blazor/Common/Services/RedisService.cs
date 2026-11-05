using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace OpenBudgeteer.Blazor.Common.Services;

public class RedisService : IRedisService
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

    private string GetKey(string key) => string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}";

    public async Task<string?> GetStringValueAsync(string key)
    {
        return await _database.StringGetAsync(GetKey(key));
    }

    public async Task<HashEntry[]> GetHashEntriesAsync(string key)
    {
        return await _database.HashGetAllAsync(GetKey(key));
    }

    public async Task SetStringValueAsync(string key, string value)
    {
        await _database.StringSetAsync(GetKey(key), value);
    }
    
    public async Task SetHashValueAsync(string key, Dictionary<string, string> values)
    {
        var hashEntries = values
            .Select(value => new HashEntry(value.Key, value.Value))
            .ToArray();
        await _database.HashSetAsync(GetKey(key), hashEntries);
    }

    public async Task DeleteKeyAsync(string key)
    {
        await _database.KeyDeleteAsync(GetKey(key));
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(GetKey(key));
    }

    public async Task<bool> SetStringValueWithExpirationAsync(string key, string value, TimeSpan expiration)
    {
        return await _database.StringSetAsync(GetKey(key), value, expiration);
    }

    public async Task<bool> KeyExpireAsync(string key, TimeSpan expiration)
    {
        return await _database.KeyExpireAsync(GetKey(key), expiration);
    }

    public async Task DeleteKeysByPatternAsync(string pattern)
    {
        var prefixedPattern = GetKey(pattern);
        foreach (var endpoint in _connection.GetEndPoints())
        {
            var server = _connection.GetServer(endpoint);
            if (!server.IsConnected || server.IsReplica) continue;

            var batch = new List<RedisKey>();
            await foreach (var key in server.KeysAsync(_database.Database, prefixedPattern))
            {
                batch.Add(key);
                if (batch.Count < 100) continue;
                await _database.KeyDeleteAsync(batch.ToArray());
                batch.Clear();
            }
            if (batch.Count > 0) await _database.KeyDeleteAsync(batch.ToArray());
        }
    }
}
