namespace RealtimeConfigDemoSDK.Models
{
    public class RealtimeConfig
    {
        public int AccountId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public ConfigType Type { get; set; }
    }

    public enum ConfigType
    {
        String,
        Int,
        Bool,
        Double
    }
}