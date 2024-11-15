using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WordResult
{
    [DataMember(Name = "Data")]
    public WordResultData[] Data { get; set; } = null;
}
