using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class MiscFilters
{
    [JsonPropertyName("quality")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Quality { get; set; }

    [JsonPropertyName("ilvl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Ilvl { get; set; }

    [JsonPropertyName("gem_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Gem_level { get; set; }

    [JsonPropertyName("memory_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax MemoryStrand { get; set; }

    [JsonPropertyName("intangibility")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Intangibility { get; set; }

    [JsonPropertyName("stored_experience")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax StoredExp { get; set; }

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

    [JsonPropertyName("crafted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Crafted { get; set; }

    [JsonPropertyName("mutated")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Mutated { get; set; }
}
