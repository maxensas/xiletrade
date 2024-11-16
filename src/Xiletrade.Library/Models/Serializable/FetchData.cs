using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class FetchData
{
    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public FetchDataInfo[] Result { get; set; } = new FetchDataInfo[5];
}
