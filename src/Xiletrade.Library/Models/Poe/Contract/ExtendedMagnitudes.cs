using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ExtendedMagnitudes
{
    [JsonPropertyName("hash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Hash { get; set; }

    [JsonPropertyName("min")]
    public decimal? Min { get; set; }

    [JsonPropertyName("max")]
    public decimal? Max { get; set; }
}
