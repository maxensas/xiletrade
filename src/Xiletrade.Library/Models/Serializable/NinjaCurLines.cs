using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class NinjaCurLines
{
    [DataMember(Name = "detailsId")]
    public string Id { get; set; } = null;

    [DataMember(Name = "currencyTypeName")]
    public string Name { get; set; } = null;

    [DataMember(Name = "chaosEquivalent")]
    public double ChaosPrice { get; set; } = 0;
}
