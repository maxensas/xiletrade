using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Poe.Contract;

public sealed class ResultData
{
    [JsonPropertyName("result")]
    public string[] Result { get; set; } = null;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public int Total { get; set; } = 0;
}
