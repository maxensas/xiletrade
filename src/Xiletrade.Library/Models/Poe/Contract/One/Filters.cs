using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class Filters
{
    [JsonPropertyName("type_filters")]
    public TypeF Type { get; set; } = new TypeF();

    [JsonPropertyName("socket_filters")]
    public Socket Socket { get; set; } = new Socket();

    [JsonPropertyName("map_filters")]
    public Map Map { get; set; } = new Map();

    [JsonPropertyName("misc_filters")]
    public Misc Misc { get; set; } = new Misc();

    [JsonPropertyName("trade_filters")]
    public Trade Trade { get; set; } = new Trade();

    [JsonPropertyName("weapon_filters")]
    public Weapon Weapon { get; set; } = new Weapon();

    [JsonPropertyName("armour_filters")]
    public Armour Armour { get; set; } = new Armour();

    [JsonPropertyName("ultimatum_filters")]
    public Ultimatum Ultimatum { get; set; } = new Ultimatum();

    [JsonPropertyName("sanctum_filters")]
    public Sanctum Sanctum { get; set; } = new Sanctum();

    [JsonPropertyName("req_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Requirement Requirement { get; set; } = new Requirement();
}
