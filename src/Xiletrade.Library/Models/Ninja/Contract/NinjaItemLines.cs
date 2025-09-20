using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

[DataContract]
public sealed class NinjaItemLines
{
    [DataMember(Name = "detailsId")]
    [JsonPropertyName("detailsId")]
    public string Id { get; set; } = null;

    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "chaosValue")]
    [JsonPropertyName("chaosValue")]
    public double ChaosPrice { get; set; } = 0;

    [DataMember(Name = "exaltedValue")]
    [JsonPropertyName("exaltedValue")]
    public double ExaltPrice { get; set; } = 0;

    [DataMember(Name = "divineValue")]
    [JsonPropertyName("divineValue")]
    public double DivinePrice { get; set; } = 0;
}
