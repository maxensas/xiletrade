using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract()]
public sealed class ChatCommands
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [DataMember(Name = "command")]
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;
}
