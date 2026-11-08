using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

/// <summary>
/// Stat filter of a custom search preset : the text is taken from the filters of the current language with the id.
/// </summary>
public sealed class CustomSearchPresetStat
{
    /// <summary>Id of the stat on the trade site (e.g. "pseudo.pseudo_number_of_affix_mods").</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Min { get; set; } = null;

    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Max { get; set; } = null;

    /// <summary>Stat group : "and" (default), "not" or "count".</summary>
    [JsonPropertyName("group")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Group { get; set; } = null;
}
