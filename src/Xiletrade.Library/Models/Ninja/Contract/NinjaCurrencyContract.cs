using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

public sealed class NinjaCurrencyContract
{
    [JsonPropertyName("lines")]
    public NinjaCurLines[] Lines { get; set; } = null;

    [JsonPropertyName("currencyDetails")]
    public NinjaCurDetails[] Details { get; set; } = null;
}
