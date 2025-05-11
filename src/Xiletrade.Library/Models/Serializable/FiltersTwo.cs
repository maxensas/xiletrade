using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

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
    public MiscTwo Misc { get; set; } = new MiscTwo();

    [JsonPropertyName("type_filters")]
    public TypeTwo Type { get; set; } = new TypeTwo();

    [JsonPropertyName("equipment_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Equipment Equipment { get; set; }

    [JsonPropertyName("trade_filters")]
    public TradeTwo Trade { get; set; } = new TradeTwo();
}
