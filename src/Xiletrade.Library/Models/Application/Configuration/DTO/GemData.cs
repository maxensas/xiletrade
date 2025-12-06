using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class GemData
{
    [JsonPropertyName("result")]
    public GemResult[] Result { get; set; } = null;
}
