namespace Lho.Lambda.Utils;

public class MemoryCache
{
    private readonly object _lock = new();
    private readonly Dictionary<string, CacheEntry> _cache = [];

    public T? Get<T>(string key)
    {
        lock (_lock)
        {
            if (!_cache.TryGetValue(key, out var entry))
            {
                return default;
            }

            if (entry.ExpirationDate >= DateTimeOffset.UtcNow)
            {
                return entry.Value is T value ? value : default;
            }

            _cache.Remove(key);
            return default;
        }
    }

    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        lock (_lock)
        {
            _cache[key] = new CacheEntry(value!, DateTimeOffset.UtcNow.Add(expiration));
        }
    }

    private record CacheEntry(object Value, DateTimeOffset ExpirationDate);
}
