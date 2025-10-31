using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.Two;

public sealed class QueryTwo : QueryType
{
    [JsonPropertyName("status")]
    [JsonPropertyOrder(0)]
    public OptionTxt Status { get; set; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; }

    [JsonPropertyName("stats")]
    public Stats[] Stats { get; set; }

    [JsonPropertyName("filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FiltersTwo Filters { get; set; } = new FiltersTwo();
}
