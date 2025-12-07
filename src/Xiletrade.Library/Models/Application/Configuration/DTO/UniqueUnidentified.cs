using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class UniqueUnidentified
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null;

    [JsonPropertyName("ilvl_variant")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint[] IlvlVariant { get; set; } = null;

    [JsonPropertyName("valuable")]
    public bool Valuable { get; set; }

    [JsonPropertyName("image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Image { get; set; } = null;

    [JsonPropertyName("poe2")]
    public bool Poe2 { get; set; }
}
