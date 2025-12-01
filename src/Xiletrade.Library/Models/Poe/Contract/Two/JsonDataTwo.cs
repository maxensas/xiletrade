using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.Two;

public sealed class JsonDataTwo
{
    [JsonPropertyName("query")]
    public QueryTwo Query { get; set; } = new();

    [DataMember(Name = "sort")]
    [JsonPropertyName("sort")]
    public Sort Sort { get; set; } = new();
}
