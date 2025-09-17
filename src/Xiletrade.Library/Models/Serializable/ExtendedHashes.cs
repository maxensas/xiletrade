using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Serializable.SourceGeneration;

namespace Xiletrade.Library.Models.Serializable;

public sealed class ExtendedHashes
{
    [JsonPropertyName("explicit")]
    [JsonConverter(typeof(HashMapConverter))]
    public List<HashMap> Explicit { get; set; }

    [JsonPropertyName("implicit")]
    [JsonConverter(typeof(HashMapConverter))]
    public List<HashMap> Implicit { get; set; }

    [JsonPropertyName("desecrated")]
    [JsonConverter(typeof(HashMapConverter))]
    public List<HashMap> Desecrated { get; set; }
}
