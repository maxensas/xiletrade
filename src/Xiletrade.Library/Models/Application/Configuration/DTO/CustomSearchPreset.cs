using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

/// <summary>
/// Preset of the custom search ("search_presets" of the configuration) : item category, rarity, filters of the sections, options and stat filters.
/// </summary>
public sealed class CustomSearchPreset
{
    /// <summary>Name displayed in the preset list.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("poe2")]
    public bool Poe2 { get; set; }

    /// <summary>The search is launched when the preset is selected, false by default : the criteria are only set.</summary>
    [JsonPropertyName("auto_search")]
    public bool AutoSearch { get; set; }

    /// <summary>Item category of the trade site (e.g. "map"), none : any category.</summary>
    [JsonPropertyName("category")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Category { get; set; } = null;

    /// <summary>Rarity of the trade site : any, normal, magic, rare, unique, uniquefoil or nonunique, none : unchanged.</summary>
    [JsonPropertyName("rarity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Rarity { get; set; } = null;

    /// <summary>Minimal number of stats of the count group.</summary>
    [JsonPropertyName("count_min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? CountMin { get; set; } = null;

    /// <summary>Maximal number of stats of the count group.</summary>
    [JsonPropertyName("count_max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? CountMax { get; set; } = null;

    /// <summary>Min / max filters of the sections (type, weapon, map...).</summary>
    [JsonPropertyName("filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CustomSearchPresetFilter[] Filters { get; set; } = null;

    /// <summary>Options of the miscellaneous section (identified, corrupted...), none : any.</summary>
    [JsonPropertyName("options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CustomSearchPresetOptions Options { get; set; } = null;

    [JsonPropertyName("stats")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CustomSearchPresetStat[] Stats { get; set; } = null;
}
