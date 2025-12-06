using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class WordResult
{
    [JsonPropertyName("data")]
    public WordResultData[] Data { get; set; } = null;
}
