using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.Two;

public sealed class TradeFiltersTwo
{
    [JsonPropertyName("price")]
    public MinMax Price { get; set; } = new MinMax();

    [JsonPropertyName("indexed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Indexed { get; set; }

    [JsonPropertyName("collapse")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Collapse { get; set; }

    [JsonPropertyName("sale_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt SaleType { get; set; }
}
