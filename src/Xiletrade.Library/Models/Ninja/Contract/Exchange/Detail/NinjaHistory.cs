using System;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Exchange.Detail;

public sealed class NinjaHistory
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("rate")]
    public double Rate { get; set; }

    [JsonPropertyName("volumePrimaryValue")]
    public double VolumePrimaryValue { get; set; }
}
