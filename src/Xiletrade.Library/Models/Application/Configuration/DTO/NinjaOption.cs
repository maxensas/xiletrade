using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class NinjaOption
{
    [JsonPropertyName("map_generation")]
    public string MapGeneration { get; set; } = null;
}
