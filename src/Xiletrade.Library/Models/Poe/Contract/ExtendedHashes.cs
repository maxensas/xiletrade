using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ExtendedHashes
{
    [JsonPropertyName("explicit")]
    public List<HashMap> Explicit { get; set; }

    [JsonPropertyName("implicit")]
    public List<HashMap> Implicit { get; set; }

    [JsonPropertyName("desecrated")]
    public List<HashMap> Desecrated { get; set; }

    [JsonPropertyName("fractured")]
    public List<HashMap> Fractured { get; set; }
}
