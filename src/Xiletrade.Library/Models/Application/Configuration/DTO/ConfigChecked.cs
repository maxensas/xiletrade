using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ConfigChecked
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [JsonPropertyName("text")]
    public string Text { get; set; } = null;

    [JsonPropertyName("mod_type")]
    public string ModType { get; set; } = null;
}
