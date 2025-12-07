using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class MiscFilters
{
    [JsonPropertyName("quality")]
    public MinMax Quality { get; set; } = new MinMax();

    [JsonPropertyName("ilvl")]
    public MinMax Ilvl { get; set; } = new MinMax();

    [JsonPropertyName("gem_level")]
    public MinMax Gem_level { get; set; } = new MinMax();

    [JsonPropertyName("memory_level")]
    public MinMax MemoryStrand { get; set; } = new MinMax();

    [JsonPropertyName("stored_experience")]
    public MinMax StoredExp { get; set; } = new MinMax();

    [JsonPropertyName("gem_alternate_quality")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Gem_alternate { get; set; }

    [JsonPropertyName("corrupted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Corrupted { get; set; }

    [JsonPropertyName("synthesised_item")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Synthesis { get; set; }

    [JsonPropertyName("split")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Split { get; set; }

    [JsonPropertyName("mirrored")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Mirrored { get; set; }

    [JsonPropertyName("identified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Identified { get; set; }

    [JsonPropertyName("fractured_item")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Fractured { get; set; }
}
