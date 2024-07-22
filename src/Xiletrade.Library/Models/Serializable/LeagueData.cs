using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class LeagueData
{
    [DataMember(Name = "result")]
    public LeagueResult[] Result { get; set; } = null;
}
