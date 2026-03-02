using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Exchange.Detail;

public sealed class NinjaDetail
{
    [JsonPropertyName("item")]
    public NinjaCurrencyItem Item { get; set; }

    [JsonPropertyName("pairs")]
    public List<NinjaPair> Pairs { get; set; }

    [JsonPropertyName("core")]
    public NinjaCore Core { get; set; }
}
