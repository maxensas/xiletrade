using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class FilterResultEntrie
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;

    [DataMember(Name = "text")]
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [DataMember(Name = "type")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [DataMember(Name = "part", EmitDefaultValue = false)]
    [JsonPropertyName("part")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Part { get; set; } = string.Empty;

    [DataMember(Name = "option", EmitDefaultValue = false)]
    [JsonPropertyName("option")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FilterResultOption Option { get; set; } = new FilterResultOption();
}
