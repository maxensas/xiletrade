using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

public class ItemApi
{
    public string Name { get; }
    public string Tier { get; }
    public int Level { get; }
    public List<ExtendedMagnitudes> Magnitudes { get; }

    public string Mod { get; }

    /// <summary>Item level required by the tier : "≥50", null : unknown.</summary>
    public string RequiredLevel { get; }

    /// <summary>Range of each value : "50—60", the fixed values are not displayed, null : nothing to display.</summary>
    public string Range { get; }
    public bool TagLife { get; }
    public bool TagFire { get; }
    public bool TagCold { get; }
    public bool TagLightning { get; }
    public bool TagDesecrated { get; }
    public bool TagCrafted { get; }
    public bool TagFractured { get; }
    public bool TagMutated { get; }

    public ItemApi(ExtendedAffix affix, string mod, bool isDesecrated = false
        , bool isFractured = false, bool isMutated = false, bool isCrafted = false)
    {
        Name = affix.Name;
        if (affix.Tier?.Length > 0)
        {
            Tier = affix.Tier;
        }

        Level = affix.Level;
        Magnitudes = affix.Magnitudes;
        Mod = mod;
        RequiredLevel = Level > 0 ? "≥" + Level.ToString(CultureInfo.InvariantCulture) : null;
        Range = GetRange(Magnitudes);

        if (Magnitudes?.Count is 1)
        {
            var id = Magnitudes[0].Hash;
            TagLife = id is Strings.StatPoe1.MaxLife;
            TagFire = id is Strings.StatPoe1.FireResist;
            TagCold = id is Strings.StatPoe1.ColdResist;
            TagLightning = id is Strings.StatPoe1.LightningResist;
        }
        if (isDesecrated)
        {
            TagDesecrated = true;
        }
        if (isFractured)
        {
            TagFractured = true;
        }
        if (isMutated)
        {
            TagMutated = true;
        }
        if (isCrafted)
        {
            TagCrafted = true;
        }
    }

    private static string GetRange(List<ExtendedMagnitudes> magnitudes)
    {
        if (magnitudes is null)
        {
            return null;
        }
        StringBuilder range = null;
        foreach (var magnitude in magnitudes)
        {
            if (magnitude?.Min is not decimal min || magnitude.Max is not decimal max || min == max)
            {
                continue;
            }
            range = range is null ? new StringBuilder() : range.Append(' ');
            range.Append(min.ToString(CultureInfo.InvariantCulture)).Append('—')
                .Append(max.ToString(CultureInfo.InvariantCulture));
        }
        return range?.ToString();
    }
}
