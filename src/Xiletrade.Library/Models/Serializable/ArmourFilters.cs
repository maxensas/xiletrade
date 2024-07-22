using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class ArmourFilters
{
    [DataMember(Name = "ar")]
    public MinMax Armour { get; set; } = new MinMax();

    [DataMember(Name = "es")]
    public MinMax Energy { get; set; } = new MinMax();

    [DataMember(Name = "ev")]
    public MinMax Evasion { get; set; } = new MinMax();

    [DataMember(Name = "ward")]
    public MinMax Ward { get; set; } = new MinMax();
}
