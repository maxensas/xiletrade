using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class NinjaCurrencyContract
{
    [DataMember(Name = "lines")]
    public NinjaCurLines[] Lines { get; set; } = null;

    [DataMember(Name = "currencyDetails")]
    public NinjaCurDetails[] Details { get; set; } = null;
}
