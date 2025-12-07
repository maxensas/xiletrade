using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ParserData
{
    [JsonPropertyName("mods")]
    public ModOption[] Mods { get; set; } = null;
}
