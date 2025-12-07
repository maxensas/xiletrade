using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class FetchData
{
    [JsonPropertyName("result")]
    public FetchDataInfo[] Result { get; set; } = new FetchDataInfo[5];
}
