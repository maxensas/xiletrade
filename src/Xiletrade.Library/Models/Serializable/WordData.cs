using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WordData
{
    [DataMember(Name = "Result")]
    public WordResult[] Result { get; set; } = null;
}
