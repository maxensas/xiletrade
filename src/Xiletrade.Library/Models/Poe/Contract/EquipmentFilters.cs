using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class EquipmentFilters
{
    [JsonPropertyName("ar")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Armour { get; set; }

    [JsonPropertyName("es")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax EnergyShield { get; set; }

    [JsonPropertyName("ev")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Evasion { get; set; }

    [JsonPropertyName("ward")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax RunicWard { get; set; }

    [JsonPropertyName("aps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax AttacksPerSecond { get; set; }

    [JsonPropertyName("dps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax DamagePerSecond { get; set; }

    [JsonPropertyName("crit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax CriticalChance { get; set; }

    [JsonPropertyName("edps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax ElementalDps { get; set; }

    [JsonPropertyName("pdps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax PhysicalDps { get; set; }

    [JsonPropertyName("block")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Block { get; set; }

    [JsonPropertyName("damage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Damage { get; set; }

    [JsonPropertyName("spirit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Spirit { get; set; }

    [JsonPropertyName("rune_sockets")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax RuneSockets { get; set; }
}
