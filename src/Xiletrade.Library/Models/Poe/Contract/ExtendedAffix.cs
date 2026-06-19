using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ExtendedAffix
{
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; }

    [JsonPropertyName("tier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Tier { get; set; }

    [JsonPropertyName("level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Level { get; set; }

    [JsonPropertyName("magnitudes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ExtendedMagnitudes> Magnitudes { get; set; }
}
