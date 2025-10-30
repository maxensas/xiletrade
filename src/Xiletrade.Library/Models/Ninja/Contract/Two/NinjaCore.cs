using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Two;

public sealed class NinjaCore
{
    [JsonPropertyName("items")]
    public List<NinjaCurrencyItem> Items { get; set; }

    [JsonPropertyName("rates")]
    public NinjaCurrencyReference Rates { get; set; }

    [JsonPropertyName("primary")]
    public string Primary { get; set; }

    [JsonPropertyName("secondary")]
    public string Secondary { get; set; }
}
