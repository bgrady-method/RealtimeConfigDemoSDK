namespace RealtimeConfigDemoSDK.Services.Providers.Contracts
{
    public interface ICacheProvider
    {
        Task ClearKeyAsync(string cacheKey);
        Task<string> GetStringAsync(string cacheKey);
        Task SetStringAsync(string cacheKey, string value, TimeSpan? expiresIn = null);
        Task SetStringBySlidingExpiryAsync(string cacheKey, string value, TimeSpan? slidingExpiration = null);
        Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiresIn = null) where T : class;
        Task<T> GetAsync<T>(string cacheKey) where T : class;
    }
}
