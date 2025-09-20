using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class ResultData
{
    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public string[] Result { get; set; } = null;

    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [DataMember(Name = "total")]
    [JsonPropertyName("total")]
    public int Total { get; set; } = 0;
}
