using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Exchange.Detail;

public sealed class NinjaPair
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("rate")]
    public double Rate { get; set; }

    [JsonPropertyName("volumePrimaryValue")]
    public double VolumePrimaryValue { get; set; }

    [JsonPropertyName("history")]
    public List<NinjaHistory> History { get; set; }
}
