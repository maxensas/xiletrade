using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class PriceData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public double Amount { get; set; } = 0;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}
