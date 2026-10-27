using FluentAssertions;
using System.Text;
using Xiletrade.Library.Shared;

namespace Xiletrade.Test.CI.Shared;

public class CommonEscapeDataStringTests
{
    [Theory]
    [InlineData("")]
    [InlineData("abcXYZ019-._~")]
    [InlineData("{\"query\":{\"status\":{\"option\":\"online\"}}}")]
    [InlineData("caf\u00e9 \u20ac \u3042 \U0001F600")]
    [InlineData("a b+c/d?e=f&g#h%i!*'()")]
    public void _01_Should_Give_The_Same_Result_As_Uri_EscapeDataString(string text)
    {
        Common.EscapeDataString(Encoding.UTF8.GetBytes(text)).Should().Be(Uri.EscapeDataString(text));
    }

    [Fact]
    public void _02_Long_Text_Should_Be_Encoded()
    {
        var text = new string('\u00e9', 2000);

        Common.EscapeDataString(Encoding.UTF8.GetBytes(text)).Should().Be(Uri.EscapeDataString(text));
    }
}
