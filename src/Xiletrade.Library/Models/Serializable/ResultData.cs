using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class ResultData
{
    [DataMember(Name = "result")]
    public string[] Result { get; set; } = null;

    [DataMember(Name = "id")]
    public string ID { get; set; } = string.Empty;

    [DataMember(Name = "total")]
    public int Total { get; set; } = 0;
}
