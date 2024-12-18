using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class FiltersTwo
{
    [JsonPropertyName("map_filters")]
    public MapTwo Map { get; set; } = new MapTwo();

    [JsonPropertyName("req_filters")]
    public Requirement Requirement { get; set; } = new Requirement();

    [JsonPropertyName("misc_filters")]
    public MiscTwo Misc { get; set; } = new MiscTwo();

    [JsonPropertyName("type_filters")]
    public TypeTwo Type { get; set; } = new TypeTwo();

    [JsonPropertyName("equipment_filters")]
    public Equipment Equipment { get; set; } = new Equipment();

    [JsonPropertyName("trade_filters")]
    public TradeTwo Trade { get; set; } = new TradeTwo();
}
