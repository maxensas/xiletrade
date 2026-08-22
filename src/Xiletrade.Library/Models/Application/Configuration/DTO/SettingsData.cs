using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class SettingsData
{
    [JsonPropertyName("ninja")]
    public NinjaOption Ninja { get; set; } = null;
}
