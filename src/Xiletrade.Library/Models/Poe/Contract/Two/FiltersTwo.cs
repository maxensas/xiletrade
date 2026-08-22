using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.Two;

public sealed class FiltersTwo
{
    [JsonPropertyName("map_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MapTwo Map { get; set; }

    [JsonPropertyName("req_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Requirement Requirement { get; set; }

    [JsonPropertyName("misc_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MiscTwo Misc { get; set; }

    [JsonPropertyName("type_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TypeTwo Type { get; set; }

    [JsonPropertyName("equipment_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Equipment Equipment { get; set; }

    [JsonPropertyName("trade_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TradeTwo Trade { get; set; }
}
