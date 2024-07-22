using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class JsonData
{
    [DataMember(Name = "query")]
    public Query Query { get; set; } = new();

    [DataMember(Name = "sort")]
    public Sort Sort { get; set; } = new();
}
