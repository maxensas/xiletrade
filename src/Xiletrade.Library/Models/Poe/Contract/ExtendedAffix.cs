using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ExtendedAffix
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("tier")]
    public string Tier { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("magnitudes")]
    public List<ExtendedMagnitudes> Magnitudes { get; set; }
}
