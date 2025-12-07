using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class DivTiersData
{
    [JsonPropertyName("result")]
    public DivTiersResult[] Result { get; set; } = null;
}
