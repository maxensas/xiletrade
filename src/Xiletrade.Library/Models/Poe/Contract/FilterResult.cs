using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class FilterResult
{
    [DataMember(Name = "label")]
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [DataMember(Name = "entries")]
    [JsonPropertyName("entries")]
    public FilterResultEntrie[] Entries { get; set; } = null;
}
