using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Serializable.SourceGeneration;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class PoePrices
{
    [DataMember(Name = "min")]
    [JsonPropertyName("min")]
    public double Min { get; set; } = 0;

    [DataMember(Name = "max")]
    [JsonPropertyName("max")]
    public double Max { get; set; } = 0;

    [DataMember(Name = "currency")]
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [DataMember(Name = "pred_explanation")]
    [JsonPropertyName("pred_explanation")]
    [JsonConverter(typeof(ArrayStringJsonConverter))]
    public object[][] PredExplantion { get; set; } = null;

    [DataMember(Name = "pred_confidence_score")]
    [JsonPropertyName("pred_confidence_score")]
    [JsonConverter(typeof(DoubleJsonConverter))]
    public object PredConfidenceScore { get; set; } = null;

    [DataMember(Name = "error")]
    [JsonPropertyName("error")]
    public int Error { get; set; } = 0;

    [DataMember(Name = "error_msg")]
    [JsonPropertyName("error_msg")]
    public string ErrorMsg { get; set; } = string.Empty;
}
