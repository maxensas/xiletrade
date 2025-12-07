using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class GemResultData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("name_en")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string NameEn { get; set; } = null;

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Type { get; set; } = null;

    [JsonPropertyName("type_en")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string TypeEn { get; set; } = null;

    [JsonPropertyName("disc")]
    public string Disc { get; set; } = null;
}
