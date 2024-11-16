using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class CurrencyEntrie
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [DataMember(Name = "text")]
    [JsonPropertyName("text")]
    public string Text { get; set; } = null;

    [DataMember(Name = "image", EmitDefaultValue = false)]
    [JsonPropertyName("image")] //[DefaultValue(null)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Img { get; set; } = null;
}
