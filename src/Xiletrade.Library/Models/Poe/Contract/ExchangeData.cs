using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class ExchangeData
{
    [DataMember(Name = "status")]
    [JsonPropertyName("status")]
    public ExchangeStatus Status { get; set; } = new();

    [DataMember(Name = "have", EmitDefaultValue = false)]
    [JsonPropertyName("have")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Have { get; set; } = null;

    [DataMember(Name = "want", EmitDefaultValue = false)]
    [JsonPropertyName("want")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Want { get; set; } = null;

    [DataMember(Name = "minimum")]
    [JsonPropertyName("minimum")]
    public int Minimum { get; set; } = 1;

    [DataMember(Name = "collapse", EmitDefaultValue = false)]
    [JsonPropertyName("collapse")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object Collapse { get; set; } = null; // shop: true
}
