using FluentAssertions;
using System.Text.Json;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Application.Serialization;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Test.CI.Parsing;

/// <summary>
/// The custom search presets ("search_presets" of Data/DefaultConfig.json) must only use what the trade site knows :
/// stat ids of the filters, item categories, rarities and stat groups, and names of the min / max filters.
/// </summary>
public class SearchPresetTests
{
    private static readonly string[] _rarities = [ "any", "normal", "magic", "rare", "unique", "uniquefoil", "nonunique" ];
    private static readonly string[] _groups = [ "and", "not", "count" ];

    [Fact]
    public void _01_Custom_Search_Presets_Should_Use_Known_Values()
    {
        var presets = LoadPresets().CustomSearch;
        presets.Should().NotBeNullOrEmpty();

        var errors = new List<string>();
        foreach (var preset in presets)
        {
            var filters = preset.Poe2 ? "FiltersTwo.json" : "Filters.json";
            var items = preset.Poe2 ? "ItemsTwo.json" : "Items.json";
            var statIds = LoadIds(filters, "entries");
            var categories = LoadIds(items, null);

            if (string.IsNullOrEmpty(preset.Name))
                errors.Add("preset without name");
            if (preset.Category is not null && !categories.Contains(preset.Category))
                errors.Add($"{preset.Name} : unknown category '{preset.Category}' in {items}");
            if (preset.Rarity is not null && !_rarities.Contains(preset.Rarity))
                errors.Add($"{preset.Name} : unknown rarity '{preset.Rarity}'");
            foreach (var filter in preset.Filters ?? [])
            {
                if (!Enum.TryParse<StatPanel>(filter.Id, ignoreCase: true, out var id) || id is StatPanel.NoStat
                    || int.TryParse(filter.Id, out _))
                    errors.Add($"{preset.Name} : unknown filter '{filter.Id}'");
                if (filter.Min is null && filter.Max is null)
                    errors.Add($"{preset.Name} : filter '{filter.Id}' without min or max");
            }
            if (preset.Buyout is not null && preset.GetBuyoutIndex() is 0)
                errors.Add($"{preset.Name} : unknown buyout option '{preset.Buyout}'");
            foreach (var stat in preset.Stats ?? [])
            {
                if (!statIds.Contains(stat.Id))
                    errors.Add($"{preset.Name} : unknown stat '{stat.Id}' in {filters}");
                if (stat.Group is not null && !_groups.Contains(stat.Group))
                    errors.Add($"{preset.Name} : unknown group '{stat.Group}'");
            }
        }
        errors.Should().BeEmpty();
    }

    // index of the buyout price of the form <-> option of the trade site, as sent by the item (XiletradeItem)
    [Theory]
    [InlineData(false, 0, null)]
    [InlineData(false, 1, "chaos")]
    [InlineData(false, 2, "chaos_divine")]
    [InlineData(false, 3, "divine")]
    [InlineData(true, 0, null)]
    [InlineData(true, 1, "exalted")]
    [InlineData(true, 2, "exalted_divine")]
    [InlineData(true, 3, "chaos")]
    [InlineData(true, 4, "divine")]
    public void _02_Buyout_Option_Should_Match_The_Form(bool isPoe2, int index, string option)
    {
        new BuyoutPrice(isPoe2).GetOption(index).Should().Be(option);
        new CustomSearchPreset { Poe2 = isPoe2, Buyout = option }.GetBuyoutIndex().Should().Be(index);
    }

    [Fact]
    public void _03_Buyout_Option_Of_The_Other_Game_Should_Be_Removed()
    {
        var preset = new CustomSearchPreset { Name = "test", Poe2 = false, Buyout = "exalted" };
        var report = new List<string>();
        preset.Repair(null, report).Should().BeTrue();
        preset.Buyout.Should().BeNull();
        report.Should().ContainSingle();
    }

    private static SearchPresetData LoadPresets()
    {
        using var stream = File.OpenRead(Path.Combine(GetDataPath(), "DefaultConfig.json"));
        return new JsonHelper().Deserialize<ConfigData>(stream).SearchPreset;
    }

    // english ids : of the results (item categories), or of their entries (stats)
    private static HashSet<string> LoadIds(string fileName, string entries)
    {
        using var stream = File.OpenRead(Path.Combine(GetDataPath(), "Lang", "en-US", fileName));
        using var doc = JsonDocument.Parse(stream);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var result in doc.RootElement.GetProperty("result").EnumerateArray())
        {
            if (entries is null)
            {
                ids.Add(result.GetProperty("id").GetString() ?? string.Empty);
                continue;
            }
            foreach (var entry in result.GetProperty(entries).EnumerateArray())
            {
                ids.Add(entry.GetProperty("id").GetString() ?? string.Empty);
            }
        }
        return ids;
    }

    private static string GetDataPath()
    {
        string basePath = Environment.CurrentDirectory.Replace("Test", "Library");
        basePath = basePath[..basePath.IndexOf("bin")];
        return Path.Combine(basePath, "Data");
    }
}
