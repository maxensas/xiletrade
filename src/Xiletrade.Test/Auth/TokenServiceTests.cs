using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Test.Auth;

public class TokenServiceTests
{
    //private readonly IMessageAdapterService _message;
    private readonly ITokenService _token;
    //private readonly DataManagerService _dm;

    public TokenServiceTests()
    {
        // Simuler IMessageAdapterService (mock requis si erreur dans le service)
        var messageAdapterMock = new Mock<IMessageAdapterService>();
        var services = new ServiceCollection();
        services.AddSingleton(messageAdapterMock.Object);
        services.AddSingleton<DataManagerService>();
        services.AddSingleton<ITokenService, TokenService>();
        var sp = services.BuildServiceProvider();

        //_message = sp.GetRequiredService<IMessageAdapterService>();
        //_dm = sp.GetRequiredService<DataManagerService>();
        _token = sp.GetRequiredService<ITokenService>();
    }

    [Fact]
    public void TryInitToken_WithValidToken_LoadReturnsSameToken()
    {
        // Arrange
        var expireDays = 90;
        var query = $"access_token=test-token-123&expires_in={expireDays}";

        // Act
        var success = _token.TryInitToken(query);

        // Assert
        Assert.True(success);

        var token = _token.CacheToken;

        Assert.NotNull(token);
        Assert.Equal("test-token-123", token.AccessToken);
        Assert.False(token.IsExpired());
    }

    [Fact]
    public void TryInitToken_WithInvalidQuery_ReturnsFalse()
    {
        // Arrange
        var query = "foo=bar";

        // Act
        _token.ClearTokens();
        var result = _token.TryInitToken(query);

        // Assert
        Assert.False(result);
        Assert.Null(_token.CacheToken);
    }
}

