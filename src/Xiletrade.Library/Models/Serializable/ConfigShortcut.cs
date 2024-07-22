using System.Runtime.Serialization;

namespace Xiletrade.Library.Models.Serializable;

[DataContract(Name = "shortcuts")]
public sealed class ConfigShortcut
{
    [DataMember(Name = "enable")]
    public bool Enable { get; set; } = false;

    [DataMember(Name = "modifier")]
    public int Modifier { get; set; } = 0x0;

    [DataMember(Name = "fonction")]
    public string Fonction { get; set; } = null;

    [DataMember(Name = "keycode")]
    public int Keycode { get; set; } = 0;

    [DataMember(Name = "position")]
    public string Position { get; set; } = null;

    [DataMember(Name = "value")]
    public string Value { get; set; } = null;
}
