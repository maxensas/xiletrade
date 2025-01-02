using System;

namespace Xiletrade.Library.Models;

internal sealed class ItemFlag
{
    // init with constructor
    internal bool Unique { get; private set; }
    internal bool Rare { get; private set; }
    internal bool Magic { get; private set; }
    internal bool Normal { get; private set; }
    internal bool Gem { get; private set; }
    internal bool Currency { get; private set; }
    internal bool Divcard { get; private set; }
    internal bool Jewel { get; private set; }
    internal bool Cluster { get; private set; }
    internal bool Watchstone { get; private set; }
    internal bool Invitation { get; private set; }
    internal bool Facetor { get; private set; }
    internal bool Chronicle { get; private set; }
    internal bool Ultimatum { get; private set; }
    internal bool FilledCoffin { get; private set; }
    internal bool Logbook { get; private set; }
    internal bool ChargedCompass { get; private set; }
    internal bool Incubator { get; private set; }
    internal bool ScourgedMap { get; private set; }
    internal bool Metamorph { get; private set; }
    internal bool Voidstone { get; private set; }
    internal bool MapFragment { get; private set; }
    internal bool Flask { get; private set; }
    internal bool CapturedBeast { get; private set; }
    internal bool ArmourPiece { get; private set; }
    internal bool Shield { get; private set; }
    internal bool Stave { get; private set; }
    internal bool ShowDetail { get; private set; }
    internal bool Sentinel { get; private set; }
    internal bool MirroredTablet { get; private set; }
    internal bool MemoryLine { get; private set; }
    internal bool SanctumResearch { get; private set; }
    internal bool SanctumRelic { get; private set; }
    internal bool Tincture { get; private set; }
    internal bool Charm { get; private set; }
    internal bool AllflameEmber { get; private set; }
    internal bool Corpses { get; private set; }
    internal bool Rune { get; private set; }
    internal bool Wand { get; private set; }
    internal bool Focus { get; private set; }
    //internal bool Sceptre { get; private set; }
    // TODO : update with all weapon item class

    internal bool TrialCoins { get; private set; }
    internal bool Omen { get; private set; }
    internal bool Socketable { get; private set; }
    internal bool SkillGems { get; private set; }
    internal bool SupportGems { get; private set; }
    internal bool Tablet { get; private set; }
    internal bool Waystones { get; private set; }
    internal bool UltimatumTrial { get; private set; }

    // init in second step
    internal bool ExchangeCurrency { get; set; }
    internal bool SpecialBase { get; set; }
    internal bool ItemLevel { get; set; }
    internal bool AreaLevel { get; set; }
    //public bool CorpseLevel { get; set; }
    internal bool Weapon { get; set; }
    internal bool BlightMap { get; set; }
    internal bool BlightRavagedMap { get; set; }
    internal bool ConqMap { get; set; }
    // init in second step
    internal bool Unidentified { get; set; }
    internal bool Corrupted { get; set; }
    internal bool Mirrored { get; set; }
    internal bool FoilVariant { get; set; }
    internal bool ScourgedItem { get; set; }
    internal bool MapCategory { get; set; }

    public ItemFlag(string[] clipData, int idLang, string itemRarity, string itemType, string itemClass)
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
        Cluster = itemType.Contains(Resources.Resources.General022_Cluster, StringComparison.Ordinal);
        Watchstone = itemType.Contains(Resources.Resources.General062_Watchstone, StringComparison.Ordinal);
        Invitation = itemType.Contains(Resources.Resources.General063_Invitation, StringComparison.Ordinal);
        Facetor = itemType.Contains(Resources.Resources.General064_FacetorLens, StringComparison.Ordinal);
        Chronicle = itemType.Contains(Resources.Resources.General065_ChronicleAtzoatl, StringComparison.Ordinal);
        FilledCoffin = itemType.Contains(Resources.Resources.General127_FilledCoffin, StringComparison.Ordinal); // ONLY IN ENGLISH FOR NOW
        Rune = itemType.Contains(Resources.Resources.General132_Rune, StringComparison.Ordinal);
        ChargedCompass = itemType.Contains(Resources.Resources.General105_ChargedCompass, StringComparison.Ordinal);
        Incubator = itemType.Contains(Resources.Resources.General027_Incubator, StringComparison.Ordinal);
        ScourgedMap = itemType.Contains(Resources.Resources.General103_Scourged, StringComparison.Ordinal);
        MirroredTablet = itemType.Contains(Resources.Resources.General108_MirroredTablet, StringComparison.Ordinal);
        Ultimatum = itemType.Contains(Resources.Resources.General066_InscribedUltimatum, StringComparison.Ordinal);

