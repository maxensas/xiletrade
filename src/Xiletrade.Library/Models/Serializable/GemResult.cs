using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class GemResult
{
    [DataMember(Name = "Data")]
    public GemResultData[] Data { get; set; } = null;
}
