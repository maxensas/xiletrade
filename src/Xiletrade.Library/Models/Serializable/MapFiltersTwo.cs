using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class MapFiltersTwo
{
    [JsonPropertyName("map_tier")]
    public MinMax Tier { get; set; } = new MinMax();

    [JsonPropertyName("map_bonus")]
    public MinMax Bonus { get; set; } = new MinMax();
}
