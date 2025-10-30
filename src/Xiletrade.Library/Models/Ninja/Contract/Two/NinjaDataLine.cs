using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract.Two;

public sealed class NinjaDataLine
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("primaryValue")]
    public double PrimaryValue { get; set; }

    [JsonPropertyName("volumePrimaryValue")]
    public double VolumePrimaryValue { get; set; }

    [JsonPropertyName("maxVolumeCurrency")]
    public string MaxVolumeCurrency { get; set; }

    [JsonPropertyName("maxVolumeRate")]
    public double MaxVolumeRate { get; set; }

    /*
    [JsonPropertyName("sparkline")]
    public NinjaSparkLine Sparkline { get; set; }*/
}
