using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class WeaponFilters
{
    [JsonPropertyName("dps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Damage { get; set; } = new MinMax();

    [JsonPropertyName("pdps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Pdps { get; set; } = new MinMax();

    [JsonPropertyName("edps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Edps { get; set; } = new MinMax();
}
