using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class TypeTwo
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = false;

    [JsonPropertyName("filters")]
    public TypeFiltersTwo Filters { get; set; } = new TypeFiltersTwo();
}
