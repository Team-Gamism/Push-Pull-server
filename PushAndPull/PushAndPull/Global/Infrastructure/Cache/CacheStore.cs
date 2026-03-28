using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace PushAndPull.Global.Infrastructure.Cache;

public class CacheStore : ICacheStore
{
    private readonly IDistributedCache _cache;

    public CacheStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);

        var options = new DistributedCacheEntryOptions();
        if (ttl.HasValue)
            options.AbsoluteExpirationRelativeToNow = ttl;

        await _cache.SetAsync(key, bytes, options);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var bytes = await _cache.GetAsync(key);

        if (bytes is null)
            return default;

        return JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task DeleteAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _cache.GetAsync(key) is not null;
    }
}
