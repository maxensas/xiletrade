using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Test.Auth;

public class TokenServiceTests
{
    private readonly IMessageAdapterService _message;
    private readonly DataManagerService _dm;

    public TokenServiceTests()
    {
        // Simuler IMessageAdapterService (mock requis si erreur dans le service)
        var messageAdapterMock = new Mock<IMessageAdapterService>();
        var services = new ServiceCollection();
        services.AddSingleton(messageAdapterMock.Object);
        services.AddSingleton<DataManagerService>();
        var serviceProvider = services.BuildServiceProvider();
        _message = serviceProvider.GetRequiredService<IMessageAdapterService>();
        _dm = serviceProvider.GetRequiredService<DataManagerService>();
        _dm.TryInit();
    }

    [Fact]
    public void TryInitToken_WithValidToken_LoadReturnsSameToken()
    {
        // Arrange
        var tokenService = new TokenService(_message, _dm);
        var expireDays = 90;
        var query = $"access_token=test-token-123&expires_in={expireDays}";

        // Act
        var success = tokenService.TryInitToken(query);

        // Assert
        Assert.True(success);

        var token = tokenService.CacheToken;

        Assert.NotNull(token);
        Assert.Equal("test-token-123", token.AccessToken);
        Assert.False(token.IsExpired());
    }

    [Fact]
    public void TryInitToken_WithInvalidQuery_ReturnsFalse()
    {
        // Arrange
        var tokenService = new TokenService(_message, _dm);
        var query = "foo=bar";

        // Act
        tokenService.ClearTokens();
        var result = tokenService.TryInitToken(query);

        // Assert
        Assert.False(result);
        Assert.Null(tokenService.CacheToken);
    }
}

