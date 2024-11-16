using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Filters
{
    [DataMember(Name = "type_filters")]
    [JsonPropertyName("type_filters")]
    public Type Type { get; set; } = new Type();

    [DataMember(Name = "socket_filters")]
    [JsonPropertyName("socket_filters")]
    public Socket Socket { get; set; } = new Socket();

    [DataMember(Name = "map_filters")]
    [JsonPropertyName("map_filters")]
    public Map Map { get; set; } = new Map();

    [DataMember(Name = "misc_filters")]
    [JsonPropertyName("misc_filters")]
    public Misc Misc { get; set; } = new Misc();

    [DataMember(Name = "trade_filters")]
    [JsonPropertyName("trade_filters")]
    public Trade Trade { get; set; } = new Trade();

    [DataMember(Name = "weapon_filters")]
    [JsonPropertyName("weapon_filters")]
    public Weapon Weapon { get; set; } = new Weapon();

    [DataMember(Name = "armour_filters")]
    [JsonPropertyName("armour_filters")]
    public Armour Armour { get; set; } = new Armour();

    [DataMember(Name = "ultimatum_filters")]
    [JsonPropertyName("ultimatum_filters")]
    public Ultimatum Ultimatum { get; set; } = new Ultimatum();

    [DataMember(Name = "sanctum_filters")]
    [JsonPropertyName("sanctum_filters")]
    public Sanctum Sanctum { get; set; } = new Sanctum();
}
