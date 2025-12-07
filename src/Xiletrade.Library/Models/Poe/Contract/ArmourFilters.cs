using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ArmourFilters
{
    [JsonPropertyName("ar")]
    public MinMax Armour { get; set; } = new MinMax();

    [JsonPropertyName("es")]
    public MinMax Energy { get; set; } = new MinMax();

    [JsonPropertyName("ev")]
    public MinMax Evasion { get; set; } = new MinMax();

    [JsonPropertyName("ward")]
    public MinMax Ward { get; set; } = new MinMax();
}
