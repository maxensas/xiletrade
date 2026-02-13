using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ModResultData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null;

    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = null;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("level")]
    public string Level { get; set; } = null;

    // TO REMOVE LATER, avoid breaking <= 1.14.9 updates
    [JsonPropertyName("inherits_from")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    public string Inherits { get; set; } = string.Empty;
}
