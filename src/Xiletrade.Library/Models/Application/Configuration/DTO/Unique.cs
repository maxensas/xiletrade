using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class Unique
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = null;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null;

    [JsonPropertyName("type_en")]
    public string TypeEn { get; set; } = null;
}
