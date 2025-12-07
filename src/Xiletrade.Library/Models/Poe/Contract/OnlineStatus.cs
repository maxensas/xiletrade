using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class OnlineStatus
{
    [JsonPropertyName("league")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string League { get; set; } 

    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Status { get; set; } // null, offline, online or afk
}
