using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

[DataContract]
public sealed class BaseData
{
    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public BaseResult[] Result { get; set; } = null;
}
