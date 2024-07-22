using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class MinMax
{
    [DataMember(Name = "min", EmitDefaultValue = false)]
    public double? Min { get; set; }

    [DataMember(Name = "max", EmitDefaultValue = false)]
    public double? Max { get; set; }

    [DataMember(Name = "option", EmitDefaultValue = false)]
    public object Option { get; set; }
}
