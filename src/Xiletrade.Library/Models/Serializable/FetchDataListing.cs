using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class FetchDataListing
{
    [DataMember(Name = "indexed")]
    public string Indexed { get; set; } = string.Empty;

    [DataMember(Name = "account")]
    public AccountData Account { get; set; } = new();

    [DataMember(Name = "price")]
    public PriceData Price { get; set; } = new();

    [DataMember(Name = "offers")]
    public OfferInfo[] Offers { get; set; }

    [DataMember(Name = "whisper")]
    public object Whisper { get; set; } = string.Empty;
}
