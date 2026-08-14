using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class RequirementFilters
{
    [JsonPropertyName("dex")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Dexterity { get; set; }

    [JsonPropertyName("int")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Intelligence { get; set; }

    [JsonPropertyName("lvl")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Level { get; set; }

    [JsonPropertyName("str")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Strength { get; set; }
}
