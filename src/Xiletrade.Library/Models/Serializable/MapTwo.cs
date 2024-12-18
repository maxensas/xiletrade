using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class MapTwo
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = true;

    [JsonPropertyName("filters")]
    public MapFiltersTwo Filters { get; set; } = new MapFiltersTwo();
}
