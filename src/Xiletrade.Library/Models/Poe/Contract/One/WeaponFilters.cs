using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class WeaponFilters
{
    [JsonPropertyName("dps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Damage { get; set; }

    [JsonPropertyName("pdps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Pdps { get; set; }

    [JsonPropertyName("edps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Edps { get; set; }

    [JsonPropertyName("damage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax BaseDamage { get; set; }

    [JsonPropertyName("aps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax AttacksPerSecond { get; set; }

    [JsonPropertyName("crit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax CriticalChance { get; set; }
}
