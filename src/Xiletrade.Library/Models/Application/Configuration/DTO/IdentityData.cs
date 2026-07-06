using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class IdentityData
{
    [JsonPropertyName("identity")]
    public Identity[] Identity { get; set; } = null;
}
