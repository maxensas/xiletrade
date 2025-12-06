using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class BaseResult
{
    [JsonPropertyName("data")]
    public BaseResultData[] Data { get; set; } = null;
}
