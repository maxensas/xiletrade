using FluentAssertions;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Test.CI.Net;

public class RateLimitInfoTests
{
    private const string Policy = "trade-fetch-request-limit";

    [Fact]
    public void _01_Single_Item_Should_Give_Remaining_Requests()
    {
        var info = RateLimitInfo.Create(Policy, [("10:5:10", "1:5:0")]);

        info.Should().NotBeNull();
        info.Policy.Should().Be(Policy);
        info.Remaining.Should().Be(9);
        info.Restriction.Should().Be(0);
        info.Spare.Should().Be(9);
    }

    [Fact]
    public void _02_Most_Constrained_Item_Should_Win()
    {
        var info = RateLimitInfo.Create(Policy, [("8:10:60,15:60:120", "3:10:0,14:60:0")]);

        info.Remaining.Should().Be(1);
        info.Spare.Should().Be(1);
    }

    [Fact]
    public void _03_Most_Constrained_Rule_Should_Win()
    {
        var info = RateLimitInfo.Create(Policy, [("12:4:10", "2:4:0"), ("6:10:60", "5:10:0")]);

        info.Remaining.Should().Be(1);
    }

    [Fact]
    public void _04_Exceeded_Limit_Should_Report_The_Restriction()
    {
        var info = RateLimitInfo.Create(Policy, [("10:5:10", "11:5:10")]);

        info.Remaining.Should().Be(0);
        info.Restriction.Should().Be(10);
        info.Spare.Should().Be(0);
    }

    [Theory]
    [InlineData("10:5:10,5:60:30", "1:5:0")]  // different number of items
    [InlineData("10:5", "1:5:0")]             // missing penalty
    [InlineData("a:5:10", "1:5:0")]           // not a number
    [InlineData("10:5:10", "1:5:x")]
    [InlineData("", "")]
    public void _05_Unreadable_Values_Should_Give_An_Unknown_State(string limit, string state)
    {
        RateLimitInfo.Create(Policy, [(limit, state)]).Should().BeNull();
    }

    [Fact]
    public void _06_No_Rule_Should_Give_An_Unknown_State()
    {
        RateLimitInfo.Create(Policy, []).Should().BeNull();
    }

    [Fact]
    public void _07_Headers_Of_A_Response_Should_Be_Read()
    {
        using var response = new HttpResponseMessage();
        response.Headers.Add("X-Rate-Limit-Policy", Policy);
        response.Headers.Add("X-Rate-Limit-Rules", "Ip,Account");
        response.Headers.Add("X-Rate-Limit-Ip", "12:4:10,16:12:60");
        response.Headers.Add("X-Rate-Limit-Ip-State", "1:4:0,1:12:0");
        response.Headers.Add("X-Rate-Limit-Account", "6:4:10");
        response.Headers.Add("X-Rate-Limit-Account-State", "2:4:0");

        var info = RateLimitInfo.Read(response.Headers);

        info.Should().NotBeNull();
        info.Policy.Should().Be(Policy);
        info.Remaining.Should().Be(4);
    }

    [Fact]
    public void _08_Response_Without_Rate_Limit_Headers_Should_Give_An_Unknown_State()
    {
        using var response = new HttpResponseMessage();

        RateLimitInfo.Read(response.Headers).Should().BeNull();
    }

    [Fact]
    public void _09_Fresh_State_Should_Expire()
    {
        var info = RateLimitInfo.Create(Policy, [("10:5:10", "1:5:0")]);

        info.IsFresh(5_000).Should().BeTrue();
        info.IsFresh(-1).Should().BeFalse();
    }

    [Fact]
    public void _10_Retry_After_Should_Be_Read()
    {
        using var response = new HttpResponseMessage();
        response.Headers.Add("X-Rate-Limit-Policy", Policy);
        response.Headers.Add("X-Rate-Limit-Rules", "Ip");
        response.Headers.Add("X-Rate-Limit-Ip", "10:5:10");
        response.Headers.Add("X-Rate-Limit-Ip-State", "11:5:10");
        response.Headers.Add("Retry-After", "7");

        RateLimitInfo.Read(response.Headers).RetryAfter.Should().Be(7);
    }

    [Fact]
    public void _11_Missing_Retry_After_Should_Give_Zero()
    {
        RateLimitInfo.Create(Policy, [("10:5:10", "1:5:0")]).RetryAfter.Should().Be(0);
    }

    [Theory]
    [InlineData("trade-search-request-limit", true)]
    [InlineData("trade-fetch-request-limit", true)]
    [InlineData("trade-exchange-request-limit", true)]
    [InlineData("static-data-request-limit", false)]
    [InlineData("", false)]
    public void _12_Only_Trade_Policies_Should_Be_Tracked(string policy, bool expected)
    {
        RateLimitInfo.IsTradePolicy(policy).Should().Be(expected);
    }
}
