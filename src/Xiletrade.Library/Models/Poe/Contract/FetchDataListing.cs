using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class FetchDataListing
{
    [DataMember(Name = "indexed")]
    [JsonPropertyName("indexed")]
    public string Indexed { get; set; } = string.Empty;

    [DataMember(Name = "account")]
    [JsonPropertyName("account")]
    public AccountData Account { get; set; } = new();

    [DataMember(Name = "price")]
    [JsonPropertyName("price")]
    public PriceData Price { get; set; } = new();

    [DataMember(Name = "offers")]
    [JsonPropertyName("offers")]
    public OfferInfo[] Offers { get; set; }

    [DataMember(Name = "whisper")]
    [JsonPropertyName("whisper")]
    public string Whisper { get; set; } = string.Empty; // was object
}
