using System.Text;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.DB.Domain;

internal sealed class PoeDb
{
    internal string Link { get; private set; }
    
    internal PoeDb(DataManagerService dm, ItemData item)
    {
        var url = new StringBuilder(Strings.UrlPoedbHost);
        var idLang = dm.Config.Options.Language;
        var sufLang = idLang is 0 ? "us/"
            : idLang is 1 ? "kr/"
            : idLang is 2 ? "fr/"
            : idLang is 3 ? "sp/"
            : idLang is 4 ? "de/"
            : idLang is 5 ? "pt/"
            : idLang is 6 ? "ru/"
            : idLang is 7 ? "th/"
            : idLang is 8 ? "tw/"
            : idLang is 9 ? "cn/"
            : idLang is 10 ? "jp/"
            : "us/";

        url.Append(sufLang);

        var itemClass = item.Flag.Armour.BodyArmours ? "Body_Armours"
                        : item.Flag.Armour.Helmets ? "Helmets"
                        : item.Flag.Armour.Boots ? "Boots"
                        : item.Flag.Armour.Gloves ? "Gloves"
                        : item.Flag.Offhand.Shield ? "Shields"
                        : item.Flag.Offhand.Focus ? "Foci"
                        : item.Flag.Blueprints ? "Blueprints"
                        : item.Flag.Contracts ? "Contracts"
                        : item.Flag.Slot.LifeFlask ? "Life_Flasks"
                        : item.Flag.Slot.ManaFlask ? "Mana_Flasks"
                        : item.Flag.Slot.HybridFlask ? "Hybrid_Flasks"
                        : item.Flag.Slot.UtilityFlask ? "Utility_Flasks"
                        : item.Flag.Slot.Charm ? "Charms"
                        : item.Flag.Weapon.Staff ? "Staves"
                        : item.Flag.Weapon.Wand ? "Wands"
                        : item.Flag.Jewel.Cobalt ? "Cobalt_Jewel"
                        : item.Flag.Jewel.Crimson ? "Crimson_Jewel"
                        : item.Flag.Jewel.Viridian ? "Viridian_Jewel"
                        : item.Flag.Jewel.Ruby ? "Ruby"
                        : item.Flag.Jewel.Emerald ? "Emerald"
                        : item.Flag.Jewel.Sapphire ? "Sapphire"
                        : item.Flag.Jewel.Prismatic ? "Prismatic_Jewel"
                        : item.Flag.Jewel.Timeless ? "Timeless_Jewel"
                        : item.Flag.Jewel.Murderous ? "Murderous_Eye_Jewel"
                        : item.Flag.Jewel.Searching ? "Searching_Eye_Jewel"
                        : item.Flag.Jewel.Hypnotic ? "Hypnotic_Eye_Jewel"
                        : item.Flag.Jewel.Ghastly ? "Ghastly_Eye_Jewel"
                        : item.Flag.Jewel.ClusterLarge ? "Large_Cluster_Jewel"
                        : item.Flag.Jewel.ClusterMedium ? "Medium_Cluster_Jewel"
                        : item.Flag.Jewel.ClusterSmall ? "Small_Cluster_Jewel"
                        : item.Flag.Weapon.Sceptre ? "Sceptres"
                        : item.Flag.Weapon.Claws ? "Claws"
                        : item.Flag.Weapon.Daggers ? "Daggers"
                        : item.Flag.Weapon.WandConvoking ? "Convoking_Wand"
                        : item.Flag.Weapon.Wand ? "Wands"
                        : item.Flag.Weapon.OneHandSwords ? "One_Hand_Swords"
                        : item.Flag.Weapon.OneHandAxes ? "One_Hand_Axes"
                        : item.Flag.Weapon.OneHandMaces ? "One_Hand_Maces"
                        : item.Flag.Weapon.Spears ? "Spears"
                        : item.Flag.Weapon.Flails ? "Flails"
                        : item.Flag.Weapon.Bows ? "Bows"
                        : item.Flag.Weapon.TwoHandSwords ? "Two_Hand_Swords"
                        : item.Flag.Weapon.TwoHandAxes ? "Two_Hand_Axes"
                        : item.Flag.Weapon.TwoHandMaces ? "Two_Hand_Maces"
                        : item.Flag.Weapon.FishingRods ? "Fishing_Rods"
                        : item.Flag.Weapon.Crossbows ? "Crossbows"
                        : item.Flag.Offhand.Focus ? "Foci"
                        : item.Flag.Slot.Charm ? "Charms"
                        : item.Flag.Weapon.ThrustingOneHandSwords ? "Thrusting_One_Hand_Swords"
                        : item.Flag.Weapon.RuneDaggers ? "Rune_Daggers"
                        : item.Flag.Weapon.IsStave ? "Staves"
                        : item.Flag.Weapon.Warstaff ? "Warstaves"
                        : item.Flag.Weapon.QuarterStaff ? "Quarterstaves"
                        : string.Empty;

        if (itemClass.Length > 0 && item.Flag.Armour.IsArmour)
        {
            if (item.Options.Armour.Length > 0)
            {
                itemClass += "_str";
            }
            if (item.Options.Evasion.Length > 0)
            {
                itemClass += "_dex";
            }
            if (item.Options.Energy.Length > 0)
            {
                itemClass += "_int";
            }
            if (item.Options.Ward.Length > 0)
            {
                if (item.Flag.Armour.Helmets)
                {
                    itemClass = "Runic_Crown";
                }
                if (item.Flag.Armour.Boots)
                {
                    itemClass = "Runic_Sabatons";
                }
                if (item.Flag.Armour.Gloves)
                {
                    itemClass = "Runic_Gauntlets";
                }
            }
        }
        if (item.Flag.Waystones)
        {
            var match = RegexUtil.DecimalNoPlusPattern().Matches(item.TypeEn);
            if (match.Count is 1 && int.TryParse(match[0].Value, out int val)) // ex: currenItem.TypeEn "Waystone (Tier 14)"
            {
                if (val < 6)
                {
                    itemClass = "Waystones_low_tier";
                }
                if (val >= 6 && val < 11)
                {
                    itemClass = "Waystones_mid_tier";
                }
                if (val >= 11)
                {
                    itemClass = "Waystones_top_tier";
                }
            }
        }
        if (itemClass.Length is 0)
        {
            if (item.Flag.Jewellery.IsJewellery || item.Flag.Offhand.Quivers)
            {
                itemClass = item.Flag.GetItemClass();
            }
        }
        if (itemClass.Length is 0)
        {
            itemClass = item.TypeEn.Replace(" ", "_");
        }
        if (itemClass.Length is 0)
        {
            Link = url.Append("Modifiers").ToString();
            return;
        }
        Link = url.Append(itemClass).Append("#ModifiersCalc").ToString();
    }
}
