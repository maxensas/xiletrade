using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ConfigMods
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [JsonPropertyName("text")]
    public string Text { get; set; } = null;
}
