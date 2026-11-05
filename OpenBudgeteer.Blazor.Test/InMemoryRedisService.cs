using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBudgeteer.Blazor.Common.Services;
using StackExchange.Redis;

namespace OpenBudgeteer.Blazor.Test;

/// <summary>
/// Minimal in-memory substitute for <see cref="RedisService"/> (string values only, expirations are ignored)
/// </summary>
public class InMemoryRedisService : IRedisService
{
    public ConcurrentDictionary<string, string> Store { get; } = new();

    public IEnumerable<string> KeysStartingWith(string prefix) => Store.Keys.Where(k => k.StartsWith(prefix));

    public Task<string> GetStringValueAsync(string key, string defaultValue) =>
        Task.FromResult(Store.TryGetValue(key, out var value) ? value : defaultValue);

    public Task<int> GetIntValueAsync(string key, int defaultValue) =>
        Task.FromResult(Store.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : defaultValue);

    public Task<bool> GetBoolValueAsync(string key, bool defaultValue) =>
        Task.FromResult(Store.TryGetValue(key, out var value) && bool.TryParse(value, out var result) ? result : defaultValue);

    public Task<HashEntry[]> GetHashEntriesAsync(string key) => throw new NotSupportedException();

    public Task SetStringValueAsync(string key, string value)
    {
        Store[key] = value;
        return Task.CompletedTask;
    }

    public Task SetIntValueAsync(string key, int value) => SetStringValueAsync(key, value.ToString());

    public Task SetBoolValueAsync(string key, bool value) => SetStringValueAsync(key, value.ToString());

    public Task SetHashValueAsync(string key, Dictionary<string, string> values) => throw new NotSupportedException();

    public Task DeleteKeyAsync(string key)
    {
        Store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ContainsKeyAsync(string key) => Task.FromResult(Store.ContainsKey(key));

    public Task<bool> SetStringValueWithExpirationAsync(string key, string value, TimeSpan expiration)
    {
        Store[key] = value;
        return Task.FromResult(true);
    }

    public Task<bool> KeyExpireAsync(string key, TimeSpan expiration) => Task.FromResult(Store.ContainsKey(key));

    public Task DeleteKeysByPatternAsync(string pattern)
    {
        if (!pattern.EndsWith('*') || pattern[..^1].Contains('*'))
            throw new NotSupportedException("Only trailing wildcard patterns are supported");
        foreach (var key in KeysStartingWith(pattern[..^1]).ToList())
            Store.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
