# RealtimeConfigDemoSDK
## Setup
1. Install the SDK
```
dotnet add package RealtimeConfigDemoSDK --version 1.0.0
```
2. Import the packages
```c#
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealtimeConfigDemoSDK.Services;
using RealtimeConfigDemoSDK.Services.Providers;
using RealtimeConfigDemoSDK.Services.Providers.Contracts;

...
            var gatewayUrl = config["GatewayUrl"] ?? throw new ArgumentNullException("GatewayUrl is required in appsettings.json");
            
            // Set up logging (optional, depends on your SDK's constructor requirements)
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<RealtimeConfigService>();

            // Initialize the SDK's services
            // Create an instance of IDistributedCache (example using MemoryDistributedCache)
            IDistributedCache distributedCache = new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
            ICacheProvider cacheProvider = new CacheProvider(distributedCache); // Example provider; replace if needed
            var realtimeConfigService = new RealtimeConfigService(cacheProvider, gatewayUrl, logger);
```
3. Consume the value
```c#
int a = await realtimeConfigService.GetConfigAsync(accountId, "KeyThresholdValue", 1000);
```

## Updating the package
1. Pack the SDK
```powershell
dotnet pack --configuration Release
```
2. Publish
```powershell
dotnet nuget push RealtimeConfigDemoSDK/bin/Release/RealtimeConfigSemoSDK.1.0.0.nupkg --source github 
```