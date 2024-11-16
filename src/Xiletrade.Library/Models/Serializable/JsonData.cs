using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class JsonData
{
    [DataMember(Name = "query")]
    [JsonPropertyName("query")]
    public Query Query { get; set; } = new();

    [DataMember(Name = "sort")]
    [JsonPropertyName("sort")]
    public Sort Sort { get; set; } = new();
}
