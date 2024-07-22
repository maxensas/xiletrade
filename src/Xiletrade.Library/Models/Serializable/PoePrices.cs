using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class PoePrices
{
    [DataMember(Name = "min")]
    public double Min { get; set; } = 0;

    [DataMember(Name = "max")]
    public double Max { get; set; } = 0;

    [DataMember(Name = "currency")]
    public string Currency { get; set; } = string.Empty;

    [DataMember(Name = "pred_explanation")]
    public object[][] PredExplantion { get; set; } = null;

    [DataMember(Name = "pred_confidence_score")]
    public object PredConfidenceScore { get; set; } = null;

    [DataMember(Name = "error")]
    public int Error { get; set; } = 0;

    [DataMember(Name = "error_msg")]
    public string ErrorMsg { get; set; } = string.Empty;
}
