using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class MapFilters
{
    [JsonPropertyName("map_tier")]
    public MinMax Tier { get; set; } = new MinMax();

    [JsonPropertyName("area_level")]
    public MinMax Area { get; set; } = new MinMax();

    [JsonPropertyName("map_iiq")]
    public MinMax Iiq { get; set; } = new MinMax();

    [JsonPropertyName("map_iir")]
    public MinMax Iir { get; set; } = new MinMax();

    [JsonPropertyName("map_packsize")]
    public MinMax PackSize { get; set; } = new MinMax();

    [JsonPropertyName("map_shaped")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Shaper { get; set; }

    [JsonPropertyName("map_elder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Elder { get; set; }

    [JsonPropertyName("map_blighted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Blight { get; set; }

    [JsonPropertyName("map_uberblighted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt BlightRavaged { get; set; }

    [JsonPropertyName("map_completion_reward")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt MapReward { get; set; }

    [JsonPropertyName("scourge_tier")]
    public MinMax ScourgeTier { get; set; } = new MinMax();
}
