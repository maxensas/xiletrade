using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Ninja.Contract;

[DataContract]
public sealed class NinjaCurDetails
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;

    [DataMember(Name = "icon")]
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = null;

    [DataMember(Name = "name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "tradeId")]
    [JsonPropertyName("tradeId")]
    public string TradeId { get; set; } = null;
}
