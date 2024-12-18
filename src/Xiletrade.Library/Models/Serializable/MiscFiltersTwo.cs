using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class MiscFiltersTwo
{
    [JsonPropertyName("gem_level")]
    public MinMax GemLevel { get; set; } = new MinMax();

    [JsonPropertyName("area_level")]
    public MinMax AreaLevel { get; set; } = new MinMax();

    [JsonPropertyName("stack_size")]
    public MinMax StackSize { get; set; } = new MinMax();

    [JsonPropertyName("gem_sockets")]
    public MinMax GemSockets { get; set; } = new MinMax();

    [JsonPropertyName("sanctum_gold")]
    public MinMax BaryaSacredWater { get; set; } = new MinMax();

    [JsonPropertyName("unidentified_tier")]
    public MinMax UnidentifiedTier { get; set; } = new MinMax();

    [JsonPropertyName("mirrored")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Mirrored { get; set; }

    [JsonPropertyName("corrupted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Corrupted { get; set; }

    [JsonPropertyName("identified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Identified { get; set; }

    [JsonPropertyName("alternate_art")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt AlternateArt { get; set; }
}
