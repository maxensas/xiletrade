using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class BaseResult
{
    [DataMember(Name = "data")]
    [JsonPropertyName("data")]
    public BaseResultData[] Data { get; set; } = null;
}
