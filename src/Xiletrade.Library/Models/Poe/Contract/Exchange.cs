using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class Exchange
{
    [JsonPropertyName("exchange")]
    public ExchangeData ExchangeData { get; set; } = new();

    [JsonPropertyName("engine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Engine { get; set; } = null; // shop: "new"
}