using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class AccountData
{
    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "lastCharacterName")]
    [JsonPropertyName("lastCharacterName")]
    public string LastCharacterName { get; set; } = string.Empty;

    [DataMember(Name = "language")]
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [DataMember(Name = "online")]
    [JsonPropertyName("online")]
    public OnlineStatus Online { get; set; } = null;
}
