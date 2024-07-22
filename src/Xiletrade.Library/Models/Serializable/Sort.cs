using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class Sort
{
    [DataMember(Name = "price")]
    public string Price { get; set; }
}
