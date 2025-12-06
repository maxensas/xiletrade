using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract.One;

public sealed class Query : QueryType
{
    [JsonPropertyName("status")]
    [JsonPropertyOrder(0)]
    public OptionTxt Status { get; set; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; }

    [JsonPropertyName("term")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Term { get; set; }

    [JsonPropertyName("stats")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Stats[] Stats { get; set; }

    [JsonPropertyName("filters")]
    public Filters Filters { get; set; } = new Filters();
}
