using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

public sealed class NinjaCurLines
{
    [JsonPropertyName("detailsId")]
    public string Id { get; set; } = null;

    [JsonPropertyName("currencyTypeName")]
    public string Name { get; set; } = null;

    [JsonPropertyName("chaosEquivalent")]
    public double ChaosPrice { get; set; } = 0;
}
