using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

[DataContract]
public sealed class GemResult
{
    [DataMember(Name = "data")]
    [JsonPropertyName("data")]
    public GemResultData[] Data { get; set; } = null;
}
