using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Two;

public sealed class NinjaItemTwoContract
{
    [JsonPropertyName("coreCurrencyItems")]
    public List<NinjaCurrencyItem> CurrencyItems { get; set; }

    [JsonPropertyName("items")]
    public List<NinjaDataItem> Items { get; set; }
}
