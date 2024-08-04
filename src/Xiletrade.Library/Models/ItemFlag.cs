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
    //internal bool Prophecy { get; private set; }
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

    internal bool Wand { get; private set; } // TODO : update with all weapon item class
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
        Unique = itemRarity == Resources.Resources.General006_Unique;
        Rare = itemRarity == Resources.Resources.General007_Rare;
        Magic = itemRarity == Resources.Resources.General008_Magic;
        Normal = itemRarity == Resources.Resources.General009_Normal;
        Gem = itemRarity == Resources.Resources.General029_Gem;
        Currency = itemRarity == Resources.Resources.General026_Currency;
        Divcard = itemRarity == Resources.Resources.General028_DivinationCard;

        Jewel = IsJewel(itemType, idLang);
        Cluster = itemType.Contains(Resources.Resources.General022_Cluster, StringComparison.Ordinal);
        Watchstone = itemType.Contains(Resources.Resources.General062_Watchstone, StringComparison.Ordinal);
        Invitation = itemType.Contains(Resources.Resources.General063_Invitation, StringComparison.Ordinal);
        Facetor = itemType.Contains(Resources.Resources.General064_FacetorLens, StringComparison.Ordinal);
        Chronicle = itemType.Contains(Resources.Resources.General065_ChronicleAtzoatl, StringComparison.Ordinal);
        Ultimatum = itemType.Contains(Resources.Resources.General066_InscribedUltimatum, StringComparison.Ordinal); // change to : Engraved Ultimatum
        FilledCoffin = itemType.Contains(Resources.Resources.General127_FilledCoffin, StringComparison.Ordinal); // ONLY IN ENGLISH FOR NOW
        Rune = itemType.Contains(Resources.Resources.General132_Rune, StringComparison.Ordinal);
        Logbook = itemType.Contains(Resources.Resources.General094_Logbook, StringComparison.Ordinal);
        ChargedCompass = itemType.Contains(Resources.Resources.General105_ChargedCompass, StringComparison.Ordinal);
        Incubator = itemType.Contains(Resources.Resources.General027_Incubator, StringComparison.Ordinal);
        ScourgedMap = itemType.Contains(Resources.Resources.General103_Scourged, StringComparison.Ordinal);
        MirroredTablet = itemType.Contains(Resources.Resources.General108_MirroredTablet, StringComparison.Ordinal);

        Wand = itemClass.Contains(Resources.Resources.ItemClass_wand, StringComparison.Ordinal);

        Metamorph = itemClass.Contains(Resources.Resources.ItemClass_metamorphSample, StringComparison.Ordinal);
        Voidstone = itemClass.Contains(Resources.Resources.ItemClass_atlas, StringComparison.Ordinal);
        MemoryLine = itemClass.Contains(Resources.Resources.ItemClass_memory, StringComparison.Ordinal);
        SanctumResearch = itemClass.Contains(Resources.Resources.ItemClass_sanctumResearch, StringComparison.Ordinal);
        SanctumRelic = itemClass.Contains(Resources.Resources.ItemClass_sanctumRelic, StringComparison.Ordinal);
        MapFragment = itemClass.Contains(Resources.Resources.ItemClass_mapFragments, StringComparison.Ordinal)
            || clipData[^1].Contains(Resources.Resources.General050_ChkMapFragment1, StringComparison.Ordinal)
            || clipData[^1].Contains(Resources.Resources.General051_ChkMapFragment2, StringComparison.Ordinal);
        //Prophecy = clipData[^1].Contains(Resources.Resources.General049_ChkProphecy, StringComparison.Ordinal);
        Flask = clipData[^1].Contains(Resources.Resources.General053_ChkFlask, StringComparison.Ordinal);
        CapturedBeast = clipData[^1].Contains(Resources.Resources.General054_ChkBeast, StringComparison.Ordinal);

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
