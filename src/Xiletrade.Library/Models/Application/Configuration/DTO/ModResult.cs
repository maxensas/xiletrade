using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ModResult
{
    [JsonPropertyName("data")]
    public ModResultData[] Data { get; set; } = null;
}
