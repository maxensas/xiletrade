using System;
using System.Linq;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

/// <summary>
/// Record used to instantiate item flags, recover item class and category used for trade api.
/// </summary>
public sealed record ItemFlag
{
    internal ItemFlagRarity Rarity { get; }
    internal ItemFlagJewel Jewel { get; }
    internal ItemFlagWeapon Weapon { get; }
    internal ItemFlagOffhand Offhand { get; }
    internal ItemFlagArmour Armour { get; }
    internal ItemFlagSocket Socket { get; }
    internal ItemFlagJewellery Jewellery { get; }
    internal ItemFlagSlot Slot { get; }
    internal ItemFlagMap Map { get; }
    internal ItemFlagGem Gem { get; }
    internal ItemFlagArea Area { get; }
    internal ItemFlagTag Tag { get; }

    // using item type only
    internal bool Invitation { get; }
    internal bool Facetor { get; }
    internal bool Rune { get; }
    internal bool Incubator { get; }
    internal bool Vial { get; }
    internal bool MercenaryWarrant { get; }
    internal bool ScryingOrb { get; }

    // using item class only
    internal bool Currency { get; }
    internal bool Divcard { get; }
    internal bool UltimatumPoe2 { get; }
    internal bool Metamorph { get; }
    internal bool Voidstone { get; }
    internal bool Sentinel { get; }
    internal bool MemoryLine { get; }
    internal bool SanctumRelic { get; }
    internal bool Corpses { get; }
    internal bool Omen { get; }
    internal bool Tablet { get; }
    internal bool Waystones { get; }
    internal bool StackableCurrency { get; }
    internal bool DelveStackable { get; }
    internal bool Pieces { get; }
    internal bool PinnacleKeys { get; }
    internal bool VaultKeys { get; }
    internal bool Graft { get; }
    internal bool Wombgift { get; }
    internal bool Blueprints { get; }
    internal bool Contracts { get; }
    internal bool Breachstone { get; }

    // other flags
    internal bool AllflameEmber { get; }
    internal bool Scarab { get; }

