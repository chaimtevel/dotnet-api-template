namespace Services.HelperServices;

public interface ICachingService
{
    Task<T> GetCacheAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken token, TimeSpan? expirationTimeSpan = null);
    void ClearCache(string key);
}
