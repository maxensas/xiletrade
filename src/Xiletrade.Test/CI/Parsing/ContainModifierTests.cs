using FluentAssertions;
using System.Text.Json;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;

namespace Xiletrade.Test.CI.Parsing;

/// <summary>
/// <see cref="FilterDataExtensions.ContainModifier"/> (indexed) must give the same result as a scan of all the entries.
/// </summary>
public class ContainModifierTests
{
    [Fact]
    public void _01_Should_Match_The_Scan_With_Null_Empty_And_Duplicate_Texts()
    {
        var filter = new FilterData
        {
            Result =
            [
                new() { Entries = [new() { Text = "+# to maximum Life" }, new() { Text = null }] },
                new() { Entries = [new() { Text = "+# to maximum Life" }, new() { Text = "#% increased Damage" }] },
            ]
        };

        foreach (var text in new[] { "+# to maximum Life", "#% increased Damage", "", "+# to maximum life", "+# to maximum Life ", "x" })
        {
            filter.ContainModifier(text).Should().Be(Scan(filter, text), $"text '{text}'");
        }
    }

    [Theory]
    [InlineData("en-US", "Filters")]
    [InlineData("en-US", "FiltersTwo")]
    [InlineData("fr-FR", "Filters")]
    [InlineData("ja-JP", "FiltersTwo")]
    public void _02_Should_Match_The_Scan_On_Filters(string lang, string fileName)
    {
        var filter = LoadFilter(lang, fileName);
        var random = new Random(1234);

        foreach (var result in filter.Result)
        {
            foreach (var entry in result.Entries)
            {
                var text = entry.Text;
                filter.ContainModifier(text).Should().BeTrue();

                // modified text : usually not found, must match the scan anyway
                var modified = text.Length > 0
                    ? text.Remove(random.Next(text.Length), 1)
                    : "x";
                if (filter.ContainModifier(modified) != Scan(filter, modified))
                {
                    Assert.Fail($"text '{modified}'");
                }
            }
        }
    }

    // previous implementation
    private static bool Scan(FilterData filter, ReadOnlySpan<char> modText)
    {
        foreach (var result in filter.Result)
        {
            foreach (var entry in result.Entries)
            {
                if (entry.Text.AsSpan().SequenceEqual(modText))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static FilterData LoadFilter(string lang, string fileName)
    {
        string basePath = Environment.CurrentDirectory.Replace("Test", "Library");
        basePath = basePath[..basePath.IndexOf("bin")];
        using var stream = File.OpenRead(Path.Combine(basePath, "Data", "Lang", lang, fileName + ".json"));
        using var doc = JsonDocument.Parse(stream);

        return new FilterData
        {
            Result = [.. doc.RootElement.GetProperty("result").EnumerateArray().Select(result => new FilterResult
            {
                Entries = [.. result.GetProperty("entries").EnumerateArray()
                    .Select(e => new FilterResultEntrie { Text = e.GetProperty("text").GetString() })]
            })]
        };
    }
}
