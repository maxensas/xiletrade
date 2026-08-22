using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class Heist
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("filters")]
    public HeistFilters Filters { get; set; } = new();
}
