using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

/// <summary>
/// Record used to instantiate item flags, recover item class and category used for trade api.
/// </summary>
public sealed record ItemFlag
{
    internal bool Unique { get; }
    internal bool Rare { get; }
    internal bool Magic { get; }
    internal bool Normal { get; }
    internal bool Currency { get; }
    internal bool Divcard { get; }
    internal bool Jewel { get; }
    internal bool Cluster { get; }
    internal bool Watchstone { get; }
    internal bool Invitation { get; }
    internal bool Facetor { get; }
    internal bool Chronicle { get; }
    internal bool Ultimatum { get; }
    internal bool UltimatumPoe2 { get; }
    internal bool FilledCoffin { get; }
    internal bool Logbook { get; }
    internal bool ChargedCompass { get; }
    internal bool Incubator { get; }
    internal bool Metamorph { get; }
    internal bool Voidstone { get; }
    internal bool MapFragment { get; }
    internal bool CapturedBeast { get; }
    internal bool ShowDetail { get; }
    internal bool Sentinel { get; }
    internal bool MirroredTablet { get; }
    internal bool MemoryLine { get; }
    internal bool SanctumResearch { get; }
    internal bool SanctumRelic { get; }
    internal bool AllflameEmber { get; }
    internal bool Corpses { get; }
    internal bool Rune { get; }
    internal bool TrialCoins { get; }
    internal bool Omen { get; }
    internal bool Socketable { get; }
    internal bool SkillGems { get; }
    internal bool SupportGems { get; }
    internal bool Gems { get; }
    internal bool Tablet { get; }
    internal bool Waystones { get; }
    internal bool StackableCurrency { get; }
    internal bool MiscMapItems { get; }
    internal bool DelveStackable { get; }
    internal bool Map { get; }
    internal bool Pieces { get; }
    internal bool Transfigured { get; }
    internal bool Unidentified { get; }
    internal bool Corrupted { get; }
    internal bool Mirrored { get; }
    internal bool Split { get; }
    internal bool FoilVariant { get; }
    internal bool ScourgedItem { get; }
    internal bool ItemLevel { get; }
    internal bool AreaLevel { get; }
    internal bool PinnacleKeys { get; }
    internal bool UncutGem { get; }
    internal bool VaultKeys { get; }
    internal bool Scarab { get; }
    internal bool Graft { get; }
    internal bool Wombgift { get; }

    //flasks-slots
    internal bool Flask { get; }
    internal bool UtilityFlask { get; }
    internal bool LifeFlask { get; }
    internal bool ManaFlask { get; }
    internal bool Tincture { get; }
    internal bool Charm { get; }

    //armours
    internal bool BodyArmours { get; }
    internal bool Boots { get; }
    internal bool Gloves { get; }
    internal bool Helmets { get; }
    //armours-offhands
    internal bool Shield { get; }
    internal bool Bucklers { get; }
    internal bool Focus { get; }
    //armours-group
    internal bool ArmourPiece { get; }

    //offhands
    internal bool Quivers { get; }

    //jewellery
    internal bool Belts { get; }
    internal bool Rings { get; }
    internal bool Amulets { get; }
    internal bool Trinkets { get; }

    //weapons
    internal bool Wand { get; }
    internal bool Sceptre { get; }
    internal bool Staff { get; }
    internal bool Warstaff { get; }
    internal bool QuarterStaff { get; }
    internal bool Spears { get; }
    internal bool Bows { get; }
    internal bool ThrustingOneHandSwords { get; }
    internal bool OneHandSwords { get; }
    internal bool TwoHandSwords { get; }
    internal bool OneHandMaces { get; }
    internal bool TwoHandMaces { get; }
    internal bool OneHandAxes { get; }
    internal bool TwoHandAxes { get; }
    internal bool Daggers { get; }
    internal bool RuneDaggers { get; }
    internal bool Claws { get; }
    internal bool FishingRods { get; }
    internal bool Crossbows { get; }
    internal bool Traps { get; }
    internal bool Flails { get; }
    //weapon-group
    internal bool Stave { get; }
    internal bool Weapon { get; }

