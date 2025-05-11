using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Serializable.SourceGeneration;

namespace Xiletrade.Library.Models.Serializable;

public sealed class QueryTwo
{
    [JsonPropertyName("status")]
    [JsonPropertyOrder(0)]
    public OptionTxt Status { get; set; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(QueryTypeJsonConverter))]
    public object Type { get; set; } // can be 'GemTransfigured' or string

    [JsonPropertyName("stats")]
    public Stats[] Stats { get; set; }

    [JsonPropertyName("filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FiltersTwo Filters { get; set; } = new FiltersTwo();
}
