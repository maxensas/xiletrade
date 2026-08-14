using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.Two;

public sealed class MiscFiltersTwo
{
    [JsonPropertyName("gem_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax GemLevel { get; set; }

    [JsonPropertyName("area_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax AreaLevel { get; set; }

    [JsonPropertyName("stack_size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax StackSize { get; set; }

    [JsonPropertyName("gem_sockets")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax GemSockets { get; set; }

    [JsonPropertyName("sanctum_gold")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax BaryaSacredWater { get; set; }

    [JsonPropertyName("unidentified_tier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax UnidentifiedTier { get; set; }

    [JsonPropertyName("mirrored")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Mirrored { get; set; }

    [JsonPropertyName("corrupted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Corrupted { get; set; }

    [JsonPropertyName("twice_corrupted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt TwiceCorrupted { get; set; }

    [JsonPropertyName("identified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Identified { get; set; }

    [JsonPropertyName("fractured_item")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Fractured { get; set; }

    [JsonPropertyName("alternate_art")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt AlternateArt { get; set; }

    [JsonPropertyName("crafted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Crafted { get; set; }

    [JsonPropertyName("mutated")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Mutated { get; set; }

    [JsonPropertyName("desecrated")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Desecrated { get; set; }

    [JsonPropertyName("veiled")] // Unrevealed
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Veiled { get; set; }

    [JsonPropertyName("sanctified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Sanctified { get; set; }
}
