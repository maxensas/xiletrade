using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class OfferInfo
{
    [DataMember(Name = "exchange")]
    public ExchangeInfo Exchange { get; set; } = new();

    [DataMember(Name = "item")]
    public ItemInfo Item { get; set; } = new();
}
