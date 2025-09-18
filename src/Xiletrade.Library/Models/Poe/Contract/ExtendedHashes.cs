using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Application.Serialization.Converter;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.Models.Poe.Contract;

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
