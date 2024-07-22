using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class DivTiersData
{
    [DataMember(Name = "result")]
    public DivTiersResult[] Result { get; set; } = null;
}
