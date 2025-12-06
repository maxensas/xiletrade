using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class DivTiersResult
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = null;
   
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = null;
}
