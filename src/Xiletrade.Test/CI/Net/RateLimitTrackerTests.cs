using FluentAssertions;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Test.CI.Net;

/// <summary>
/// Fetch requests compared to the limits of the trade api : requests sent, windows emptying with time,
/// hits of other clients (spread over their own window only) and restrictions given by the responses.
/// </summary>
public class RateLimitTrackerTests
{
    private const string Policy = "trade-fetch-request-limit";

    private long _now = 1_000_000;

    private RateLimitTracker CreateTracker() => new(() => _now);

    [Fact]
    public void _01_Unknown_Limits_Should_Not_Wait()
    {
        var tracker = CreateTracker();
        tracker.OnSent();
        tracker.GetWait(5, 1).Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void _02_Full_Window_Should_Wait_Until_Its_Oldest_Request_Leaves()
    {
        var tracker = CreateTracker();
        tracker.OnSent(); // t = 0
        _now += 1000;
        tracker.OnSent(); // t = 1 s
        tracker.OnResponse(RateLimitInfo.Create(Policy, [("4:4:10", "2:4:0")]));

        tracker.GetWait(2, 1).Should().Be(TimeSpan.Zero); // 2 + 2 <= 4
        tracker.GetWait(3, 1).Should().Be(TimeSpan.FromSeconds(3)); // the first one leaves the window of 4 s at t = 4 s
    }

    [Fact]
    public void _03_Part_Of_The_Window_Should_Be_Kept()
    {
        var tracker = CreateTracker();
        tracker.OnResponse(RateLimitInfo.Create(Policy, [("10:4:10", "0:4:0")]));
        for (int i = 0; i < 6; i++)
        {
            tracker.OnSent();
        }

        tracker.GetWait(1, 0.6).Should().BeGreaterThan(TimeSpan.Zero); // 7 > 6 : the live searches stop
        tracker.GetWait(1, 1).Should().Be(TimeSpan.Zero); // a price check still can
    }

    [Fact]
    public void _04_Window_Should_Empty_With_Time()
    {
        var tracker = CreateTracker();
        tracker.OnResponse(RateLimitInfo.Create(Policy, [("2:4:10", "0:4:0")]));
        tracker.OnSent();
        tracker.OnSent();
        tracker.GetWait(1, 1).Should().BeGreaterThan(TimeSpan.Zero);

        _now += 4001;
        tracker.GetWait(1, 1).Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void _05_Hits_Of_Other_Clients_Should_Be_Counted()
    {
        var tracker = CreateTracker();
        tracker.OnSent();
        // the server counts 4 hits : 3 come from elsewhere (trade site in a browser)
        tracker.OnResponse(RateLimitInfo.Create(Policy, [("5:10:60", "4:10:0")]));

        tracker.GetWait(1, 1).Should().Be(TimeSpan.Zero);
        tracker.GetWait(2, 1).Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void _06_Restriction_Should_Block_Until_Its_End()
    {
        var tracker = CreateTracker();
        tracker.OnResponse(RateLimitInfo.Create(Policy, [("5:10:60", "6:10:60")]));

        tracker.GetWait(1, 1).Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void _07_More_Requests_Than_The_Window_Allows_Should_Wait_The_Whole_Period()
    {
        var tracker = CreateTracker();
        tracker.OnResponse(RateLimitInfo.Create(Policy, [("4:12:60", "0:12:0")]));

        tracker.GetWait(5, 1).Should().Be(TimeSpan.FromSeconds(12));
    }

    [Fact]
    public void _08_Hits_Of_A_Long_Window_Should_Not_Block_A_Short_One()
    {
        var tracker = CreateTracker();
        // 300 hits in 3 hours, 30 in 5 minutes : nothing in the last 12 seconds
        tracker.OnResponse(RateLimitInfo.Create(Policy, [("12:4:10,16:12:300,100:300:600,1000:10800:3600", "0:4:0,0:12:0,30:300:0,300:10800:0")]));

        tracker.GetWait(1, 0.6).Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void _09_Hits_Of_Other_Clients_Should_Leave_Their_Window_Evenly()
    {
        var tracker = CreateTracker();
        tracker.OnResponse(RateLimitInfo.Create(Policy, [("10:100:60", "8:100:0")]));

        tracker.GetWait(1, 1).Should().Be(TimeSpan.Zero); // 8 + 1 <= 10
        // 6 allowed : the 8 hits have to go down to 5, after 3/8 of the period
        tracker.GetWait(1, 0.6).Should().Be(TimeSpan.FromMilliseconds(37_500));
    }
}
