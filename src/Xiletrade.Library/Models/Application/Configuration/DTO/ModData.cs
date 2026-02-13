using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ModData
{
    [JsonPropertyName("result")]
    public ModResult[] Result { get; set; } = null;
}
