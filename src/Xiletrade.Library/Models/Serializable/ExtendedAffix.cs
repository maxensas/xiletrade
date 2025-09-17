using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

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

    [JsonIgnore]
    public string Mod { get; set; }

    [JsonIgnore]
    public bool TagLife { get; set; }

    [JsonIgnore]
    public bool TagFire { get; set; }

    [JsonIgnore]
    public bool TagCold { get; set; }

    [JsonIgnore]
    public bool TagLightning { get; set; }

    [JsonIgnore]
    public bool TagDesecrated { get; set; }
}
