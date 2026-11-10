using Xiletrade.Library.Services;

namespace Xiletrade.Test.CI.Auth;

public class ProtocolUrlHandlerTests
{
    [Theory]
    [InlineData("?id=0123456789abcdef0123456789abcdef", "0123456789abcdef0123456789abcdef")]
    [InlineData("?v=1&id=0123456789ABCDEF0123456789ABCDEF&realm=pc", "0123456789ABCDEF0123456789ABCDEF")]
    [InlineData("id=abc", "abc")]
    public void _01_GetQueryValue_Should_Return_Value_Of_Parameter(string query, string expected)
    {
        var value = ProtocolUrlHandler.GetQueryValue(query, "id");

        Assert.Equal(expected, value.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("?")]
    [InlineData("?sid=abc")]
    [InlineData("?id")]
    [InlineData("?realm=pc&v=1")]
    public void _02_GetQueryValue_Should_Return_Empty_When_Missing(string query)
    {
        var value = ProtocolUrlHandler.GetQueryValue(query, "id");

        Assert.True(value.IsEmpty);
    }
}
