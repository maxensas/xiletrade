using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class TradeTwo
{
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; } = true;

    [JsonPropertyName("filters")]
    public TradeFiltersTwo Filters { get; set; } = new TradeFiltersTwo();
}
