using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class OfferInfo
{
    [DataMember(Name = "exchange")]
    [JsonPropertyName("exchange")]
    public ExchangeInfo Exchange { get; set; } = new();

    [DataMember(Name = "item")]
    [JsonPropertyName("item")]
    public ItemInfo Item { get; set; } = new();
}
