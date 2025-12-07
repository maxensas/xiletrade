using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ExchangeStatus
{
    [JsonPropertyName("option")]
    public string Option { get; set; } = "online";
}
