using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Trade
{
    [DataMember(Name = "disabled")]
    public bool Disabled { get; set; } = false;

    [DataMember(Name = "filters")]
    public TradeFilters Filters { get; set; } = new TradeFilters();
}
