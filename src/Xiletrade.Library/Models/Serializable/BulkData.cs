using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class BulkData
{
    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "total")]
    public int Total { get; set; }

    [DataMember(Name = "result")]
    public Dictionary<string, FetchDataInfo> Result { get; set; }
}
