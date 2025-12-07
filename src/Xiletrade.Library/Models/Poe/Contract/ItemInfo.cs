using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ItemInfo
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public double Amount { get; set; } = 0;

    [JsonPropertyName("stock")]
    public int Stock { get; set; } = 0;

    [JsonPropertyName("whisper")]
    public string Whisper { get; set; } = string.Empty;
}
