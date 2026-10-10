using Microsoft.Extensions.DependencyInjection;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Test.Common;

namespace Xiletrade.Test.Auth;

public class TokenServiceTests : LibServiceConfiguration
{
    private readonly ITokenService _token;

    public TokenServiceTests()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        _token = provider.GetRequiredService<ITokenService>();
    }

    [Fact]
    public void _01_Init_Token_With_Valid_Query_Returns_AccessToken()
    {
        // Arrange
        var expireDays = 90;
        var query = $"access_token=test-token-123&expires_in={expireDays}";

        // Act
        var success = _token.TryInitToken(query);
        var token = _token.CacheToken;

        // Assert
        Assert.True(success);
        Assert.NotNull(token);
        Assert.Equal("test-token-123", token.AccessToken);
        Assert.False(token.IsExpired());
    }

    [Fact]
    public void _02_Init_Token_With_Bad_Query_Returns_Null_Token()
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

