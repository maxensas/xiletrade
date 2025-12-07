using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ExchangeData
{
    [JsonPropertyName("status")]
    public ExchangeStatus Status { get; set; } = new();

    [JsonPropertyName("have")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Have { get; set; } = null;

    [JsonPropertyName("want")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Want { get; set; } = null;

    [JsonPropertyName("minimum")]
    public int Minimum { get; set; } = 1;

    [JsonPropertyName("collapse")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object Collapse { get; set; } = null; // shop: true
}
