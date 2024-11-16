using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class GemTransfigured
{
    [DataMember(Name = "option")]
    [JsonPropertyName("option")]
    public string Option { get; set; } = null;

    [DataMember(Name = "discriminator")]
    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; } = null;
}
