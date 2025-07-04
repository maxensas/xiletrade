using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class NinjaLeagues
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("hardcore")]
    public bool Hardcore { get; set; }

    [JsonPropertyName("indexed")]
    public bool Indexed { get; set; }
}
