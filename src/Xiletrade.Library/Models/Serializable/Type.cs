using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Type
{
    [DataMember(Name = "filters")]
    public TypeFilters Filters { get; set; } = new TypeFilters();
}
