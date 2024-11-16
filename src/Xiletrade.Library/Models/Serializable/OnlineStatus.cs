using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class OnlineStatus
{
    [DataMember(Name = "league")]
    [JsonPropertyName("league")]
    public string League { get; set; } = string.Empty;

    [DataMember(Name = "status")]
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // null or afk
}
