using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class Filters
{
    [JsonPropertyName("type_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TypeF Type { get; set; }

    [JsonPropertyName("socket_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Socket Socket { get; set; }

    [JsonPropertyName("map_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Map Map { get; set; }

    [JsonPropertyName("misc_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Misc Misc { get; set; }

    [JsonPropertyName("trade_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Trade Trade { get; set; }

    [JsonPropertyName("weapon_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Weapon Weapon { get; set; }

    [JsonPropertyName("armour_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Armour Armour { get; set; }

    [JsonPropertyName("ultimatum_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Ultimatum Ultimatum { get; set; }

    [JsonPropertyName("sanctum_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Sanctum Sanctum { get; set; }

    [JsonPropertyName("req_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Requirement Requirement { get; set; }

    [JsonPropertyName("heist_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Heist Heist { get; set; }
}
