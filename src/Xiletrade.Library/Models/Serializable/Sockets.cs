using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Sockets
{
    [DataMember(Name = "min", EmitDefaultValue = false)]
    public double? Min { get; set; }

    [DataMember(Name = "max", EmitDefaultValue = false)]
    public double? Max { get; set; }

    [DataMember(Name = "r", EmitDefaultValue = false)]
    public double? Red { get; set; }

    [DataMember(Name = "g", EmitDefaultValue = false)]
    public double? Green { get; set; }

    [DataMember(Name = "b", EmitDefaultValue = false)]
    public double? Blue { get; set; }

    [DataMember(Name = "w", EmitDefaultValue = false)]
    public double? White { get; set; }
}
