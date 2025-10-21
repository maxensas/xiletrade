using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Two;

public sealed class NinjaDataItem
{
    [JsonPropertyName("item")]
    public NinjaCurrencyItem Item { get; set; }

    [JsonPropertyName("rate")]
    public NinjaCurrencyReference Rate { get; set; }
    /*
    [JsonPropertyName("sparklines")]
    public object Sparklines { get; set; }
    */
    [JsonPropertyName("volumes")]
    public NinjaCurrencyReference Volumes { get; set; }

    [JsonPropertyName("primaryValue")]
    public double PrimaryValue { get; set; }

    [JsonPropertyName("volumePrimaryValue")]
    public double VolumePrimaryValue { get; set; }

    [JsonPropertyName("maxVolumeCurrency")]
    public string MaxVolumeCurrency { get; set; }
}
