using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class MapFiltersTwo
{
    [JsonPropertyName("map_tier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Tier { get; set; }

    [JsonPropertyName("map_iiq")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Quantity { get; set; }

    [JsonPropertyName("map_iir")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Rarity { get; set; }

    [JsonPropertyName("map_packsize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax PackSize { get; set; }

    [JsonPropertyName("map_rare_monsters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax RareMonsters { get; set; }

    [JsonPropertyName("map_magic_monsters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax MagicMonsters { get; set; }

    //not used yet
    [JsonPropertyName("map_bonus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Bonus { get; set; }

    [JsonPropertyName("map_gold")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Gold { get; set; }

    [JsonPropertyName("map_revives")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Revives { get; set; }

    [JsonPropertyName("map_experience")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Experience { get; set; }

    [JsonPropertyName("ultimatum_hint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Ultimatum { get; set; } // Victorious, Cowardly, Deadly
}
