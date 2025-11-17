using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

[DataContract]
public sealed class MiscFilters
{
    [DataMember(Name = "quality")]
    [JsonPropertyName("quality")]
    public MinMax Quality { get; set; } = new MinMax();

    [DataMember(Name = "ilvl")]
    [JsonPropertyName("ilvl")]
    public MinMax Ilvl { get; set; } = new MinMax();

    [DataMember(Name = "gem_level")]
    [JsonPropertyName("gem_level")]
    public MinMax Gem_level { get; set; } = new MinMax();

    [DataMember(Name = "memory_level")]
    [JsonPropertyName("memory_level")]
    public MinMax MemoryStrand { get; set; } = new MinMax();

    [DataMember(Name = "stored_experience")]
    [JsonPropertyName("stored_experience")]
    public MinMax StoredExp { get; set; } = new MinMax();

    [DataMember(Name = "gem_alternate_quality", EmitDefaultValue = false)]
    [JsonPropertyName("gem_alternate_quality")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Gem_alternate { get; set; }

    [DataMember(Name = "corrupted", EmitDefaultValue = false)]
    [JsonPropertyName("corrupted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Corrupted { get; set; }

    [DataMember(Name = "synthesised_item", EmitDefaultValue = false)]
    [JsonPropertyName("synthesised_item")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Synthesis { get; set; }

    [DataMember(Name = "split", EmitDefaultValue = false)]
    [JsonPropertyName("split")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Split { get; set; }

    [DataMember(Name = "mirrored", EmitDefaultValue = false)]
    [JsonPropertyName("mirrored")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Mirrored { get; set; }

    [DataMember(Name = "identified", EmitDefaultValue = false)]
    [JsonPropertyName("identified")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Identified { get; set; }
}
