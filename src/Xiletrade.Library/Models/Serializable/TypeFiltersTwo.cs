using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class TypeFiltersTwo
{
    [JsonPropertyName("ilvl")]
    public MinMax ItemLevel { get; set; } = new MinMax();

    [JsonPropertyName("rarity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Rarity { get; set; }

    [JsonPropertyName("quality")]
    public MinMax Quality { get; set; } = new MinMax();

    [JsonPropertyName("category")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Category { get; set; }
}
