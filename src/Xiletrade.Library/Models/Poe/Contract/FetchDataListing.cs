using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class FetchDataListing
{
    [JsonPropertyName("indexed")]
    public string Indexed { get; set; } = string.Empty;

    [JsonPropertyName("account")]
    public AccountData Account { get; set; } = new();

    [JsonPropertyName("price")]
    public PriceData Price { get; set; } = new();

    [JsonPropertyName("offers")]
    public OfferInfo[] Offers { get; set; }

    [JsonPropertyName("whisper")]
    public string Whisper { get; set; } = string.Empty; // was object
}
