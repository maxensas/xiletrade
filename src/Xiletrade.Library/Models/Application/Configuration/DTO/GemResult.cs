using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class GemResult
{
    [JsonPropertyName("data")]
    public GemResultData[] Data { get; set; } = null;
}
