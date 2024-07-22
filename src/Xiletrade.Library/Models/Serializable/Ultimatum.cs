using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Ultimatum
{
    [DataMember(Name = "disabled")]
    public bool Disabled { get; set; } = false;

    [DataMember(Name = "filters")]
    public UltimatumFilters Filters { get; set; } = new UltimatumFilters();
}
