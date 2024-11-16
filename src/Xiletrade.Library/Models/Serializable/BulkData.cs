using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract]
public sealed class BulkData
{
    [DataMember(Name = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [DataMember(Name = "total")]
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [DataMember(Name = "result")]
    [JsonPropertyName("result")]
    public Dictionary<string, FetchDataInfo> Result { get; set; }
}
