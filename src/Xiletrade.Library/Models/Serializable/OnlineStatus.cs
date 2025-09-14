using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class OnlineStatus
{
    [DataMember(Name = "league")]
    [JsonPropertyName("league")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string League { get; set; } 

    [DataMember(Name = "status")]
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Status { get; set; } // null, offline, online or afk
}
