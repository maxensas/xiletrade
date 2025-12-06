using System.Text.Json.Serialization;

namespace Xiletrade.Library.Models.Application.Configuration.DTO;

public sealed class ConfigData
{
    [JsonPropertyName("options")]
    public ConfigOption Options { get; set; } = null;

    [JsonPropertyName("shortcuts")]
    public ConfigShortcut[] Shortcuts { get; set; } = null;
    /*
    [DataMember(Name = "checked")]
    public ConfigChecked[] Checked = null;
    */
    [JsonPropertyName("dangerous_map_mods")]
    public ConfigMods[] DangerousMapMods { get; set; } = null;

    [JsonPropertyName("rares_item_mods")]
    public ConfigMods[] RareItemMods { get; set; } = null;

    [JsonPropertyName("chat_commands")]
    public ChatCommands[] ChatCommands { get; set; } = null;

    [JsonPropertyName("regular_expressions")]
    public ConfigRegex[] RegularExpressions { get; set; } = null;
}