        // using item class
        Flask = itemClass.Contains(Resources.Resources.ItemClass_utilityFlask, StringComparison.Ordinal)
            || itemClass.Contains(Resources.Resources.ItemClass_lifeFlask, StringComparison.Ordinal)
            || itemClass.Contains(Resources.Resources.ItemClass_manaFlask, StringComparison.Ordinal);
        // old: Flask = clipData[^1].Contains(Resources.Resources.General053_ChkFlask, StringComparison.Ordinal);
        Jewel = itemClass.Contains(Resources.Resources.ItemClass_jewels, StringComparison.Ordinal);
        // old: Jewel = IsJewel(itemType, idLang);
        Wand = itemClass.Contains(Resources.Resources.ItemClass_wand, StringComparison.Ordinal);
        Voidstone = itemClass.Contains(Resources.Resources.ItemClass_atlas, StringComparison.Ordinal);
        MemoryLine = itemClass.Contains(Resources.Resources.ItemClass_memory, StringComparison.Ordinal);
        SanctumResearch = itemClass.Contains(Resources.Resources.ItemClass_sanctumResearch, StringComparison.Ordinal);
        SanctumRelic = itemClass.Contains(Resources.Resources.ItemClass_sanctumRelic, StringComparison.Ordinal);
        MapFragment = itemClass.Contains(Resources.Resources.ItemClass_mapFragments, StringComparison.Ordinal);
        ArmourPiece = itemClass.Contains(Resources.Resources.ItemClass_bodyArmours, StringComparison.Ordinal)
            || itemClass.Contains(Resources.Resources.ItemClass_boots, StringComparison.Ordinal)
            || itemClass.Contains(Resources.Resources.ItemClass_gloves, StringComparison.Ordinal)
            || itemClass.Contains(Resources.Resources.ItemClass_helmets, StringComparison.Ordinal)
            || itemClass.Contains(Resources.Resources.ItemClass_shields, StringComparison.Ordinal);
        Shield = itemClass.Contains(Resources.Resources.ItemClass_shields, StringComparison.Ordinal);
        Stave = itemClass.Contains(Resources.Resources.ItemClass_staff, StringComparison.Ordinal)
            || itemClass.Contains(Resources.Resources.ItemClass_warstaff, StringComparison.Ordinal);
        Sentinel = itemClass.Contains(Resources.Resources.ItemClass_sentinel, StringComparison.Ordinal);
        Tincture = itemClass.Contains(Resources.Resources.ItemClass_tincture, StringComparison.Ordinal);
        Charm = itemClass.Contains(Resources.Resources.ItemClass_charm, StringComparison.Ordinal);
        AllflameEmber = itemClass.Contains(Resources.Resources.ItemClass_allflame, StringComparison.Ordinal);
        Corpses = itemClass.Contains(Resources.Resources.ItemClass_corpses, StringComparison.Ordinal);
        UltimatumTrial = itemClass.StartsWith(Resources.Resources.ItemClass_inscribedUltimatum, StringComparison.Ordinal);
        Logbook = itemClass.StartsWith(Resources.Resources.ItemClass_expeditionLogbooks, StringComparison.Ordinal) 
            || itemType.Contains(Resources.Resources.General094_Logbook, StringComparison.Ordinal);
        TrialCoins = itemClass.StartsWith(Resources.Resources.ItemClass_trialCoins, StringComparison.Ordinal);
        Omen = itemClass.StartsWith(Resources.Resources.ItemClass_omen, StringComparison.Ordinal);
        Socketable = itemClass.StartsWith(Resources.Resources.ItemClass_socketable, StringComparison.Ordinal);
        SkillGems = itemClass.StartsWith(Resources.Resources.ItemClass_skillGems, StringComparison.Ordinal);
        SupportGems = itemClass.StartsWith(Resources.Resources.ItemClass_supportGems, StringComparison.Ordinal);
        Tablet = itemClass.StartsWith(Resources.Resources.ItemClass_tablet, StringComparison.Ordinal);
        Waystones = itemClass.StartsWith(Resources.Resources.ItemClass_waystones, StringComparison.Ordinal);
        Focus = itemClass.StartsWith(Resources.Resources.ItemClass_foci, StringComparison.Ordinal);

        // using clipdata
        CapturedBeast = clipData[^1].Contains(Resources.Resources.General054_ChkBeast, StringComparison.Ordinal);

        ShowDetail = Gem || Divcard || AllflameEmber /*|| Prophecy */
            || (MapFragment && !Invitation && !Chronicle && !Ultimatum && !MirroredTablet)
            || (Currency && !Chronicle && !Ultimatum && !MirroredTablet && !FilledCoffin);
    }

    private static bool IsJewel(string itemType, int idLang)
    {
        bool is_jewel = false;
        if (itemType.Contains(Resources.Resources.General021_Jewel, StringComparison.Ordinal))
        {
            if (idLang is 8 or 9) // tw, cn
            {
                int idx = itemType.IndexOf(Resources.Resources.General021_Jewel, StringComparison.Ordinal);
                if ((idx + 2) == itemType.Length) // "珠寶", "珠宝"
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
