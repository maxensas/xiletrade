using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

[DataContract]
public sealed class Trade
{
    [DataMember(Name = "disabled")]
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = true;

    [DataMember(Name = "filters")]
    [JsonPropertyName("filters")]
    public TradeFilters Filters { get; set; } = new TradeFilters();
}
