using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Trade
{
    [DataMember(Name = "disabled")]
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = false;

    [DataMember(Name = "filters")]
    [JsonPropertyName("filters")]
    public TradeFilters Filters { get; set; } = new TradeFilters();
}
