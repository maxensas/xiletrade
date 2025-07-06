using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

public sealed class DustData
{
    [JsonPropertyName("level")]
    public DustLevel[] Level { get; set; } = null;
}
