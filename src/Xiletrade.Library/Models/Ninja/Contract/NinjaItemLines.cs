using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

public sealed class NinjaItemLines
{
    [JsonPropertyName("detailsId")]
    public string Id { get; set; } = null;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("chaosValue")]
    public double ChaosPrice { get; set; } = 0;

    [JsonPropertyName("exaltedValue")]
    public double ExaltPrice { get; set; } = 0;

    [JsonPropertyName("divineValue")]
    public double DivinePrice { get; set; } = 0;
    /*
    [JsonPropertyName("baseType")]
    public string BaseType { get; set; } = null;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = null;

    [JsonPropertyName("variant")]
    public string Variant { get; set; } = null;

    [JsonPropertyName("mapTier")]
    public int MapTier { get; set; } = 0;
    */
}
