using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

public sealed record ItemFlagTag
{
    internal bool CapturedBeast { get; }
    internal bool Transfigured { get; }
    internal bool Unidentified { get; }
    internal bool OnceCorrupted { get; }
    internal bool TwiceCorrupted { get; }
    internal bool Mirrored { get; }
    internal bool Fractured { get; }
    internal bool Synthesised { get; }
    internal bool Split { get; }
    internal bool Imbued { get; }
    internal bool FoilVariant { get; }
    internal bool ScourgedItem { get; }
    internal bool ItemLevel { get; }
    internal bool AreaLevel { get; }
    internal bool VaalSkillGems { get; }

    //influenced items
    internal bool InfluenceShaper { get; }
    internal bool InfluenceElder { get; }
    internal bool InfluenceCrusader { get; }
    internal bool InfluenceRedeemer { get; }
    internal bool InfluenceHunter { get; }
    internal bool InfluenceWarlord { get; }

    // group
    internal bool Corrupted { get; }

    /// <summary>
    /// Use clipboard data to recover specific flag parameters
    /// </summary>
    /// <param name="infodesc"></param>
    /// <param name="gem"></param>
    internal ItemFlagTag(InfoDescription infodesc, ItemFlagGem gem)
    {
        foreach (var data in infodesc.Item)
        {
            var line = data.Replace(Strings.CRLF, string.Empty);
            if (!CapturedBeast)
            {
                CapturedBeast = line.StartsWith(Resources.Resources.General054_ChkBeast);
            }
            if (!Transfigured)
            {
                Transfigured = line.Equal(Resources.Resources.General150_Transfigured);
            }
            if (!Unidentified)
            {
                Unidentified = line.Equal(Resources.Resources.General039_Unidentify);
            }
            if (!OnceCorrupted)
            {
                OnceCorrupted = line.Equal(Resources.Resources.General037_Corrupt);
            }
            if (!TwiceCorrupted)
            {
                TwiceCorrupted = line.Equal(Resources.Resources.General210_TwiceCorrupted);
            }
            if (!Mirrored)
            {
                Mirrored = line.Equal(Resources.Resources.General109_Mirrored);
            }
            if (!Fractured)
            {
                Fractured = line.Equal(Resources.Resources.General167_FracturedItem);
            }
            if (!Synthesised)
            {
                Synthesised = line.Equal(Resources.Resources.General047_Synthesis);
            }
            if (!Split)
            {
                Split = line.Equal(Resources.Resources.General157_Split);
            }
            if (!Imbued)
            {
                Imbued = line.Equal(Resources.Resources.General174_Imbued);
            }
            if (!FoilVariant)
            {
                FoilVariant = line.StartWith(Resources.Resources.General110_FoilUnique);
            }
            if (!ScourgedItem)
            {
                ScourgedItem = line.Equal(Resources.Resources.General099_ScourgedItem);
            }
            if (!ItemLevel)
            {
                ItemLevel = line.Contain(Resources.Resources.General032_ItemLv)
                    || line.Contain(Resources.Resources.General143_WaystoneTier)
                    || line.Contain(Resources.Resources.General242_MercenaryLevel);
            }
            if (!AreaLevel)
            {
                AreaLevel = line.Contain(Resources.Resources.General067_AreaLevel)
                    || line.Contain(Resources.Resources.General198_AreaLevelBis);
            }
            if (gem.Skill && !VaalSkillGems)
            {
                VaalSkillGems = line.Contain(Resources.Resources.General038_Vaal);
            }
            if (!InfluenceShaper)
            {
                InfluenceShaper = line.Contain(Resources.Resources.General041_Shaper);
            }
            if (!InfluenceElder)
            {
                InfluenceElder = line.Contain(Resources.Resources.General042_Elder);
            }
            if (!InfluenceCrusader)
            {
                InfluenceCrusader = line.Contain(Resources.Resources.General043_Crusader);
            }
            if (!InfluenceRedeemer)
            {
                InfluenceRedeemer = line.Contain(Resources.Resources.General044_Redeemer);
            }
            if (!InfluenceHunter)
            {
                InfluenceHunter = line.Contain(Resources.Resources.General045_Hunter);
            }
            if (!InfluenceWarlord)
            {
                InfluenceWarlord = line.Contain(Resources.Resources.General046_Warlord);
            }
        }

        Corrupted = OnceCorrupted || TwiceCorrupted;
    }
}
