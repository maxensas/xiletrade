using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class MapFilters
{
    [DataMember(Name = "map_tier")]
    public MinMax Tier { get; set; } = new MinMax();

    [DataMember(Name = "area_level")]
    public MinMax Area { get; set; } = new MinMax();

    [DataMember(Name = "map_iiq")]
    public MinMax Iiq { get; set; } = new MinMax();

    [DataMember(Name = "map_iir")]
    public MinMax Iir { get; set; } = new MinMax();

    [DataMember(Name = "map_packsize")]
    public MinMax PackSize { get; set; } = new MinMax();

    [DataMember(Name = "map_shaped", EmitDefaultValue = false)]
    public Options Shaper { get; set; }

    [DataMember(Name = "map_elder", EmitDefaultValue = false)]
    public Options Elder { get; set; }

    [DataMember(Name = "map_blighted", EmitDefaultValue = false)]
    public Options Blight { get; set; }

    [DataMember(Name = "map_uberblighted", EmitDefaultValue = false)]
    public Options BlightRavaged { get; set; }

    [DataMember(Name = "map_completion_reward", EmitDefaultValue = false)]
    public Options MapReward { get; set; }

    [DataMember(Name = "scourge_tier")]
    public MinMax ScourgeTier { get; set; } = new MinMax();
}
