using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Exchange;

public sealed class NinjaExchangeContract
{
    [JsonPropertyName("core")]
    public NinjaCore Core { get; set; }

    [JsonPropertyName("lines")]
    public List<NinjaExchangeDataLine> Line { get; set; }

    [JsonPropertyName("items")]
    public List<NinjaCurrencyItem> Items { get; set; }
}
