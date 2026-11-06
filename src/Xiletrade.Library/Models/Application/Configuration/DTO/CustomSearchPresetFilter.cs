using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

/// <summary>
/// Min / max filter of a custom search preset (item level, map tier, damage...).
/// </summary>
public sealed class CustomSearchPresetFilter
{
    /// <summary>Name of the filter in <see cref="Shared.Enum.StatPanel"/> (e.g. "MapTier", "CommonItemLevel").</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Min { get; set; } = null;

    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Max { get; set; } = null;
}
