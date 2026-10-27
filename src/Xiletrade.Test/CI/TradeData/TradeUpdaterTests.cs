using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Xiletrade.DatToJson;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Test.CI.TradeData;

public class TradeUpdaterTests
{
    private const string Leagues = "{\"result\":[{\"id\":\"Standard\",\"text\":\"Standard\"},{\"id\":\"Standard\",\"text\":\"Standard\"},{\"id\":\"Hardcore\",\"text\":\"Hardcore\"}]}";
    private const string Filters = "{\"result\":[{\"label\":\"Explicit\",\"entries\":[{\"id\":\"explicit.b\",\"text\":\"B\\u000b\",\"type\":\"explicit\"},{\"id\":\"explicit.a\",\"text\":\"A\",\"type\":\"explicit\"}]}]}";
    private const string Currency = "{\"result\":[{\"id\":\"Currency\",\"label\":\"Currency\",\"entries\":[{\"id\":\"alt\",\"text\":\"Orb of Alteration\",\"image\":\"/alt.png\"}]}]}";
    private const string Items = "{\"result\":[{\"id\":\"accessory\",\"label\":\"Accessories\",\"entries\":[{\"type\":\"Jet Amulet\"}]}]}";

    [Fact]
    public async Task _01_Every_Language_Should_Be_Written_With_The_Application_Fixes()
    {
        // Arrange
        var output = Directory.CreateTempSubdirectory().FullName;
        using var http = new HttpClient(new FakeHandler(Answer));
        var changes = new JsonChanges(null);
        var updater = new TradeUpdater(http, output, changes);

        try
        {
            // Act
            await updater.UpdateAsync(2);

            // Assert
            updater.Failures.Should().BeEmpty();
            updater.Written.Should().Be(TradeUpdater.FileCount);
            changes.Files.Should().HaveCount(TradeUpdater.FileCount).And.OnlyContain(f => f.Status == JsonFileStatus.Added);
            var json = new JsonHelper();

            var leagues = json.Deserialize<LeagueData>(File.ReadAllBytes(Path.Combine(output, "fr-FR", "LeaguesTwo.json")));
            leagues.Result.Select(l => l.Id).Should().Equal("Standard", "Hardcore");

            var filters = json.Deserialize<FilterData>(File.ReadAllBytes(Path.Combine(output, "en-US", "FiltersTwo.json")));
            filters.Result[0].Entries.Select(e => e.ID).Should().Equal("explicit.a", "explicit.b");

            // The line tabulation is only removed for the portuguese filters, as the application does.
            json.Deserialize<FilterData>(File.ReadAllBytes(Path.Combine(output, "pt-BR", "FiltersTwo.json")))
                .Result[0].Entries[1].Text.Should().Be("B");
            File.Exists(Path.Combine(output, "ja-JP", "CurrencyTwo.json")).Should().BeTrue();
            File.Exists(Path.Combine(output, "zh-CN", "ItemsTwo.json")).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    [Fact]
    public async Task _02_Refused_Server_Should_Keep_Its_Previous_Files()
    {
        // Arrange: the chinese server answers with an html page.
        var output = Directory.CreateTempSubdirectory().FullName;
        var previous = Path.Combine(output, "zh-CN", "Leagues.json");
        Directory.CreateDirectory(Path.GetDirectoryName(previous)!);
        File.WriteAllText(previous, "previous");
        using var http = new HttpClient(new FakeHandler(request => request.RequestUri!.Host is "poe.game.qq.com"
            ? new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("<html>denied</html>") }
            : Answer(request)));
        var updater = new TradeUpdater(http, output, new JsonChanges(null));

        try
        {
            // Act
            await updater.UpdateAsync(1);

            // Assert
            updater.Failures.Should().HaveCount(4).And.OnlyContain(f => f.Culture == "zh-CN");
            updater.Written.Should().Be(TradeUpdater.FileCount - 4);
            File.ReadAllText(previous).Should().Be("previous");
            File.Exists(Path.Combine(output, "ko-KR", "Leagues.json")).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    [Fact]
    public async Task _03_Empty_Answer_Should_Not_Replace_A_File()
    {
        // Arrange: no league at all (maintenance, api change...).
        var output = Directory.CreateTempSubdirectory().FullName;
        using var http = new HttpClient(new FakeHandler(request => request.RequestUri!.AbsolutePath.EndsWith("/leagues")
            ? Json("{\"result\":[]}")
            : Answer(request)));
        var updater = new TradeUpdater(http, output, new JsonChanges(null));

        try
        {
            // Act
            await updater.UpdateAsync(1);

            // Assert
            updater.Failures.Should().HaveCount(11).And.OnlyContain(f => f.File == "Leagues.json" && f.Reason == "no data in the answer");
            File.Exists(Path.Combine(output, "en-US", "Leagues.json")).Should().BeFalse();
            File.Exists(Path.Combine(output, "en-US", "Filters.json")).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    [Fact]
    public async Task _04_Too_Many_Requests_Should_Be_Tried_Again()
    {
        // Arrange: the first request of each url is refused, the server asks to wait 0 second.
        var output = Directory.CreateTempSubdirectory().FullName;
        HashSet<string> refused = [];
        using var http = new HttpClient(new FakeHandler(request =>
        {
            if (refused.Add(request.RequestUri!.AbsoluteUri))
            {
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
                return response;
            }
            return Answer(request);
        }));
        var updater = new TradeUpdater(http, output, new JsonChanges(null));

        try
        {
            // Act
            await updater.UpdateAsync(1);

            // Assert
            updater.Failures.Should().BeEmpty();
            updater.Written.Should().Be(TradeUpdater.FileCount);
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    [Fact]
    public async Task _05_Second_Run_Should_Show_Only_The_Changed_Entries()
    {
        // Arrange: a new league between two runs.
        var output = Directory.CreateTempSubdirectory().FullName;
        var leagues = Leagues;
        using var http = new HttpClient(new FakeHandler(request => request.RequestUri!.AbsolutePath.EndsWith("/leagues")
            ? Json(leagues)
            : Answer(request)));

        try
        {
            await new TradeUpdater(http, output, new JsonChanges(null)).UpdateAsync(1);
            leagues = Leagues.Replace("]}", ",{\"id\":\"Keepers\",\"text\":\"Keepers\"}]}", StringComparison.Ordinal);
            using var diff = new MemoryStream();
            var changes = new JsonChanges(diff);

            // Act
            await new TradeUpdater(http, output, changes).UpdateAsync(1);

            // Assert
            changes.Files.Where(f => f.Status != JsonFileStatus.Unchanged).Should().HaveCount(11)
                .And.OnlyContain(f => f.Path.EndsWith("/Leagues.json") && f.Removed == 0 && f.Added == 1 && f.Moved == 0);
            Encoding.UTF8.GetString(diff.ToArray()).Should().StartWith(
                "diff --git a/en-US/Leagues.json b/en-US/Leagues.json\n" +
                "--- a/en-US/Leagues.json\n" +
                "+++ b/en-US/Leagues.json\n" +
                "@@ -2,0 +3 @@\n" +
                "+{\"id\":\"Keepers\",\"text\":\"Keepers\"}\n");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static HttpResponseMessage Answer(HttpRequestMessage request)
    {
        var path = request.RequestUri!.AbsolutePath;
        return Json(path.EndsWith("/leagues") ? Leagues
            : path.EndsWith("/stats") ? Filters
            : path.EndsWith("/static") ? Currency
            : Items);
    }

    private static HttpResponseMessage Json(string json)
        => new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> answer) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(answer(request));
    }
}
