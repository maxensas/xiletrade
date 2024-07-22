using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class PriceData
{
    [DataMember(Name = "type")]
    public string Type { get; set; } = string.Empty;

    [DataMember(Name = "amount")]
    public double Amount { get; set; } = 0;

    [DataMember(Name = "currency")]
    public string Currency { get; set; } = string.Empty;
}
