using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WordData
{
    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public WordResult[] Result { get; set; } = null;
}
