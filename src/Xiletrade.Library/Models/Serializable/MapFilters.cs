using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class MapFilters
{
    [DataMember(Name = "map_tier")]
    [JsonPropertyName("map_tier")]
    public MinMax Tier { get; set; } = new MinMax();

    [DataMember(Name = "area_level")]
    [JsonPropertyName("area_level")]
    public MinMax Area { get; set; } = new MinMax();

    [DataMember(Name = "map_iiq")]
    [JsonPropertyName("map_iiq")]
    public MinMax Iiq { get; set; } = new MinMax();

    [DataMember(Name = "map_iir")]
    [JsonPropertyName("map_iir")]
    public MinMax Iir { get; set; } = new MinMax();

    [DataMember(Name = "map_packsize")]
    [JsonPropertyName("map_packsize")]
    public MinMax PackSize { get; set; } = new MinMax();

    [DataMember(Name = "map_shaped", EmitDefaultValue = false)]
    [JsonPropertyName("map_shaped")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Options Shaper { get; set; }

    [DataMember(Name = "map_elder", EmitDefaultValue = false)]
    [JsonPropertyName("map_elder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Options Elder { get; set; }

    [DataMember(Name = "map_blighted", EmitDefaultValue = false)]
    [JsonPropertyName("map_blighted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Options Blight { get; set; }

    [DataMember(Name = "map_uberblighted", EmitDefaultValue = false)]
    [JsonPropertyName("map_uberblighted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Options BlightRavaged { get; set; }

    [DataMember(Name = "map_completion_reward", EmitDefaultValue = false)]
    [JsonPropertyName("map_completion_reward")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Options MapReward { get; set; }

    [DataMember(Name = "scourge_tier")]
    [JsonPropertyName("scourge_tier")]
    public MinMax ScourgeTier { get; set; } = new MinMax();
}
