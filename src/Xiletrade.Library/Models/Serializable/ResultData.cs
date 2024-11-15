using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class ResultData
{
    [DataMember(Name = "Result")]
    public string[] Result { get; set; } = null;

    [DataMember(Name = "Id")]
    public string Id { get; set; } = string.Empty;

    [DataMember(Name = "Total")]
    public int Total { get; set; } = 0;
}