    internal bool TwoRuneSocketable { get; } // exceptional not corrupted
    internal bool ThreeRuneSocketable { get; } // exceptional not corrupted

    internal bool Jewellery { get; }
    internal bool ByType { get; }

    // parameter only
    internal bool Area { get { return Chronicle || Ultimatum || Logbook || SanctumResearch || TrialCoins || MirroredTablet; } }

    /// <summary>
    /// Instantiate all item flags.
    /// </summary>
    public ItemFlag(InfoDescription infodesc, ReadOnlySpan<char> itemRarity, ReadOnlySpan<char> itemType, ReadOnlySpan<char> itemClass)
    {
        // using rarity
        Unique = itemRarity.SequenceEqual(Resources.Resources.General006_Unique);
        Rare = itemRarity.SequenceEqual(Resources.Resources.General007_Rare);
        Magic = itemRarity.SequenceEqual(Resources.Resources.General008_Magic);
        Normal = itemRarity.SequenceEqual(Resources.Resources.General009_Normal);
        Currency = itemRarity.SequenceEqual(Resources.Resources.General026_Currency);
        Divcard = itemRarity.SequenceEqual(Resources.Resources.General028_DivinationCard);

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
        MirroredTablet = itemType.Contain(Resources.Resources.General108_MirroredTablet);
        Ultimatum = itemType.Contain(Resources.Resources.ItemClass_inscribedUltimatum);
        
        // using item class
        UtilityFlask = itemClass.Contain(Resources.Resources.ItemClass_utilityFlask);
        LifeFlask = itemClass.Contain(Resources.Resources.ItemClass_lifeFlask);
        ManaFlask = itemClass.Contain(Resources.Resources.ItemClass_manaFlask);
        Charm = itemClass.Contain(Resources.Resources.ItemClass_charm);
        Flask = UtilityFlask || LifeFlask || ManaFlask;
        Jewel = itemClass.Contain(Resources.Resources.ItemClass_jewels);
        Voidstone = itemClass.Contain(Resources.Resources.ItemClass_atlas);
        MemoryLine = itemClass.Contain(Resources.Resources.ItemClass_memory);
        SanctumResearch = itemClass.Contain(Resources.Resources.ItemClass_sanctumResearch);
        SanctumRelic = itemClass.Contain(Resources.Resources.ItemClass_sanctumRelic);
        BodyArmours = itemClass.Contain(Resources.Resources.ItemClass_bodyArmours);
        Boots = itemClass.Contain(Resources.Resources.ItemClass_boots);
        Gloves = itemClass.Contain(Resources.Resources.ItemClass_gloves);
        Helmets = itemClass.Contain(Resources.Resources.ItemClass_helmets);
        Shield = itemClass.Contain(Resources.Resources.ItemClass_shields);
        Bucklers = itemClass.Contain(Resources.Resources.ItemClass_bucklers);
        Focus = itemClass.StartWith(Resources.Resources.ItemClass_foci);
        ArmourPiece = BodyArmours || Boots || Gloves || Helmets || Shield || Bucklers || Focus;
        Quivers = itemClass.Contain(Resources.Resources.ItemClass_quivers);
        Wand = itemClass.Contain(Resources.Resources.ItemClass_wand);
        Sceptre = itemClass.StartWith(Resources.Resources.ItemClass_sceptres);
        Staff = itemClass.Contain(Resources.Resources.ItemClass_staff);
        Warstaff = itemClass.Contain(Resources.Resources.ItemClass_warstaff);
        QuarterStaff = itemClass.StartWith(Resources.Resources.ItemClass_quarterstaves);
        Spears = itemClass.Contain(Resources.Resources.ItemClass_spears);
        ThrustingOneHandSwords = itemClass.Contain(Resources.Resources.ItemClass_thrustingOneHandSwords);
        Bows = itemClass.StartWith(Resources.Resources.ItemClass_bows);
        OneHandSwords = itemClass.StartWith(Resources.Resources.ItemClass_oneHandSwords);
        TwoHandSwords = itemClass.StartWith(Resources.Resources.ItemClass_twoHandSwords);
        OneHandMaces = itemClass.StartWith(Resources.Resources.ItemClass_oneHandMaces);
        TwoHandMaces = itemClass.StartWith(Resources.Resources.ItemClass_twoHandMaces);
        OneHandAxes = itemClass.StartWith(Resources.Resources.ItemClass_oneHandAxes);
        TwoHandAxes = itemClass.StartWith(Resources.Resources.ItemClass_twoHandAxes);
        Daggers = itemClass.StartWith(Resources.Resources.ItemClass_daggers);
        RuneDaggers = itemClass.StartWith(Resources.Resources.ItemClass_runeDaggers);
        Claws = itemClass.StartWith(Resources.Resources.ItemClass_claws);
        FishingRods = itemClass.StartWith(Resources.Resources.ItemClass_fishingRods);
        Crossbows = itemClass.StartWith(Resources.Resources.ItemClass_crossbows);
        Traps = itemClass.StartWith(Resources.Resources.ItemClass_traps);
        Flails = itemClass.StartWith(Resources.Resources.ItemClass_flails);
        Stave = Staff || Warstaff || QuarterStaff;
        Weapon = Wand || Sceptre || Staff || Warstaff || QuarterStaff
            || Spears || Bows || ThrustingOneHandSwords || OneHandSwords || TwoHandSwords || OneHandMaces
            || TwoHandMaces || OneHandAxes || TwoHandAxes || Daggers || RuneDaggers
            || Claws || FishingRods || Crossbows || Traps || Flails;
        Sentinel = itemClass.Contain(Resources.Resources.ItemClass_sentinel);
        Tincture = itemClass.Contain(Resources.Resources.ItemClass_tincture);
        Corpses = itemClass.Contain(Resources.Resources.ItemClass_corpses);
        Logbook = itemClass.StartWith(Resources.Resources.ItemClass_expeditionLogbooks)
            || itemType.Contain(Resources.Resources.General094_Logbook);
        TrialCoins = itemClass.StartWith(Resources.Resources.ItemClass_trialCoins);
        Omen = itemClass.StartWith(Resources.Resources.ItemClass_omen);
        Socketable = itemClass.StartWith(Resources.Resources.ItemClass_socketable);
        SkillGems = itemClass.StartWith(Resources.Resources.ItemClass_skillGems);
        SupportGems = itemClass.StartWith(Resources.Resources.ItemClass_supportGems);
        UncutGem = itemClass.Contain(Resources.Resources.General151_UncutSpiritGem)
            || itemClass.Contain(Resources.Resources.General152_UncutSkillGem)
            || itemClass.Contain(Resources.Resources.General153_UncutSupportGem);
        Gems = SkillGems || SupportGems;
        Tablet = itemClass.StartWith(Resources.Resources.ItemClass_tablet);
        Waystones = itemClass.StartWith(Resources.Resources.ItemClass_waystones);
        Map = itemClass.StartWith(Resources.Resources.ItemClass_maps);
        Rings = itemClass.StartWith(Resources.Resources.ItemClass_rings);
        Amulets = itemClass.StartWith(Resources.Resources.ItemClass_amulets);
        Belts = itemClass.StartWith(Resources.Resources.ItemClass_belts);
        Trinkets = itemClass.StartWith(Resources.Resources.ItemClass_trinkets);
        StackableCurrency = itemClass.StartWith(Resources.Resources.ItemClass_stackableCurrency);
        MiscMapItems = itemClass.StartWith(Resources.Resources.ItemClass_miscMapItems);
        DelveStackable = itemClass.StartWith(Resources.Resources.ItemClass_delveStackable);
        Pieces = itemClass.StartWith(Resources.Resources.ItemClass_pieces);
        UltimatumPoe2 = itemClass.Contain(Resources.Resources.ItemClass_inscribedUltimatum);
        PinnacleKeys = itemClass.Contain(Resources.Resources.ItemClass_pinnacleKeys);
        VaultKeys = itemClass.Contain(Resources.Resources.ItemClass_vaultKeys);
        MapFragment = itemClass.Contain(Resources.Resources.ItemClass_mapFragments);
        Graft = itemClass.Contain(Resources.Resources.ItemClass_grafts);
        Wombgift = itemClass.Contain(Resources.Resources.ItemClass_wombgifts);

        AllflameEmber = itemClass.Contain(Resources.Resources.ItemClass_allflame) 
            || MapFragment && itemType.Contains(Resources.Resources.General165_AllflameEmber, StringComparison.OrdinalIgnoreCase);
        Scarab = MapFragment && itemType.Contains(Resources.Resources.General164_Scarab, StringComparison.OrdinalIgnoreCase);

        Jewellery = Amulets || Rings || Belts || Trinkets;
        ByType = Jewellery || Weapon || ArmourPiece || Quivers;

        TwoRuneSocketable = Wand || Daggers || RuneDaggers || Claws || OneHandAxes 
            || OneHandMaces || OneHandSwords || Sceptre || Spears || Flails || Staff
            || Boots || Gloves || Helmets || Shield || Bucklers || Focus;
        ThreeRuneSocketable = Warstaff || QuarterStaff || Bows || TwoHandSwords 
            || TwoHandMaces || TwoHandAxes || Crossbows || BodyArmours || Traps;

        // using clipdata
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
            if (!Corrupted)
            {
                Corrupted = line.Equal(Resources.Resources.General037_Corrupt);
            }
            if (!Mirrored)
            {
                Mirrored = line.Equal(Resources.Resources.General109_Mirrored);
            }
            if (!Split)
            {
                Split = line.Equal(Resources.Resources.General157_Split);
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
                    || line.Contain(Resources.Resources.General143_WaystoneTier);
            }
            if (!AreaLevel)
            {
                AreaLevel = line.Contain(Resources.Resources.General067_AreaLevel);
            }
        }
        
