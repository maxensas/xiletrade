using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class LicenceData
{
    [DataMember(Name = "licence")]
    public string Licence { get; set; } = null;
}
