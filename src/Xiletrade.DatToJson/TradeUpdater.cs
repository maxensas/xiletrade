using System.Globalization;
using System.Net;
using System.Text;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Shared;

namespace Xiletrade.DatToJson;

/// <summary>A file that could not be updated: the previous one is kept.</summary>
internal sealed record TradeFailure(string Culture, string File, string Url, string Reason);

/// <summary>
/// Downloads the data of the trade website (leagues, filters, currency, item types) of one game, for every language,
/// and writes the json files of the application: the "update all" of the configuration window, without the files
/// generated from the game.
/// </summary>
/// <remarks>
/// Same api, same models, same fixes (<see cref="TradeDataRules"/>) and same serializer as the application,
/// so the files are identical to the ones it writes.
/// One request at a time: a server that refuses or does not answer only leaves its own files unchanged.
/// Files are written through <see cref="JsonChanges"/>, which compares them with the previous generation first.
/// </remarks>
internal sealed class TradeUpdater(HttpClient http, string outputDirectory, JsonChanges changes)
{
    /// <summary>Answers "too many requests" or a server error: tried again, up to this number of attempts.</summary>
    private const int MaxAttempts = 3;

    /// <summary>Longest wait before trying again, even when the server asks for more.</summary>
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(60);

    private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(10);

    private readonly JsonHelper _json = new();

    /// <summary>Fixes the downloaded json, returns null when there is nothing to fix.</summary>
    private delegate byte[]? PrepareJson(ReadOnlySpan<byte> json, int idxLang);

    /// <summary>Files written (changed or not).</summary>
    internal int Written { get; private set; }

    internal List<TradeFailure> Failures { get; } = [];

    /// <summary>Number of files of a game: 4 per language.</summary>
    internal static int FileCount => Strings.Culture.Length * 4;

    internal async Task UpdateAsync(int game)
    {
        var poe2 = game is 2;
        for (var idxLang = 0; idxLang < Strings.Culture.Length; idxLang++)
        {
            var api = Strings.Api.GetUpdate(idxLang, poe2);
            Console.WriteLine($"PoE {game} {Strings.Culture[idxLang]} : {api}");

            await UpdateFileAsync<LeagueData>(api + "leagues", poe2 ? Strings.File._leagues2 : Strings.File._leagues1, idxLang,
                prepare: null, arrange: TradeDataRules.RemoveDuplicateLeagues, isValid: static d => d.Result is { Length: > 0 });
            await UpdateFileAsync<FilterData>(api + "stats", poe2 ? Strings.File._filters2 : Strings.File._filters1, idxLang,
                prepare: TradeDataRules.FixFilterJson, arrange: TradeDataRules.SortFilterEntries, isValid: static d => d.Result is { Length: > 0 });
            await UpdateFileAsync<CurrencyResult>(api + "static", poe2 ? Strings.File._currency2 : Strings.File._currency1, idxLang,
                prepare: null, arrange: null, isValid: static d => d.Result is { Length: > 0 });
            await UpdateFileAsync<ItemTypeResult>(api + "items", poe2 ? Strings.File._items2 : Strings.File._items1, idxLang,
                prepare: null, arrange: null, isValid: static d => d.Result is { Length: > 0 });
        }
    }

    /// <summary>Markdown summary of the downloads, for the report.</summary>
    internal void AppendReport(StringBuilder report, int game)
    {
        report.Append("# PoE ").Append(game).AppendLine(" trade data").AppendLine();
        report.Append(CultureInfo.InvariantCulture, $"Files downloaded: **{Written}** / {FileCount}.").AppendLine();
        if (Failures.Count is 0)
        {
            return;
        }

        report.AppendLine().AppendLine("Not updated (the previous file is kept):").AppendLine();
        report.AppendLine("| Language | File | Url | Error |");
        report.AppendLine("|---|---|---|---|");
        foreach (var failure in Failures)
        {
            report.Append(CultureInfo.InvariantCulture,
                $"| {failure.Culture} | {failure.File} | {failure.Url} | {failure.Reason.ReplaceLineEndings(" ").Replace('|', '/')} |").AppendLine();
        }
    }

    private async Task UpdateFileAsync<T>(string url, string fileName, int idxLang,
        PrepareJson? prepare, Func<T, T?>? arrange, Func<T, bool> isValid) where T : class
    {
        var culture = Strings.Culture[idxLang];
        try
        {
            var json = await DownloadAsync(url);
            if (prepare is not null && json.Length > 0)
            {
                json = prepare(json, idxLang) ?? json;
            }

            var data = _json.Deserialize<T>(json);
            if (data is not null && arrange is not null)
            {
                data = arrange(data);
            }
            // Never replace a file with an empty one (maintenance page, api change...).
            if (data is null || !isValid(data))
            {
                Fail(culture, fileName, url, "no data in the answer");
                return;
            }

            changes.Write(outputDirectory, Path.Combine(outputDirectory, culture, fileName), _json.SerializeToUtf8<T>(data));
            Written++;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException
            or System.Text.Json.JsonException or NotSupportedException or InvalidOperationException)
        {
            // HttpRequestException: refused or unreachable server (an html page instead of json ends as a JsonException).
            Fail(culture, fileName, url, ex.Message);
        }
    }

    private void Fail(string culture, string fileName, string url, string reason)
    {
        Console.WriteLine($"  {fileName} not updated: {reason}");
        Failures.Add(new TradeFailure(culture, fileName, url, reason));
    }

    /// <summary>GET, tried again on "too many requests" (waiting as asked by the server) and on server errors.</summary>
    private async Task<byte[]> DownloadAsync(string url)
    {
        for (var attempt = 1; ; attempt++)
        {
            TimeSpan delay;
            try
            {
                using var response = await http.GetAsync(url);
                if (response.IsSuccessStatusCode || attempt >= MaxAttempts
                    || (response.StatusCode is not HttpStatusCode.TooManyRequests && (int)response.StatusCode < 500))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsByteArrayAsync();
                }
                delay = response.Headers.RetryAfter?.Delta ?? DefaultRetryDelay;
            }
            catch (TaskCanceledException) when (attempt < MaxAttempts)
            {
                // Timeout.
                delay = DefaultRetryDelay;
            }
            await Task.Delay(delay < MaxRetryDelay ? delay : MaxRetryDelay);
        }
    }
}
