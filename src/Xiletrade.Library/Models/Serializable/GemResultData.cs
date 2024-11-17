using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class GemResultData
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "nameEn")]
    [JsonPropertyName("nameEn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string NameEn { get; set; } = null;

    [DataMember(Name = "type")]
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Type { get; set; } = null;

    [DataMember(Name = "typeEn")]
    [JsonPropertyName("typeEn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string TypeEn { get; set; } = null;

    [DataMember(Name = "disc")]
    [JsonPropertyName("disc")]
    public string Disc { get; set; } = null;
}
