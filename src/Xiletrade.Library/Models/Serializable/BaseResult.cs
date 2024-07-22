using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class BaseResult
{
    [DataMember(Name = "data")]
    public BaseResultData[] Data { get; set; } = null;
}
