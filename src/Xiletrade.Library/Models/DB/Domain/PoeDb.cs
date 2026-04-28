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
        StringBuilder url = new(Strings.UrlPoedbHost);
        var culture = Strings.Culture[dm.Config.Options.Language];
        var isPoe2 = dm.Config.Options.GameVersion is 1;
        var cul = culture is "en-US" ? "us/"
            : culture is "ko-KR" ? "kr/"
            : culture is "fr-FR" ? "fr/"
            : culture is "es-ES" ? "sp/"
            : culture is "de-DE" ? "de/"
            : culture is "pt-BR" ? "pt/"
            : culture is "ru-RU" ? "ru/"
            : culture is "th-TH" ? "th/"
            : culture is "zh-TW" ? "tw/"
            : culture is "zh-CN" ? "cn/"
            : culture is "ja-JP" ? "jp/"
            : "us/";
        url.Append(cul);

        var itemClass = string.Empty;

        if (item.Flag.Amulets || item.Flag.Rings || item.Flag.Belts
            || item.Flag.Quivers || item.Flag.Trinkets)
        {
            itemClass = item.Flag.GetItemClass();
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
            itemClass = item.Flag.BodyArmours ? "Body_Armours"
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
                        : itemClass;

            if (itemClass.Length > 0 && item.Flag.ArmourPiece)
            {
                if (item.Option[Resources.Resources.General055_Armour].Length > 0)
                {
                    itemClass += "_str";
                }
                if (item.Option[Resources.Resources.General057_Evasion].Length > 0)
                {
                    itemClass += "_dex";
                }
                if (item.Option[Resources.Resources.General056_Energy].Length > 0)
                {
                    itemClass += "_int";
                }
                if (item.Option[Resources.Resources.General095_Ward].Length > 0)
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

        }
        if (itemClass.Length is 0)
        {
            itemClass = item.TypeEn.Replace(" ", "_");
        }

        if (itemClass.Length is 0)
        {
            Link = url.Append("Modifiers").ToString();
        }
        Link = url.Append(itemClass).Append("#ModifiersCalc").ToString();
    }
}
