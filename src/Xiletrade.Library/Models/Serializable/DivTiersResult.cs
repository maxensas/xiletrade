using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class DivTiersResult
{
    [DataMember(Name = "tag")]
    public string Tag { get; set; } = null;
    [DataMember(Name = "tier")]
    public string Tier { get; set; } = null;
}
