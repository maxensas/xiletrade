using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract()]
public sealed class ConfigData
{
    [DataMember(Name = "options")]
    [JsonPropertyName("options")]
    public ConfigOption Options { get; set; } = null;

    [DataMember(Name = "shortcuts")]
    [JsonPropertyName("shortcuts")]
    public ConfigShortcut[] Shortcuts { get; set; } = null;
    /*
    [DataMember(Name = "checked")]
    public ConfigChecked[] Checked = null;
    */
    [DataMember(Name = "dangerous_map_mods")]
    [JsonPropertyName("dangerous_map_mods")]
    public ConfigMods[] DangerousMapMods { get; set; } = null;

    [DataMember(Name = "rares_item_mods")]
    [JsonPropertyName("rares_item_mods")]
    public ConfigMods[] RareItemMods { get; set; } = null;

    [DataMember(Name = "chat_commands")]
    [JsonPropertyName("chat_commands")]
    public ChatCommands[] ChatCommands { get; set; } = null;
}
