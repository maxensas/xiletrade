using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class Unique
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = null;

    [JsonPropertyName("class")]
    public string Class { get; set; } = null;

    [JsonPropertyName("class_en")]
    public string ClassEn { get; set; } = null;

    [JsonPropertyName("base")]
    public string Base { get; set; } = null;

    [JsonPropertyName("base_en")]
    public string BaseEn { get; set; } = null;
}
