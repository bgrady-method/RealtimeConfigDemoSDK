using RealtimeConfigDemoSDK.Services.Providers.Contracts;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace RealtimeConfigDemoSDK.Services.Providers
{
    public class CacheProvider : ICacheProvider
    {
        public const string CacheKeyPrefix = "realtimeconfig";

        private struct CacheKeySections
        {
            public const string CredentialsFailed = "credentials.failed";
            public const string AttemptCounter = "attempt.counter";
            public const string OriginalQueueDate = "original.queue.date";
            public const string AttemptLog = "attempt.log";
            public const string LastProcessDate = "process.date";
            public const string LastSendDate = "send.date";
        }

        private readonly IDistributedCache _cache;

        public CacheProvider(IDistributedCache database)
        {
            _cache = database;
        }

        public async Task SetStringAsync(string cacheKey, string value, TimeSpan? expiresIn = null)
        {
            cacheKey = $"{CacheKeyPrefix}-{cacheKey}";
            await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = expiresIn });
        }

        public async Task SetStringBySlidingExpiryAsync(string cacheKey, string value, TimeSpan? slidingExpiration = null)
        {
            cacheKey = $"{CacheKeyPrefix}-{cacheKey}";
            await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions() { SlidingExpiration = slidingExpiration });
        }

        public async Task<string> GetStringAsync(string cacheKey)
        {
            cacheKey = $"{CacheKeyPrefix}-{cacheKey}";
            return await _cache.GetStringAsync(cacheKey);
        }
        
        public async Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiresIn = null) where T : class
        {
            cacheKey = $"{CacheKeyPrefix}-{cacheKey}";
            var options = new DistributedCacheEntryOptions();
            if (expiresIn.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiresIn;
            }
            // Serialize the object to string
            var serializedObject = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            await _cache.SetStringAsync(cacheKey, serializedObject, options);
        }
        
        public async Task<T> GetAsync<T>(string cacheKey) where T : class
        {
            cacheKey = $"{CacheKeyPrefix}-{cacheKey}";
            var serializedObject = await _cache.GetStringAsync(cacheKey);
            return serializedObject == null 
                ? null :
                // Deserialize the string to object
                Newtonsoft.Json.JsonConvert.DeserializeObject<T>(serializedObject);
        }

        public async Task ClearKeyAsync(string cacheKey)
        {
            cacheKey = $"{CacheKeyPrefix}-{cacheKey}";
            await _cache.RemoveAsync(cacheKey);
        }
    }
}
