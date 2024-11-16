using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class SanctumFilters
{
    [DataMember(Name = "sanctum_resolve", EmitDefaultValue = false)]
    [JsonPropertyName("sanctum_resolve")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Resolve { get; set; } = new();

    [DataMember(Name = "sanctum_max_resolve", EmitDefaultValue = false)]
    [JsonPropertyName("sanctum_max_resolve")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax MaxResolve { get; set; } = new();

    [DataMember(Name = "sanctum_inspiration", EmitDefaultValue = false)]
    [JsonPropertyName("sanctum_inspiration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Inspiration { get; set; } = new();

    [DataMember(Name = "sanctum_gold", EmitDefaultValue = false)]
    [JsonPropertyName("sanctum_gold")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MinMax Aureus { get; set; } = new();
}
