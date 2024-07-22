using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract()]
public sealed class ConfigData
{
    [DataMember(Name = "options")]
    public ConfigOption Options { get; set; } = null;

    [DataMember(Name = "shortcuts")]
    public ConfigShortcut[] Shortcuts { get; set; } = null;
    /*
    [DataMember(Name = "checked")]
    public ConfigChecked[] Checked = null;
    */
    [DataMember(Name = "dangerous_map_mods")]
    public ConfigMods[] DangerousMods { get; set; } = null;

    [DataMember(Name = "rares_item_mods")]
    public ConfigMods[] RareMods { get; set; } = null;
}
