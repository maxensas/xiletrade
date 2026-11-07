using FluentAssertions;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Test.CI.Live;

/// <summary>
/// Urls of the live searches, as on the trade site : websocket, page of the search (referer) and site of the session cookie.
/// </summary>
[Collection(GameContextCollection.Name)]
public sealed class LiveSearchUrlTests : IDisposable
{
    // gateways : 0 www, 1 kakao, 2 fr, 8 taiwan, 9 tencent
    [Theory]
    [InlineData(false, 0, "wss://www.pathofexile.com/api/trade/live/")]
    [InlineData(true, 0, "wss://www.pathofexile.com/api/trade2/live/poe2/")]
    [InlineData(false, 2, "wss://fr.pathofexile.com/api/trade/live/")]
    [InlineData(false, 1, "wss://poe.kakaogames.com/api/trade/live/")]
    public void _01_Live_Url_Should_Follow_The_Trade_Site(bool isPoe2, int gateway, string expected)
    {
        Strings.InitializeGameContext(isPoe2, gateway);
        Strings.Api.Live.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, 0, "https://www.pathofexile.com/trade/search/Standard/abc")]
    [InlineData(true, 0, "https://www.pathofexile.com/trade2/search/poe2/Standard/abc")]
    [InlineData(false, 1, "https://poe.kakaogames.com/trade/search/Standard/abc")]
    public void _02_Search_Page_Should_Follow_The_Trade_Site(bool isPoe2, int gateway, string expected)
    {
        Strings.InitializeGameContext(isPoe2, gateway);
        Strings.Api.GetSearchPage("Standard", "abc").Should().Be(expected);
    }

    [Theory]
    [InlineData(0, ".pathofexile.com")]
    [InlineData(2, ".pathofexile.com")]
    [InlineData(1, "poe.kakaogames.com")]
    [InlineData(8, "pathofexile.tw")]
    [InlineData(9, "poe.game.qq.com")]
    public void _03_Session_Cookie_Should_Belong_To_The_Site_Of_The_Gateway(int gateway, string expected)
    {
        Strings.InitializeGameContext(false, gateway);
        NetService.GetSessionDomain().Should().Be(expected);
    }

    // default context : path of exile 1, www
    public void Dispose() => Strings.InitializeGameContext(false, 0);
}
