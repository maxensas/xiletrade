using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class NinjaValue
{
    [DataMember(Name = "detailsId")]
    public string Id { get; set; } = null;

    [DataMember(Name = "name")]
    public string Name { get; set; } = null;

    [DataMember(Name = "chaosValue")]
    public double ChaosPrice { get; set; } = 0;

    [DataMember(Name = "exaltedValue")]
    public double ExaltPrice { get; set; } = 0;

    [DataMember(Name = "divineValue")]
    public double DivinePrice { get; set; } = 0;
}
