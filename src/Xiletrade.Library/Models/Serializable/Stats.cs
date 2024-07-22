using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Stats
{
    [DataMember(Name = "type")]
    public string Type { get; set; }

    [DataMember(Name = "filters")]
    public StatsFilters[] Filters { get; set; }
}
