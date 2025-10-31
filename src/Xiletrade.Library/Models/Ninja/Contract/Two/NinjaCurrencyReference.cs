using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Two;

public sealed class NinjaCurrencyReference
{
    [JsonPropertyName("exalted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Exalted { get; set; }

    [JsonPropertyName("chaos")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Chaos { get; set; }

    [JsonPropertyName("divine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Divine { get; set; }
}
