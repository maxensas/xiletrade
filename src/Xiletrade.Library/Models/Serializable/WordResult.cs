using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WordResult
{
    [DataMember(Name = "data")]
    [JsonPropertyName("data")]
    public WordResultData[] Data { get; set; } = null;
}
