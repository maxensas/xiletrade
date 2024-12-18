using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class EquipmentFilters
{
    [JsonPropertyName("ar")]
    public MinMax Armour { get; set; } = new MinMax();

    [JsonPropertyName("es")]
    public MinMax EnergyShield { get; set; } = new MinMax();

    [JsonPropertyName("ev")]
    public MinMax Evasion { get; set; } = new MinMax();

    [JsonPropertyName("aps")]
    public MinMax AttacksPerSecond { get; set; } = new MinMax();

    [JsonPropertyName("dps")]
    public MinMax DamagePerSecond { get; set; } = new MinMax();

    [JsonPropertyName("crit")]
    public MinMax CriticalChance { get; set; } = new MinMax();

    [JsonPropertyName("edps")]
    public MinMax ElementalDps { get; set; } = new MinMax();

    [JsonPropertyName("pdps")]
    public MinMax PhysicalDps { get; set; } = new MinMax();

    [JsonPropertyName("block")]
    public MinMax Block { get; set; } = new MinMax();

    [JsonPropertyName("damage")]
    public MinMax Damage { get; set; } = new MinMax();

    [JsonPropertyName("spirit")]
    public MinMax Spirit { get; set; } = new MinMax();

    [JsonPropertyName("rune_sockets")]
    public MinMax EmptyRuneSockets { get; set; } = new MinMax();
}
