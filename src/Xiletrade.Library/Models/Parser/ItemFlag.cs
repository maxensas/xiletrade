using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Parser;

public sealed class ItemFlag
{
    // init with constructor
    internal bool Unique { get; }
    internal bool Rare { get; }
    internal bool Magic { get; }
    internal bool Normal { get; }
    internal bool Gem { get; }
    internal bool Currency { get; }
    internal bool Divcard { get; }
    internal bool Jewel { get; }
    internal bool Cluster { get; }
    internal bool Watchstone { get; }
    internal bool Invitation { get; }
    internal bool Facetor { get; }
    internal bool Chronicle { get; }
    internal bool Ultimatum { get; }
    internal bool FilledCoffin { get; }
    internal bool Logbook { get; }
    internal bool ChargedCompass { get; }
    internal bool Incubator { get; }
    internal bool ScourgedMap { get; }
    internal bool Metamorph { get; }
    internal bool Voidstone { get; }
    internal bool MapFragment { get; }
    internal bool Flask { get; }
    internal bool CapturedBeast { get; }
    internal bool ArmourPiece { get; }
    internal bool Shield { get; }
    internal bool Stave { get; }
    internal bool ShowDetail { get; }
    internal bool Sentinel { get; }
    internal bool MirroredTablet { get; }
    internal bool MemoryLine { get; }
    internal bool SanctumResearch { get; }
    internal bool SanctumRelic { get; }
    internal bool Tincture { get; }
    internal bool Charm { get; }
    internal bool AllflameEmber { get; }
    internal bool Corpses { get; }
    internal bool Rune { get; }
    internal bool Wand { get; }
    internal bool Focus { get; }
    internal bool Sceptre { get; }
    // TODO : update with all weapon item class
    internal bool TrialCoins { get; }
    internal bool Omen { get; }
    internal bool Socketable { get; }
    internal bool SkillGems { get; }
    internal bool SupportGems { get; }
    internal bool Tablet { get; }
    internal bool Waystones { get; }
    internal bool UltimatumTrial { get; }

    // init in second step
    internal bool Unidentified { get; set; }
    internal bool Corrupted { get; set; }
    internal bool Mirrored { get; set; }
    internal bool FoilVariant { get; set; }
    internal bool ScourgedItem { get; set; }
    internal bool MapCategory { get; set; }

    // init in third step
    internal bool ItemLevel { get; set; }
    internal bool AreaLevel { get; set; }
    internal bool Weapon { get; set; }

    // init in fourth step
    internal bool ExchangeCurrency { get; set; }
    internal bool SpecialBase { get; set; }
    internal bool BlightMap { get; set; }
    internal bool BlightRavagedMap { get; set; }
    internal bool ConqMap { get; set; }

