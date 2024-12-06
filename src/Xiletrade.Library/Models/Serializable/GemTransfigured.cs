using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class GemTransfigured(string option, string discriminator)
{
    [DataMember(Name = "option")]
    [JsonPropertyName("option")]
    public string Option { get; set; } = option;

    [DataMember(Name = "discriminator")]
    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; } = discriminator;
}
