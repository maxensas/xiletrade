using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ChatCommands
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;
}