    public ItemFlag(string[] clipData, string itemRarity, string itemType, string itemClass)
    {
        // using rarity
        Unique = itemRarity == Resources.Resources.General006_Unique;
        Rare = itemRarity == Resources.Resources.General007_Rare;
        Magic = itemRarity == Resources.Resources.General008_Magic;
        Normal = itemRarity == Resources.Resources.General009_Normal;
        Gem = itemRarity == Resources.Resources.General029_Gem;
        Currency = itemRarity == Resources.Resources.General026_Currency;
        Divcard = itemRarity == Resources.Resources.General028_DivinationCard;

        // using item type
        Cluster = itemType.Contain(Resources.Resources.General022_Cluster);
        Watchstone = itemType.Contain(Resources.Resources.General062_Watchstone);
        Invitation = itemType.Contain(Resources.Resources.General063_Invitation);
        Facetor = itemType.Contain(Resources.Resources.General064_FacetorLens);
        Chronicle = itemType.Contain(Resources.Resources.General065_ChronicleAtzoatl);
        FilledCoffin = itemType.Contain(Resources.Resources.General127_FilledCoffin); // ONLY IN ENGLISH FOR NOW
        Rune = itemType.Contain(Resources.Resources.General132_Rune);
        ChargedCompass = itemType.Contain(Resources.Resources.General105_ChargedCompass);
        Incubator = itemType.Contain(Resources.Resources.General027_Incubator);
        ScourgedMap = itemType.Contain(Resources.Resources.General103_Scourged);
        MirroredTablet = itemType.Contain(Resources.Resources.General108_MirroredTablet);
        Ultimatum = itemType.Contain(Resources.Resources.General066_InscribedUltimatum);

        // using item class
        Flask = itemClass.Contain(Resources.Resources.ItemClass_utilityFlask)
            || itemClass.Contain(Resources.Resources.ItemClass_lifeFlask)
            || itemClass.Contain(Resources.Resources.ItemClass_manaFlask);
        Jewel = itemClass.Contain(Resources.Resources.ItemClass_jewels);
        Wand = itemClass.Contain(Resources.Resources.ItemClass_wand);
        Voidstone = itemClass.Contain(Resources.Resources.ItemClass_atlas);
        MemoryLine = itemClass.Contain(Resources.Resources.ItemClass_memory);
        SanctumResearch = itemClass.Contain(Resources.Resources.ItemClass_sanctumResearch);
        SanctumRelic = itemClass.Contain(Resources.Resources.ItemClass_sanctumRelic);
        MapFragment = itemClass.Contain(Resources.Resources.ItemClass_mapFragments);
        ArmourPiece = itemClass.Contain(Resources.Resources.ItemClass_bodyArmours)
            || itemClass.Contain(Resources.Resources.ItemClass_boots)
            || itemClass.Contain(Resources.Resources.ItemClass_gloves)
            || itemClass.Contain(Resources.Resources.ItemClass_helmets)
            || itemClass.Contain(Resources.Resources.ItemClass_shields);
        Shield = itemClass.Contain(Resources.Resources.ItemClass_shields);
        Stave = itemClass.Contain(Resources.Resources.ItemClass_staff)
            || itemClass.Contain(Resources.Resources.ItemClass_warstaff);
        Sentinel = itemClass.Contain(Resources.Resources.ItemClass_sentinel);
        Tincture = itemClass.Contain(Resources.Resources.ItemClass_tincture);
        Charm = itemClass.Contain(Resources.Resources.ItemClass_charm);
        AllflameEmber = itemClass.Contain(Resources.Resources.ItemClass_allflame);
        Corpses = itemClass.Contain(Resources.Resources.ItemClass_corpses);
        UltimatumTrial = itemClass.StartWith(Resources.Resources.ItemClass_inscribedUltimatum);
        Logbook = itemClass.StartWith(Resources.Resources.ItemClass_expeditionLogbooks)
            || itemType.Contain(Resources.Resources.General094_Logbook);
        TrialCoins = itemClass.StartWith(Resources.Resources.ItemClass_trialCoins);
        Omen = itemClass.StartWith(Resources.Resources.ItemClass_omen);
        Socketable = itemClass.StartWith(Resources.Resources.ItemClass_socketable);
        SkillGems = itemClass.StartWith(Resources.Resources.ItemClass_skillGems);
        SupportGems = itemClass.StartWith(Resources.Resources.ItemClass_supportGems);
        Tablet = itemClass.StartWith(Resources.Resources.ItemClass_tablet);
        Waystones = itemClass.StartWith(Resources.Resources.ItemClass_waystones);
        Focus = itemClass.StartWith(Resources.Resources.ItemClass_foci);
        Sceptre = itemClass.StartWith(Resources.Resources.ItemClass_sceptres);

        // using clipdata
        CapturedBeast = clipData[^1].Contain(Resources.Resources.General054_ChkBeast);

        ShowDetail = Gem || Divcard || AllflameEmber
            || MapFragment && !Invitation && !Chronicle && !Ultimatum && !MirroredTablet
            || Currency && !Chronicle && !Ultimatum && !MirroredTablet && !FilledCoffin;
    }

    /// <summary>
    /// Deprecated
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="idLang"></param>
    /// <returns></returns>
    private static bool IsJewel(string itemType, int idLang)
    {
        bool is_jewel = false;
        if (itemType.Contain(Resources.Resources.General021_Jewel))
        {
            if (idLang is 8 or 9) // tw, cn
            {
                int idx = itemType.IndexOf(Resources.Resources.General021_Jewel, StringComparison.Ordinal);
                if (idx + 2 == itemType.Length) // "珠寶", "珠宝"
                {
                    is_jewel = true;
                }
                return is_jewel;
            }
            string[] tjew = itemType.Replace("-", " ").Split(' ');
            for (int i = 0; i < tjew.Length; i++)
            {
                if (tjew[i] == Resources.Resources.General021_Jewel)
                {
                    is_jewel = true;
                    break;
                }
            }
        }
        return is_jewel;
    }
}
