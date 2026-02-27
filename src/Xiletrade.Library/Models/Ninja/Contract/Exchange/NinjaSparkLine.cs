using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Exchange;

public sealed class NinjaSparkLine
{
    [JsonPropertyName("totalChange")]
    public double TotalChange { get; set; }

    [JsonPropertyName("data")]
    public List<double> Data { get; set; }
}
