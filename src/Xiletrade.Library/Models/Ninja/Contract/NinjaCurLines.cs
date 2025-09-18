using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

[DataContract]
public sealed class NinjaCurLines
{
    [DataMember(Name = "detailsId")]
    [JsonPropertyName("detailsId")]
    public string Id { get; set; } = null;

    [DataMember(Name = "currencyTypeName")]
    [JsonPropertyName("currencyTypeName")]
    public string Name { get; set; } = null;

    [DataMember(Name = "chaosEquivalent")]
    [JsonPropertyName("chaosEquivalent")]
    public double ChaosPrice { get; set; } = 0;
}
