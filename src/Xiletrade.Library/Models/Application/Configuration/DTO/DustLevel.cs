using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class DustLevel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("baseType")]
    public string BaseType { get; set; } = null;

    [JsonPropertyName("dustVal")]
    public double DustVal { get; set; }
}
