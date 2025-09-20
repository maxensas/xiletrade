using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class PriceData
{
    [DataMember(Name = "type")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [DataMember(Name = "amount")]
    [JsonPropertyName("amount")]
    public double Amount { get; set; } = 0;

    [DataMember(Name = "currency")]
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}
