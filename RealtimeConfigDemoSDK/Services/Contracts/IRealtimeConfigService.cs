namespace RealtimeConfigDemoSDK.Services
{
    public interface IRealtimeConfigService
    {
        Task<string> GetConfigAsync(int accountId, string key, string defaultValue);
        Task<int> GetConfigAsync(int accountId, string key, int defaultValue);
        Task<double> GetConfigAsync(int accountId, string key, double defaultValue);
        Task<bool> GetConfigAsync(int accountId, string key, bool defaultValue);

    }
}