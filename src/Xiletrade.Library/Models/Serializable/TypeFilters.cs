using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class TypeFilters
{
    [DataMember(Name = "category", EmitDefaultValue = false)]
    [JsonPropertyName("category")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Category { get; set; }

    [DataMember(Name = "rarity", EmitDefaultValue = false)]
    [JsonPropertyName("rarity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OptionTxt Rarity { get; set; }
}
