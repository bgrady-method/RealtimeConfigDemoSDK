using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using RealtimeConfigDemoSDK.Services;
using RealtimeConfigDemoSDK.Models;
using RealtimeConfigDemoSDK.Services.Providers.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RestSharp;

public class RealtimeConfigServiceTests
{
    private readonly Mock<ICacheProvider> _cacheProviderMock;
    private readonly Mock<RestClient> _restClientMock;
    private readonly ILogger<RealtimeConfigService> _logger;

    public RealtimeConfigServiceTests()
    {
        _cacheProviderMock = new Mock<ICacheProvider>();
        _restClientMock = new Mock<RestClient>();
        _logger = new NullLogger<RealtimeConfigService>();
    }

    [Fact]
    public async Task GetConfigAsync_ReturnsCachedValue_WhenConfigIsInCache()
    {
        // Arrange
        var accountId = 1;
        var key = "testKey";
        var defaultValue = "defaultValue";
        var cachedValue = new RealtimeConfig { AccountId = accountId, Key = key, Value = "cachedValue", Type = ConfigType.String };

        _cacheProviderMock.Setup(cp => cp.GetAsync<RealtimeConfig>(It.Is<string>(k => k.Contains(key))))
            .ReturnsAsync(cachedValue);

        var service = new RealtimeConfigService(_cacheProviderMock.Object, "http://dummyurl", _logger);

        // Act
        var result = await service.GetConfigAsync(accountId, key, defaultValue);

        // Assert
        Assert.Equal("cachedValue", result);
    }
}
