using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Serializable.SourceGeneration;

namespace Xiletrade.Library.Models.Serializable;

public sealed class ItemProperties
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("values")]
    [JsonConverter(typeof(ValueTupleListConverter))]
    public List<(string, int)> Values { get; set; }

    [JsonPropertyName("displayMode")]
    public int DisplayMode { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }
}
