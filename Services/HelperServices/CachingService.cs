using Microsoft.Extensions.Caching.Memory;

namespace Services.HelperServices;

internal class CachingService : ICachingService
{
    public CachingService(IMemoryCache memoryCache)
    {
        MemoryCache = memoryCache;
    }

    public IMemoryCache MemoryCache { get; }

    public async Task<T> GetCacheAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken token
        , TimeSpan? expirationTimeSpan = null)
    {
        // Look for cache key.
        if (!MemoryCache.TryGetValue(key, out T cacheEntry))
        {
            // Key not in cache, so get data.
            cacheEntry = await factory(token);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expirationTimeSpan.HasValue ? expirationTimeSpan : TimeSpan.FromDays(1)
            };

            // Save data in cache.
            MemoryCache.Set(key, cacheEntry, cacheEntryOptions);
        }

        return cacheEntry;
    }

    public void ClearCache(string key) => MemoryCache.Remove(key);

}