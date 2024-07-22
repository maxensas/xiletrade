using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class NinjaItemContract
{
    [DataMember(Name = "lines")]
    public NinjaItemLines[] Lines { get; set; } = null;
}
