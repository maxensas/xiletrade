using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class GemResult
{
    [DataMember(Name = "data")]
    public GemResultData[] Data { get; set; } = null;
}
