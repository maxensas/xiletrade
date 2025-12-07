using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Prices.Contract;

public sealed class PoePrices
{
    [JsonPropertyName("min")]
    public double Min { get; set; } = 0;

    [JsonPropertyName("max")]
    public double Max { get; set; } = 0;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("pred_explanation")]
    public object[][] PredExplantion { get; set; } = null;

    [JsonPropertyName("pred_confidence_score")]
    public ConfidenceScore PredConfidenceScore { get; set; } = null;

    [JsonPropertyName("error")]
    public int Error { get; set; } = 0;

    [JsonPropertyName("error_msg")]
    public string ErrorMsg { get; set; } = string.Empty;
}