        ShowDetail = Gems || Divcard || AllflameEmber || MiscMapItems && !Ultimatum
            || MapFragment && !Invitation && !Chronicle && !Ultimatum && !MirroredTablet
            || Currency && !Chronicle && !Ultimatum && !MirroredTablet && !FilledCoffin;
    }

    /// <summary>
    /// Get item category used for trade api.
    /// </summary>
    /// <returns></returns>
    internal string GetItemCategoryApi()
    {
        return 
            //weapon
            Sceptre ? "weapon.sceptre" : QuarterStaff ? "weapon.warstaff" 
            : Stave ? "weapon.staff" : Wand ? "weapon.wand" : Spears ? "weapon.spear"
            : ThrustingOneHandSwords ? "weapon.rapier" : OneHandSwords ? "weapon.onesword"
            : OneHandAxes ? "weapon.oneaxe" : OneHandMaces ? "weapon.onemace"
            : TwoHandAxes ? "weapon.twoaxe" : TwoHandMaces ? "weapon.twomace"
            : TwoHandSwords ? "weapon.twosword" : Claws ? "weapon.claw"
            : RuneDaggers ? "weapon.runedagger" : Daggers ? "weapon.basedagger"
            : Bows ? "weapon.bow" : FishingRods ? "weapon.rod" : Crossbows ? "weapon.crossbow"
            : Traps ? "weapon.trap" : Flails ? "weapon.flail" : Weapon ? "weapon" 
            //armour
            : BodyArmours ? "armour.chest" : Helmets ? "armour.helmet"
            : Boots ? "armour.boots" : Gloves ? "armour.gloves"
            : Shield ? "armour.shield" : Bucklers ? "armour.buckler" 
            : Focus ? "armour.focus" : Quivers ? "armour.quiver" : ArmourPiece ? "armour"
            //accessory
            : Amulets ? "accessory.amulet" : Rings ? "accessory.ring" 
            : Belts ? "accessory.belt" : Trinkets ? "accessory.trinket"
            //map
            : Tablet ? "map.tablet" : Waystones ? "map.waystone" : TrialCoins ? "map.barya"
            : MapFragment ? "map.fragment" : MiscMapItems ? string.Empty : Map ? "map"
            //jewel
            : Cluster ? "jewel.cluster" : Jewel ? "jewel"
            //other
            : Divcard ? "card" : MemoryLine ? "memoryline" : CapturedBeast ? "monster.beast"
            : Flask ? "flask" : Gems ? "gem" : Sentinel ? "sentinel" : Tincture ? "tincture"
            : SanctumRelic ? "sanctum.relic" : SanctumResearch ? "sanctum.research" : Corpses ? "corpse"
            : Wombgift ? "wombgift" : Graft ? "graft"
            : Ultimatum || PinnacleKeys || Charm || Logbook || UncutGem || VaultKeys ? string.Empty
            : Pieces ? "currency.piece" : Currency || StackableCurrency ? "currency"
            : string.Empty; 
    }

    /// <summary>
    /// Get item class.
    /// </summary>
    /// <returns></returns>
    internal string GetItemClass()
    {
        return
            //weapon
            Sceptre ? "Sceptre" : QuarterStaff ? "QuarterStaff"
            : Stave ? "Stave" : Wand ? "Wand" : Spears ? "Spears"
            : ThrustingOneHandSwords ? "Thrusting One Hand Swords" : OneHandSwords ? "One Hand Swords"
            : OneHandAxes ? "One Hand Axes" : OneHandMaces ? "One Hand Maces"
            : TwoHandAxes ? "Two Hand Axes" : TwoHandMaces ? "Two Hand Maces"
            : TwoHandSwords ? "Two Hand Swords" : Claws ? "Claws"
            : RuneDaggers ? "Rune Daggers" : Daggers ? "Daggers"
            : Bows ? "Bows" : FishingRods ? "Fishing Rods" : Crossbows ? "Crossbows"
            : Traps ? "Traps" : Flails ? "Flails" : Weapon ? "Weapon"
            //armour
            : BodyArmours ? "Body Armours" : Helmets ? "Helmets"
            : Boots ? "Boots" : Gloves ? "Gloves"
            : Shield ? "Shield" : Bucklers ? "Bucklers"
            : Focus ? "Focus" : Quivers ? "Quivers" : ArmourPiece ? "Armour"
            //accessory
            : Amulets ? "Amulets" : Rings ? "Rings"
            : Belts ? "Belts" : Trinkets ? "Trinkets"
            //map
            : Tablet ? "Tablet" : Waystones ? "Waystones"
            : MapFragment ? "Map Fragment" : MiscMapItems ? "Misc Map Items" : Map ? "Map"
            //jewel
            : Cluster ? "Cluster jewel" : Jewel ? "Jewel"
            //currency
            : Pieces ? "Pieces" : Currency ? "Currency" : StackableCurrency ? "Stackable Currency"
            //other
            : Divcard ? "Divination card" : MemoryLine ? "MemoryLine" : CapturedBeast ? "Captured Beast"
            : Flask ? "Flask" : Logbook ? "Logbook" : Gems ? "Gems"
            : Sentinel ? "Sentinel" : Charm ? "Charm" : Tincture ? "Tincture"
            : SanctumRelic ? "Sanctum Relic" : SanctumResearch ? "Sanctum Research"
            : string.Empty;
    }
}
