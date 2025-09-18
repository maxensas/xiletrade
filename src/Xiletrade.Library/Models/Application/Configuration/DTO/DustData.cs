using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class DustData
{
    [JsonPropertyName("level")]
    public DustLevel[] Level { get; set; } = null;
}
