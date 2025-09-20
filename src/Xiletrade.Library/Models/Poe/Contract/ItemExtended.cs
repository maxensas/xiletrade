using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ItemExtended
{
    [JsonPropertyName("dps")]
    public decimal Dps { get; set; }

    [JsonPropertyName("pdps")]
    public decimal Pdps { get; set; }

    [JsonPropertyName("edps")]
    public decimal Edps { get; set; }

    [JsonPropertyName("ev")]
    public int EvaMaxQuality { get; set; }

    [JsonPropertyName("ev_aug")]
    public bool EvaMaxDisplay { get; set; }

    [JsonPropertyName("es")]
    public int EsMaxQuality { get; set; }

    [JsonPropertyName("es_aug")]
    public bool EsMaxDisplay { get; set; }

    [JsonPropertyName("ar")]
    public int ArMaxQuality { get; set; }

    [JsonPropertyName("ar_aug")]
    public bool ArMaxDisplay { get; set; }

    [JsonPropertyName("mods")]
    public ExtendedMod Mods { get; set; }

    [JsonPropertyName("hashes")]
    public ExtendedHashes Hashes { get; set; }
}
