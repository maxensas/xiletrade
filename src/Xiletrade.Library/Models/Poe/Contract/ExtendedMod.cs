using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ExtendedMod
{
    [JsonPropertyName("explicit")]
    public List<ExtendedAffix> Explicit { get; set; }

    [JsonPropertyName("implicit")]
    public List<ExtendedAffix> Implicit { get; set; }

    [JsonPropertyName("desecrated")]
    public List<ExtendedAffix> Desecrated { get; set; }

    [JsonPropertyName("fractured")]
    public List<ExtendedAffix> Fractured { get; set; }
}
