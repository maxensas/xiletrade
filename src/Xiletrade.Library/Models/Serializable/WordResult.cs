using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class WordResult
{
    [DataMember(Name = "data")]
    public WordResultData[] Data { get; set; } = null;
}
