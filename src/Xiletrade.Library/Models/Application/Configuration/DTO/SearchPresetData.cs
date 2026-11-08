using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

/// <summary>
/// Presets of the custom search : "search_presets" of the configuration, and content of the import / export files.
/// </summary>
public sealed class SearchPresetData
{
    [JsonPropertyName("unidentified_unique")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UniqueUnidentified[] UnidUnique { get; set; } = null;

    /// <summary>Presets of the custom search : category, rarity and stat filters.</summary>
    [JsonPropertyName("custom_search")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CustomSearchPreset[] CustomSearch { get; set; } = null;
}
