using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Moq;
using Xunit;

namespace Xiletrade.Test.Auth;

public class TokenServiceTests
{
    private readonly IServiceProvider _serviceProvider;

    public TokenServiceTests()
    {
        // Simuler IMessageAdapterService (mock requis si erreur dans le service)
        var messageAdapterMock = new Mock<IMessageAdapterService>();
        var services = new ServiceCollection();
        services.AddSingleton(messageAdapterMock.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void TryParseQuery_WithValidToken_LoadReturnsSameToken()
    {
        // Arrange
        var tokenService = new TokenService(_serviceProvider);
        var expireDays = 90;
        var query = $"access_token=test-token-123&expires_in={expireDays}";

        // Act
        var success = tokenService.TryParseQuery(query);

        // Assert
        Assert.True(success);

        var token = tokenService.CacheToken;

        Assert.NotNull(token);
        Assert.Equal("test-token-123", token.AccessToken);
        Assert.False(token.IsExpired());
    }

    [Fact]
    public void TryParseQuery_WithInvalidQuery_ReturnsFalse()
    {
        // Arrange
        var tokenService = new TokenService(_serviceProvider);
        var query = "foo=bar";

        // Act
        tokenService.Clear();
        var result = tokenService.TryParseQuery(query);

        // Assert
        Assert.False(result);
        Assert.Null(tokenService.CacheToken);
    }
}

