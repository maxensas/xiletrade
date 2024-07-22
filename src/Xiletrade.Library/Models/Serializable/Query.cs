using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Query
{
    [DataMember(Name = "status", Order = 0)]
    public Options Status { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "type", EmitDefaultValue = false)]
    public object Type { get; set; }

    [DataMember(Name = "stats")]
    public Stats[] Stats { get; set; }

    [DataMember(Name = "filters")]
    public Filters Filters { get; set; } = new Filters();
}
