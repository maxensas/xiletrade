using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ExtendedMagnitudes
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("min")]
    public FlexibleString Min { get; set; }

    [JsonPropertyName("max")]
    public FlexibleString Max { get; set; }
}
