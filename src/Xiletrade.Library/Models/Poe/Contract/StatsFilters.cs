using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

[DataContract]
public sealed class StatsFilters
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Value { get; set; } = new();

    [DataMember(Name = "disabled")]
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
}
