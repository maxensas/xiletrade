using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class TypeF
{
    [JsonPropertyName("filters")]
    public TypeFilters Filters { get; set; } = new TypeFilters();
}
