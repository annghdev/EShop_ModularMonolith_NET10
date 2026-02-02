using Kernel.Application;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace Infrastructure;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, HashSet<string>> _prefixKeys = [];
    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_memoryCache.TryGetValue(key, out var cachedValue))
        {
            return Task.FromResult((T?)cachedValue);
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(5)
        };

        _memoryCache.Set(key, value, options);

        // Tự động phát hiện prefix
        var prefix = ExtractPrefix(key);
        if (prefix != null)
        {
            var keys = _prefixKeys.GetOrAdd(prefix, _ => new HashSet<string>());
            lock (keys)
            {
                keys.Add(key);
            }
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);

        var prefix = ExtractPrefix(key);
        if (prefix != null && _prefixKeys.TryGetValue(prefix, out var keys))
        {
            lock (keys)
            {
                keys.Remove(key);
                if (keys.Count == 0)
                {
                    _prefixKeys.TryRemove(prefix, out _);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        if (_prefixKeys.TryGetValue(prefix, out var keys))
        {
            lock (keys)
            {
                foreach (var key in keys)
                {
                    _memoryCache.Remove(key);
                }

                _prefixKeys.TryRemove(prefix, out _);
            }
        }

        return Task.CompletedTask;
    }

    private static string? ExtractPrefix(string key)
    {
        var index = key.IndexOf(':');
        return index > 0 ? key[..index] : null;
    }
}