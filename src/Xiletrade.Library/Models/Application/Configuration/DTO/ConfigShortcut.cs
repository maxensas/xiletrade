using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

[DataContract(Name = "shortcuts")]
public sealed class ConfigShortcut
{
    [DataMember(Name = "enable")]
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = false;

    [DataMember(Name = "modifier")]
    [JsonPropertyName("modifier")]
    public int Modifier { get; set; } = 0x0;

    [DataMember(Name = "fonction")]
    [JsonPropertyName("fonction")]
    public string Fonction { get; set; } = null;

    [DataMember(Name = "keycode")]
    [JsonPropertyName("keycode")]
    public int Keycode { get; set; } = 0;

    [DataMember(Name = "value", EmitDefaultValue = false)]
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Value { get; set; } = null;
}
