using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Services.HelperServices;

public interface ICacheService
{
    ValueTask<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken token, TimeSpan? expirationTimeSpan = null);
    ValueTask SetAsync<T>(string key, T value, CancellationToken token, TimeSpan? expirationTimeSpan = null);
    Task<T?> GetAsync<T>(string key, CancellationToken token);
    ValueTask RemoveAsync(string key, CancellationToken token);
    ValueTask RemoveByTagAsync(string tag, CancellationToken token);
}

internal class InternalCacheService(IFusionCache fusionCache, ILogger<ICacheService> logger) : ICacheService
{
    public ValueTask<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken token, TimeSpan? expirationTimeSpan = null)
    {
        var duration = expirationTimeSpan ?? fusionCache.DefaultEntryOptions.Duration;

        return fusionCache.GetOrSetAsync(key, factory, SetupOptions(expirationTimeSpan), token);
    }

    public ValueTask SetAsync<T>(string key, T value, CancellationToken token, TimeSpan? expirationTimeSpan = null)
    {
        var duration = expirationTimeSpan ?? fusionCache.DefaultEntryOptions.Duration;

        return fusionCache.SetAsync(key, value, SetupOptions(expirationTimeSpan), token);
    }

    private FusionCacheEntryOptions? SetupOptions(TimeSpan? expirationTimeSpan)
    {
        if (expirationTimeSpan is null)
        {
            return default;
        }

        var updatedOptions = fusionCache.DefaultEntryOptions.Duplicate(expirationTimeSpan);

        if (updatedOptions.IsFailSafeEnabled)
        {
            if (updatedOptions.FailSafeMaxDuration < expirationTimeSpan)
            {
                updatedOptions.FailSafeMaxDuration = expirationTimeSpan.Value;
            }

            if (updatedOptions.DistributedCacheFailSafeMaxDuration < expirationTimeSpan)
            {
                updatedOptions.DistributedCacheFailSafeMaxDuration = expirationTimeSpan.Value;
            }
        }

        return updatedOptions;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken token)
    {
        var maybeValue = await fusionCache.TryGetAsync<T>(key, token: token);

        if (maybeValue.HasValue == false)
        {
            logger.LogInformation($"Cache item not found for key {key}");
            return default;
        }

        return maybeValue.Value;
    }

    public ValueTask RemoveAsync(string key, CancellationToken token)
    {
        return fusionCache.RemoveAsync(key, token: token);
    }

    public ValueTask RemoveByTagAsync(string tag, CancellationToken token)
    {
        return fusionCache.RemoveByTagAsync(tag, token: token);
    }
}
