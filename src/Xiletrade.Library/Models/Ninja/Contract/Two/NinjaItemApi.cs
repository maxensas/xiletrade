using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Two;

public sealed class NinjaItemApi
{
    [JsonPropertyName("coreCurrencyItems")]
    public List<NinjaCurrencyItem> CurrencyItems { get; set; }

    [JsonPropertyName("items")]
    public List<NinjaItem> Items { get; set; }
}
