using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xiletrade.Library.Models.Ninja.Contract.Exchange;

namespace Xiletrade.Library.Models.Ninja.Contract;

public sealed class NinjaItemTwoContract
{
    [JsonPropertyName("core")]
    public NinjaCore Core { get; set; }

    [JsonPropertyName("lines")]
    public List<NinjaItemDataLine> Line { get; set; }
}
