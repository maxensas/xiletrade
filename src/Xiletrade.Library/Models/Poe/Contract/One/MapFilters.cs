using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class MapFilters
{
    [JsonPropertyName("map_tier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Tier { get; set; }

    [JsonPropertyName("area_level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Area { get; set; }

    [JsonPropertyName("map_iiq")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Iiq { get; set; }

    [JsonPropertyName("map_iir")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Iir { get; set; }

    [JsonPropertyName("map_packsize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax PackSize { get; set; }

    [JsonPropertyName("map_gold")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Gold { get; set; }

    [JsonPropertyName("chart_sulphur")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax ChartSulphur { get; set; }

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
}
