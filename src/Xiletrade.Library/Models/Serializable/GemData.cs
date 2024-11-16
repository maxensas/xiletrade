using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class GemData
{
    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public GemResult[] Result { get; set; } = null;
}
