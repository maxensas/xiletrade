using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

[DataContract]
public sealed class TypeF
{
    [DataMember(Name = "filters")]
    [JsonPropertyName("filters")]
    public TypeFilters Filters { get; set; } = new TypeFilters();
}
