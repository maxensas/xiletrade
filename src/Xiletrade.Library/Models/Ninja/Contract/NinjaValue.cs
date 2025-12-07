using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

public sealed class NinjaValue
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
}
