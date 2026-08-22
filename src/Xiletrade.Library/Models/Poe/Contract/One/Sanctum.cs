using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class Sanctum
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("filters")]
    public SanctumFilters Filters { get; set; } = new();
}
