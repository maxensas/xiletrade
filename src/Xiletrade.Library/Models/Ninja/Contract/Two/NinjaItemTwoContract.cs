using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Two;

public sealed class NinjaItemTwoContract
{
    [JsonPropertyName("core")]
    public NinjaCore Core { get; set; }

    [JsonPropertyName("lines")]
    public List<NinjaDataLine> Line { get; set; }

    [JsonPropertyName("items")]
    public List<NinjaCurrencyItem> Items { get; set; }
}
