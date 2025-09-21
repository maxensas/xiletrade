using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Two;

public sealed class NinjaCurrencyReference
{
    [JsonPropertyName("exalted")]
    public double Exalted { get; set; }

    [JsonPropertyName("chaos")]
    public double Chaos { get; set; }

    [JsonPropertyName("divine")]
    public double Divine { get; set; }
}
