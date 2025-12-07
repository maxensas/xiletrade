using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class OfferInfo
{
    [JsonPropertyName("exchange")]
    public ExchangeInfo Exchange { get; set; } = new();

    [JsonPropertyName("item")]
    public ItemInfo Item { get; set; } = new();
}
