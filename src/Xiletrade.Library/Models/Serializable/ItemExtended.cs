using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class ItemExtended
{
    [JsonPropertyName("dps")]
    public decimal Dps { get; set; }

    [JsonPropertyName("pdps")]
    public decimal Pdps { get; set; }

    [JsonPropertyName("edps")]
    public decimal Edps { get; set; }

    [JsonPropertyName("mods")]
    public ExtendedMod Mods { get; set; }

    [JsonPropertyName("hashes")]
    public ExtendedHashes Hashes { get; set; }
}
