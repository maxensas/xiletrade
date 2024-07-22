using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

public sealed class ToolTipItem
{
    public string Text { get; set; }
    public string Kind { get; set; }

    public ToolTipItem(string valText, string custom = null)
    {
        Text = valText;

        Dictionary<string, string> dicTag = new()
        {
            { Resources.Resources.General082_TagAttack, Strings.ModTag.Attack },
            { Resources.Resources.General083_TagPhysical, Strings.ModTag.Physical },
            { Resources.Resources.General084_TagCaster, Strings.ModTag.Caster },
            { Resources.Resources.General085_TagSpeed, Strings.ModTag.Speed },
            { Resources.Resources.General086_TagCritical, Strings.ModTag.Critical },
            { Resources.Resources.General087_TagFire, Strings.ModTag.Fire },
            { Resources.Resources.General088_TagCold, Strings.ModTag.Cold },
            { Resources.Resources.General089_TagLightning, Strings.ModTag.Lightning },
            { Resources.Resources.General090_TagChaos, Strings.ModTag.Chaos },
            { Resources.Resources.General091_TagLife, Strings.ModTag.Life },
            { Resources.Resources.General092_TagDefences, Strings.ModTag.Defences }
        };

        Kind = dicTag.TryGetValue(Text, out string value) ? value : custom?.Length > 0 ? custom : string.Empty;
    }
}
