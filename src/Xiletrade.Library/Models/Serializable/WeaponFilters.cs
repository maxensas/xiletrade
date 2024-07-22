using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WeaponFilters
{
    [DataMember(Name = "dps", EmitDefaultValue = false)]
    public MinMax Damage { get; set; } = new MinMax();

    [DataMember(Name = "pdps", EmitDefaultValue = false)]
    public MinMax Pdps { get; set; } = new MinMax();

    [DataMember(Name = "edps", EmitDefaultValue = false)]
    public MinMax Edps { get; set; } = new MinMax();
}
