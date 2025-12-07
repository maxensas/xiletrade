using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class Ultimatum
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = true;

    [JsonPropertyName("filters")]
    public UltimatumFilters Filters { get; set; } = new UltimatumFilters();
}
