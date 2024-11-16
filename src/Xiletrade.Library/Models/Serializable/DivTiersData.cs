using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class DivTiersData
{
    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public DivTiersResult[] Result { get; set; } = null;
}
