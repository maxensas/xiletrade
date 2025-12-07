using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class BulkData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("result")]
    public Dictionary<string, FetchDataInfo> Result { get; set; }
}
