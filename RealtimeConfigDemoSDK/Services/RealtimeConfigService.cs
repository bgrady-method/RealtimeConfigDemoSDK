using RealtimeConfigDemoSDK.Models;
using RealtimeConfigDemoSDK.Services;
using RealtimeConfigDemoSDK.Services.Providers;
using RealtimeConfigDemoSDK.Services.Providers.Contracts;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace RealtimeConfigDemoSDK.Services
{
    public class RealtimeConfigRefreshDate
    {
        public DateTime LastRefresh { get; set; }
    }

    public class RealtimeConfigService : IRealtimeConfigService
    {
        ICacheProvider _cacheProvider;
        private RestClient _restClient;
        private readonly ILogger<RealtimeConfigService> _logger;
        private readonly string cacheKeyTemplate = "realtimeconfig_{0}_{1}";
        private readonly string cacheRefreshTimeTemplate = "realtimeconfig_refreshtime_{0}";

        public RealtimeConfigService(ICacheProvider cacheProvider, string gatewayUrl, ILogger<RealtimeConfigService> logger)
        {
            _cacheProvider = cacheProvider;
            _restClient = new RestClient(gatewayUrl);
            _logger = logger;
        }

        /// <summary>
        /// We lazy-load the config value from the gateway. If it is not found, we store the defaultValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="accountId"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private async Task<T> GetGenericConfigAsync<T>(int accountId, string key, T defaultValue)
        {
            T returnValue = defaultValue;
            try
            {
                // Check if the key is in the cache
                RealtimeConfig value = await _cacheProvider.GetAsync<RealtimeConfig>(string.Format(cacheKeyTemplate, accountId, key));

                // If we have a cached value, return it, and refresh the cache in the background
                if (value != null)
                {
                    returnValue = TryParseConfig<T>(value);

                    RealtimeConfigRefreshDate lastRefresh = await _cacheProvider.GetAsync<RealtimeConfigRefreshDate>(string.Format(cacheRefreshTimeTemplate, accountId));
                    if (lastRefresh == null || lastRefresh.LastRefresh.AddSeconds(30) < DateTime.UtcNow)
                    {
                        // Refresh the cache in the background
                        _ = Task.Run(() => RefreshConfigValues(accountId, key, defaultValue));
                    }
                }
                else
                {
                    returnValue = await RefreshConfigValues(accountId, key, defaultValue);
                }
            }
            catch (Exception ex)
            {
                // Handle exception or log error as needed
                _logger.LogError(ex, "Error occurred while retrieving realtime config value for key {Key}", key);
            }

            return returnValue;
        }

        private async Task<T> RefreshConfigValues<T>(int accountId, string key, T defaultValue)
        {
            T returnValue = defaultValue;
            RealtimeConfig value = ConvertPrimitiveToRealtimeConfig(accountId, key, defaultValue);

            var request = new RestRequest($"/support/configs/{accountId}", Method.Get);
            var response = await _restClient.ExecuteAsync<IEnumerable<RealtimeConfig>>(request);

            if (response.IsSuccessful && response.Data != null)
            {
                var configList = response.Data.ToList();
                bool found = false;
                foreach (var config in configList)
                {
                    await _cacheProvider.SetAsync(string.Format(cacheKeyTemplate, accountId, config.Key), config);
                    if (!string.IsNullOrEmpty(key) && config.Key == key)
                    {
                        value = config;
                        returnValue = TryParseConfig<T>(config);
                        found = true;
                    }
                }
                await _cacheProvider.SetAsync(string.Format(cacheKeyTemplate, accountId, key), value);
                if (!found)
                {
                    _logger.LogWarning("Config value for key {Key} not found for account {AccountId}", key, accountId);
                    await SetDefaultConfigValueAsync(value);
                }
                await _cacheProvider.SetAsync(string.Format(cacheRefreshTimeTemplate, accountId), new RealtimeConfigRefreshDate() { LastRefresh = DateTime.UtcNow });
            }
            else
            {
                _logger.LogError("Error occurred while retrieving realtime config values for account {AccountId}", accountId);
            }

            return returnValue;
        }

        private async Task SetDefaultConfigValueAsync(RealtimeConfig value)
        {
            var request = new RestRequest($"/support/configs/default", Method.Put);
            request.AddJsonBody(value);
            var response = await _restClient.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                _logger.LogError("Error occurred while setting default config value for key {Key}", value.Key);
            }
        }

        public RealtimeConfig ConvertPrimitiveToRealtimeConfig<T>(int accountId, string key, T value)
        {
            return new RealtimeConfig()
            {
                AccountId = accountId,
                Key = key,
                Value = value.ToString(),
                Type = GetConfigType(value)
            };
        }

        private ConfigType GetConfigType<T>(T value)
        {
            if (value is string)
            {
                return ConfigType.String;
            }
            else if (value is int)
            {
                return ConfigType.Int;
            }
            else if (value is bool)
            {
                return ConfigType.Bool;
            }
            else if (value is double)
            {
                return ConfigType.Double;
            }
            else
            {
                throw new Exception($"Invalid config type: {value.GetType()}");
            }
        }

        private T TryParseConfig<T>(RealtimeConfig config)
        {
            switch (config.Type)
            {
                case ConfigType.String:
                    return (T)Convert.ChangeType(config.Value, typeof(T));
                case ConfigType.Int:
                    return (T)Convert.ChangeType(int.Parse(config.Value), typeof(T));
                case ConfigType.Bool:
                    return (T)Convert.ChangeType(bool.Parse(config.Value), typeof(T));
                case ConfigType.Double:
                    return (T)Convert.ChangeType(double.Parse(config.Value), typeof(T));
                default:
                    throw new Exception($"Invalid config type: {config.Type}");
            }
        }

        public Task<string> GetConfigAsync(int accountId, string key, string defaultValue)
        {
            return GetGenericConfigAsync(accountId, key, defaultValue);
        }

        public Task<int> GetConfigAsync(int accountId, string key, int defaultValue)
        {
            return GetGenericConfigAsync(accountId, key, defaultValue);
        }

        public Task<double> GetConfigAsync(int accountId, string key, double defaultValue)
        {
            return GetGenericConfigAsync(accountId, key, defaultValue);
        }

        public Task<bool> GetConfigAsync(int accountId, string key, bool defaultValue)
        {
            return GetGenericConfigAsync(accountId, key, defaultValue);
        }
    }
}