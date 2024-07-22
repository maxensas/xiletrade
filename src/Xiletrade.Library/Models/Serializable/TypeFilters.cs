using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class TypeFilters
{
    [DataMember(Name = "category", EmitDefaultValue = false)]
    public Options Category { get; set; }

    [DataMember(Name = "rarity", EmitDefaultValue = false)]
    public Options Rarity { get; set; }
}
