using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Socket
{
    [DataMember(Name = "disabled")]
    public bool Disabled { get; set; } = false;

    [DataMember(Name = "filters", EmitDefaultValue = false)]
    public SocketFilters Filters { get; set; } = new();
}
