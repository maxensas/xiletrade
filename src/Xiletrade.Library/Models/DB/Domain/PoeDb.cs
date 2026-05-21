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

        var itemClass = item.Flag.BodyArmours ? "Body_Armours"
                        : item.Flag.Helmets ? "Helmets"
                        : item.Flag.Boots ? "Boots"
                        : item.Flag.Gloves ? "Gloves"
                        : item.Flag.Shield ? "Shields"
                        : item.Flag.Focus ? "Foci"
                        : item.Flag.Blueprints ? "Blueprints"
                        : item.Flag.Contracts ? "Contracts"
                        : item.Flag.LifeFlask ? "Life_Flasks"
                        : item.Flag.ManaFlask ? "Mana_Flasks"
                        : item.Flag.HybridFlask ? "Hybrid_Flasks"
                        : item.Flag.UtilityFlask ? "Utility_Flasks"
                        : item.Flag.Charm ? "Charms"
                        : item.Flag.Staff ? "Staves"
                        : item.Flag.Wand ? "Wands"
                        : item.Flag.Cobalt ? "Cobalt_Jewel"
                        : item.Flag.Crimson ? "Crimson_Jewel"
                        : item.Flag.Viridian ? "Viridian_Jewel"
                        : item.Flag.Ruby ? "Ruby"
                        : item.Flag.Emerald ? "Emerald"
                        : item.Flag.Sapphire ? "Sapphire"
                        : item.Flag.Prismatic ? "Prismatic_Jewel"
                        : item.Flag.Timeless ? "Timeless_Jewel"
                        : item.Flag.Murderous ? "Murderous_Eye_Jewel"
                        : item.Flag.Searching ? "Searching_Eye_Jewel"
                        : item.Flag.Hypnotic ? "Hypnotic_Eye_Jewel"
                        : item.Flag.Ghastly ? "Ghastly_Eye_Jewel"
                        : item.Flag.ClusterLarge ? "Large_Cluster_Jewel"
                        : item.Flag.ClusterMedium ? "Medium_Cluster_Jewel"
                        : item.Flag.ClusterSmall ? "Small_Cluster_Jewel"
                        : item.Flag.Sceptre ? "Sceptres"
                        : item.Flag.Claws ? "Claws"
                        : item.Flag.Daggers ? "Daggers"
                        : item.Flag.WandConvoking ? "Convoking_Wand"
                        : item.Flag.Wand ? "Wands"
                        : item.Flag.OneHandSwords ? "One_Hand_Swords"
                        : item.Flag.OneHandAxes ? "One_Hand_Axes"
                        : item.Flag.OneHandMaces ? "One_Hand_Maces"
                        : item.Flag.Spears ? "Spears"
                        : item.Flag.Flails ? "Flails"
                        : item.Flag.Bows ? "Bows"
                        : item.Flag.TwoHandSwords ? "Two_Hand_Swords"
                        : item.Flag.TwoHandAxes ? "Two_Hand_Axes"
                        : item.Flag.TwoHandMaces ? "Two_Hand_Maces"
                        : item.Flag.FishingRods ? "Fishing_Rods"
                        : item.Flag.Crossbows ? "Crossbows"
                        : item.Flag.Focus ? "Foci"
                        : item.Flag.Charm ? "Charms"
                        : item.Flag.ThrustingOneHandSwords ? "Thrusting_One_Hand_Swords"
                        : item.Flag.RuneDaggers ? "Rune_Daggers"
                        : item.Flag.Stave ? "Staves"
                        : item.Flag.Warstaff ? "Warstaves"
                        : item.Flag.QuarterStaff ? "Quarterstaves"
                        : string.Empty;

        if (itemClass.Length > 0 && item.Flag.ArmourPiece)
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
                if (item.Flag.Helmets)
                {
                    itemClass = "Runic_Crown";
                }
                if (item.Flag.Boots)
                {
                    itemClass = "Runic_Sabatons";
                }
                if (item.Flag.Gloves)
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
            if (item.Flag.Amulets || item.Flag.Rings || item.Flag.Belts
                || item.Flag.Quivers || item.Flag.Trinkets)
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
