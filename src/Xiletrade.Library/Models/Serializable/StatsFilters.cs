using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class StatsFilters
{
    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    public MinMax Value { get; set; } = new();

    [DataMember(Name = "disabled")]
    public bool Disabled { get; set; }
}
