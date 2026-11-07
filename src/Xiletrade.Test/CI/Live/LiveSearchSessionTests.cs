using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text;
using Xiletrade.Library.Models.Application.LiveSearch;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Test.CI.Live;

/// <summary>
/// Messages of the websocket of a live search, and limit of the live results in one minute.
/// </summary>
public class LiveSearchSessionTests
{
    [Theory]
    [InlineData("{\"auth\":true}", true)]
    [InlineData("{\"auth\":false}", false)]
    public void _01_Auth_Message_Should_Be_Read(string json, bool expected)
    {
        LiveSearchSession.TryReadMessage(Encoding.UTF8.GetBytes(json), out var auth, out var token).Should().BeTrue();
        auth.Should().Be(expected);
        token.Should().BeNull();
    }

    [Theory]
    [InlineData("{\"result\":\"abc.def.ghi\",\"count\":1}")]
    [InlineData("{\"other\":{\"a\":[1,2]},\"result\":\"abc.def.ghi\"}")]
    public void _02_Result_Message_Should_Give_Its_Token(string json)
    {
        LiveSearchSession.TryReadMessage(Encoding.UTF8.GetBytes(json), out var auth, out var token).Should().BeTrue();
        auth.Should().BeNull();
        token.Should().Be("abc.def.ghi");
    }

    [Theory]
    [InlineData("{\"count\":2}")]
    [InlineData("[1,2]")]
    [InlineData("not json")]
    [InlineData("")]
    public void _03_Other_Messages_Should_Be_Ignored(string json)
    {
        LiveSearchSession.TryReadMessage(Encoding.UTF8.GetBytes(json), out _, out _).Should().BeFalse();
    }

    [Fact]
    public void _04_Too_Many_Results_In_One_Minute_Should_Stop_The_Search()
    {
        using var session = CreateSession(maxResultsPerMinute: 3);
        session.IsTooBroad().Should().BeFalse();
        session.IsTooBroad().Should().BeFalse();
        session.IsTooBroad().Should().BeFalse();
        session.IsTooBroad().Should().BeTrue();
    }

    [Fact]
    public void _05_Without_Limit_A_Search_Should_Never_Be_Too_Broad()
    {
        using var session = CreateSession(maxResultsPerMinute: 0);
        for (int i = 0; i < 100; i++)
        {
            session.IsTooBroad().Should().BeFalse();
        }
    }

    private static LiveSearchSession CreateSession(int maxResultsPerMinute)
        => new(Mock.Of<INetService>(), "wss://localhost/live", "https://localhost/fetch/", "https://localhost/search",
            "id", maxResultsPerMinute, (_, _) => { }, _ => { }, NullLogger.Instance);
}
