using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class FetchData
{
    [DataMember(Name = "result")]
    public FetchDataInfo[] Result { get; set; } = new FetchDataInfo[5];
}
