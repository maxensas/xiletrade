using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WeaponFilters
{
    [DataMember(Name = "dps", EmitDefaultValue = false)]
    [JsonPropertyName("dps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Damage { get; set; } = new MinMax();

    [DataMember(Name = "pdps", EmitDefaultValue = false)]
    [JsonPropertyName("pdps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Pdps { get; set; } = new MinMax();

    [DataMember(Name = "edps", EmitDefaultValue = false)]
    [JsonPropertyName("edps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Edps { get; set; } = new MinMax();
}
