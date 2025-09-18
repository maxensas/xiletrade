using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class Exchange
{
    [DataMember(Name = "exchange")]
    [JsonPropertyName("exchange")]
    public ExchangeData ExchangeData { get; set; } = new();

    [DataMember(Name = "engine", EmitDefaultValue = false)]
    [JsonPropertyName("engine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Engine { get; set; } = null; // shop: "new"
}