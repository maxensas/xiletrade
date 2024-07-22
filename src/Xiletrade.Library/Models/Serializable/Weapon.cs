using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Weapon
{
    [DataMember(Name = "disabled")]
    public bool Disabled { get; set; } = false;

    [DataMember(Name = "filters")]
    public WeaponFilters Filters { get; set; } = new WeaponFilters();
}
