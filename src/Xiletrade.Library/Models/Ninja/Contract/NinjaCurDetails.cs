using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

public sealed class NinjaCurDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = null;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("tradeId")]
    public string TradeId { get; set; } = null;
}
