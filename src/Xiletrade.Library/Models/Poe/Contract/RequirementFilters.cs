using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class RequirementFilters
{
    [JsonPropertyName("dex")]
    public MinMax Dexterity { get; set; } = new MinMax();

    [JsonPropertyName("int")]
    public MinMax Intelligence { get; set; } = new MinMax();

    [JsonPropertyName("lvl")]
    public MinMax Level { get; set; } = new MinMax();

    [JsonPropertyName("str")]
    public MinMax Strength { get; set; } = new MinMax();
}
