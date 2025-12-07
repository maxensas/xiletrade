using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class WordData
{
    [JsonPropertyName("result")]
    public WordResult[] Result { get; set; } = null;
}
