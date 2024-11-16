using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract(Name = "checked")]
public sealed class ConfigChecked
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [DataMember(Name = "text")]
    [JsonPropertyName("text")]
    public string Text { get; set; } = null;

    [DataMember(Name = "mod_type")]
    [JsonPropertyName("mod_type")]
    public string ModType { get; set; } = null;
}
