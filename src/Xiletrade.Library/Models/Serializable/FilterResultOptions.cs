using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class FilterResultOptions
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public object ID { get; set; } = 0;

    [DataMember(Name = "text")]
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
