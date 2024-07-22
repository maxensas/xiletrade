using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Misc
{
    [DataMember(Name = "disabled")]
    public bool Disabled { get; set; } = false;

    [DataMember(Name = "filters")]
    public MiscFilters Filters { get; set; } = new MiscFilters();
}
