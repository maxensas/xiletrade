using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ConfigShortcut
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = false;

    [JsonPropertyName("modifier")]
    public int Modifier { get; set; } = 0x0;

    [JsonPropertyName("fonction")]
    public string Fonction { get; set; } = null;

    [JsonPropertyName("keycode")]
    public int Keycode { get; set; } = 0;

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Value { get; set; } = null;
}
