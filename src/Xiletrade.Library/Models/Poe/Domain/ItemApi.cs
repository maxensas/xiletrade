using System.Collections.Generic;
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

        if (Magnitudes?.Count is 1)
        {
            var id = Magnitudes[0].Hash;
            TagLife = id is Strings.Stat.MaxLife;
            TagFire = id is Strings.Stat.FireResist;
            TagCold = id is Strings.Stat.ColdResist;
            TagLightning = id is Strings.Stat.LightningResist;
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
}
