using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class DustLevel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("baseType")]
    public string BaseType { get; set; } = null;

    [JsonPropertyName("dustVal")]
    public double DustVal { get; set; }
}