    /// <summary>
    /// Instantiate all item flags.
    /// </summary>
    public ItemFlag(InfoDescription infodesc, ItemHeader header)
    {
        var itemRarity = header.Rarity.AsSpan();
        var itemType = header.Type.AsSpan();
        var itemClass = header.Class.AsSpan();

        // using rarity
        Currency = itemRarity.SequenceEqual(Resources.Resources.General026_Currency);
        Divcard = itemRarity.SequenceEqual(Resources.Resources.General028_DivinationCard);

        // using item type
        Invitation = itemType.Contain(Resources.Resources.General063_Invitation);
        Facetor = itemType.Contain(Resources.Resources.General064_FacetorLens);
        Rune = itemType.Contain(Resources.Resources.General132_Rune);
        Incubator = itemType.Contain(Resources.Resources.General027_Incubator);
        Vial = itemType.Contain(Resources.Resources.General217_Vial);
        MercenaryWarrant = itemType.Contain(Resources.Resources.General240_MercenaryWarrant);
        ScryingOrb = itemType.Contain(Resources.Resources.General245_ScryingOrb);

        // using item class
        Voidstone = itemClass.Contain(Resources.Resources.ItemClass_atlas);
        MemoryLine = itemClass.Contain(Resources.Resources.ItemClass_memory);
        SanctumRelic = itemClass.Contain(Resources.Resources.ItemClass_sanctumRelic);
        Sentinel = itemClass.Contain(Resources.Resources.ItemClass_sentinel);
        Corpses = itemClass.Contain(Resources.Resources.ItemClass_corpses);
        Omen = itemClass.StartWith(Resources.Resources.ItemClass_omen);
        Tablet = itemClass.StartWith(Resources.Resources.ItemClass_tablet);
        Waystones = itemClass.StartWith(Resources.Resources.ItemClass_waystones);
        StackableCurrency = itemClass.StartWith(Resources.Resources.ItemClass_stackableCurrency);
        DelveStackable = itemClass.StartWith(Resources.Resources.ItemClass_delveStackable);
        Pieces = itemClass.StartWith(Resources.Resources.ItemClass_pieces);
        UltimatumPoe2 = itemClass.Contain(Resources.Resources.ItemClass_inscribedUltimatum);
        PinnacleKeys = itemClass.Contain(Resources.Resources.ItemClass_pinnacleKeys);
        VaultKeys = itemClass.Contain(Resources.Resources.ItemClass_vaultKeys);
        Graft = itemClass.Contain(Resources.Resources.ItemClass_grafts);
        Wombgift = itemClass.Contain(Resources.Resources.ItemClass_wombgifts);
        Blueprints = itemClass.Contain(Resources.Resources.ItemClass_blueprints);
        Contracts = itemClass.Contain(Resources.Resources.ItemClass_contracts);
        Breachstone = itemClass.Contain(Resources.Resources.ItemClass_breachstones);

        Rarity = new(itemRarity);
        Jewel = new(itemClass, itemType);
        Weapon = new(itemClass, itemType);
        Offhand = new(itemClass);
        Armour = new(Offhand, itemClass);
        Socket = new(itemClass, Armour, Weapon, Offhand);
        Jewellery = new(itemClass);
        Slot = new(itemClass);
        Map = new(itemClass, itemType, Divcard);
        Gem = new(itemClass);
        Area = new(itemClass, itemType);
        Tag = new(infodesc, Gem);

        // other flags
        AllflameEmber = itemClass.Contain(Resources.Resources.ItemClass_allflame) || Map.Fragment 
            && itemType.Contains(Resources.Resources.General165_AllflameEmber, StringComparison.OrdinalIgnoreCase);
        Scarab = Map.Fragment 
            && itemType.Contains(Resources.Resources.General164_Scarab, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get item category used for trade api.
    /// </summary>
    /// <returns></returns>
    internal string GetItemCategoryApi()
    {
        return
            //weapon
            Weapon.Sceptre ? "weapon.sceptre" : Weapon.QuarterStaff ? "weapon.warstaff" 
            : Weapon.IsStave ? "weapon.staff" : Weapon.Wand ? "weapon.wand" : Weapon.Spears ? "weapon.spear"
            : Weapon.ThrustingOneHandSwords ? "weapon.rapier" : Weapon.OneHandSwords ? "weapon.onesword"
            : Weapon.OneHandAxes ? "weapon.oneaxe" : Weapon.OneHandMaces ? "weapon.onemace"
            : Weapon.TwoHandAxes ? "weapon.twoaxe" : Weapon.TwoHandMaces ? "weapon.twomace"
            : Weapon.TwoHandSwords ? "weapon.twosword" : Weapon.Claws ? "weapon.claw"
            : Weapon.RuneDaggers ? "weapon.runedagger" : Weapon.Daggers ? "weapon.basedagger"
            : Weapon.Bows ? "weapon.bow" : Weapon.FishingRods ? "weapon.rod" : Weapon.Crossbows ? "weapon.crossbow"
            : Weapon.Traps ? "weapon.trap" : Weapon.Flails ? "weapon.flail" : Weapon.Talismans ? "weapon.talisman" 
            : Weapon.IsWeapon ? "weapon" 
            //armour
            : Armour.BodyArmours ? "armour.chest" : Armour.Helmets ? "armour.helmet"
            : Armour.Boots ? "armour.boots" : Armour.Gloves ? "armour.gloves"
            : Offhand.Shield ? "armour.shield" : Offhand.Bucklers ? "armour.buckler" 
            : Offhand.Focus ? "armour.focus" : Offhand.Quivers ? "armour.quiver" : Armour.IsArmour ? "armour"
            //accessory
            : Jewellery.Amulets ? "accessory.amulet" : Jewellery.Rings ? "accessory.ring" 
            : Jewellery.Belts ? "accessory.belt" : Jewellery.Trinkets ? "accessory.trinket"
            //map
            : Tablet ? "map.tablet" : Waystones ? "map.waystone" : Area.TrialCoins ? "map.barya" : Map.Chart ? "chart"
            : MercenaryWarrant ? string.Empty
            : Map.Fragment ? "map.fragment" : Contracts ? "heistmission.contract" : Blueprints ? "heistmission.blueprint"
            : Map.Misc ? string.Empty : Map.IsMap ? "map"
            //jewel
            : Jewel.Cluster ? "jewel.cluster" : Jewel.IsJewel ? "jewel"
            //other
            : Divcard ? "card" : MemoryLine ? "memoryline" : Tag.CapturedBeast ? "monster.beast"
            : Slot.Flask ? "flask" : Gem.IsGem ? "gem" : Sentinel ? "sentinel" : Slot.Tincture ? "tincture"
            : SanctumRelic ? "sanctum.relic" : Area.SanctumResearch ? "sanctum.research" : Corpses ? "corpse"
            : Wombgift ? "wombgift" : Graft ? "graft"
            : Area.Ultimatum || PinnacleKeys || Slot.Charm || Area.Logbook || Gem.Uncut || VaultKeys ? string.Empty
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
            Weapon.Sceptre ? "Sceptre" : Weapon.QuarterStaff ? "QuarterStaff"
            : Weapon.IsStave ? "Stave" : Weapon.Wand ? "Wand" : Weapon.Spears ? "Spears"
            : Weapon.ThrustingOneHandSwords ? "Thrusting One Hand Swords" : Weapon.OneHandSwords ? "One Hand Swords"
            : Weapon.OneHandAxes ? "One Hand Axes" : Weapon.OneHandMaces ? "One Hand Maces"
            : Weapon.TwoHandAxes ? "Two Hand Axes" : Weapon.TwoHandMaces ? "Two Hand Maces"
            : Weapon.TwoHandSwords ? "Two Hand Swords" : Weapon.Claws ? "Claws"
            : Weapon.RuneDaggers ? "Rune Daggers" : Weapon.Daggers ? "Daggers"
            : Weapon.Bows ? "Bows" : Weapon.FishingRods ? "Fishing Rods" : Weapon.Crossbows ? "Crossbows"
            : Weapon.Traps ? "Traps" : Weapon.Flails ? "Flails" : Weapon.Talismans ? "Talismans" : Weapon.IsWeapon ? "Weapon"
            //armour
            : Armour.BodyArmours ? "Body Armours" : Armour.Helmets ? "Helmets"
            : Armour.Boots ? "Boots" : Armour.Gloves ? "Gloves"
            : Offhand.Shield ? "Shield" : Offhand.Bucklers ? "Bucklers"
            : Offhand.Focus ? "Focus" : Offhand.Quivers ? "Quivers" : Armour.IsArmour ? "Armour"
            //accessory
            : Jewellery.Amulets ? "Amulets" : Jewellery.Rings ? "Rings"
            : Jewellery.Belts ? "Belts" : Jewellery.Trinkets ? "Trinkets"
            //map
            : Tablet ? "Tablet" : Waystones ? "Waystones"
            : Blueprints ? "Blueprints" : Contracts ? "Contracts" : Map.Chart ? "Chart"
            : Map.Fragment ? "Map Fragment" : Map.Misc ? "Misc Map Items" : Map.IsMap ? "Map"
            //jewel
            : Jewel.Cluster ? "Cluster jewel" : Jewel.IsJewel ? "Jewel"
            //currency
            : Pieces ? "Pieces" : Currency ? "Currency" : StackableCurrency ? "Stackable Currency"
            //other
            : Divcard ? "Divination card" : MemoryLine ? "MemoryLine" : Tag.CapturedBeast ? "Captured Beast"
            : Slot.Flask ? "Flask" : Area.Logbook ? "Logbook" : Gem.IsGem ? "Gems"
            : Sentinel ? "Sentinel" : Slot.Charm ? "Charm" : Slot.Tincture ? "Tincture"
            : SanctumRelic ? "Sanctum Relic" : Area.SanctumResearch ? "Sanctum Research"
            : string.Empty;
    }
}
