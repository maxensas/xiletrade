using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.Two;

public sealed class TypeTwo
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("filters")]
    public TypeFiltersTwo Filters { get; set; } = new();
}
