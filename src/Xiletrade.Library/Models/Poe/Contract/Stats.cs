using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class Stats
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("filters")]
    public StatsFilters[] Filters { get; set; }

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Value { get; set; }

    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
}
