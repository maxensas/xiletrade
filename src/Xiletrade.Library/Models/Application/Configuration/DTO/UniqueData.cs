using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class UniqueData
{
    [JsonPropertyName("unique")]
    public Unique[] Unique { get; set; } = null;
}
