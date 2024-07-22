using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class SocketFilters
{
    [DataMember(Name = "sockets", EmitDefaultValue = false)]
    public Sockets Sockets { get; set; } = new(); //public MinMax Sockets { get; set; } = new();

    [DataMember(Name = "links", EmitDefaultValue = false)]
    public MinMax Links { get; set; } = new();
}
