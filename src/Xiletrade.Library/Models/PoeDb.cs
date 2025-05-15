using System.Text;
using Xiletrade.Library.Models.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

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
        var inheritSplit = item.Inherits.Split('/');
        var inheritOne = inheritSplit.Length > 0 ? inheritSplit[0] : string.Empty;
        var inheritTwo = inheritSplit.Length > 1 ? inheritSplit[1] : string.Empty;
        var inheritThree = inheritSplit.Length > 2 ? inheritSplit[2] : string.Empty;
        var inheritFour = inheritSplit.Length > 3 ? inheritSplit[3] : string.Empty;

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
            itemClass = inheritTwo is "BodyArmours" ? "Body_Armours"
                        : inheritTwo is "Helmets" ? "Helmets"
                        : inheritTwo is "Boots" ? "Boots"
                        : inheritTwo is "Gloves" ? "Gloves"
                        : inheritTwo is "Shields" ? "Shields"
                        : inheritTwo is "Focii" ? "Foci"
                        : inheritTwo is "HeistBlueprint" ? "Blueprints"
                        : inheritTwo is "HeistContract" ? "Contracts"
                        : inheritTwo is "AbstractLifeFlask" ? "Life_Flasks"
                        : inheritTwo is "AbstractManaFlask" ? "Mana_Flasks"
                        : inheritTwo is "AbstractHybridFlask" ? "Hybrid_Flasks"
                        : inheritTwo is "AbstractUtilityFlask" ? isPoe2 ? "Charms" : "Utility_Flasks"
                        : inheritTwo is "AbstractStaff" ? "Staves"
                        : inheritTwo is "AbstractWand" ? "Wands"
                        : inheritTwo is "JewelStr" ? isPoe2 ? "Ruby" : "Crimson_Jewel"
                        : inheritTwo is "JewelDex" ? isPoe2 ? "Emerald" : "Viridian_Jewel"
                        : inheritTwo is "JewelInt" ? isPoe2 ? "Sapphire" : "Cobalt_Jewel"
                        : inheritTwo is "JewelPrismatic" ? "Prismatic_Jewel"
                        : inheritTwo is "JewelAbyssMelee" ? "Murderous_Eye_Jewel"
                        : inheritTwo is "JewelAbyssRanged" ? "Searching_Eye_Jewel"
                        : inheritTwo is "JewelAbyssCaster" ? "Hypnotic_Eye_Jewel"
                        : inheritTwo is "JewelAbyssSummoner" ? "Ghastly_Eye_Jewel"
                        : inheritTwo is "JewelTimeless" ? "Timeless_Jewel"
                        : inheritTwo is "JewelPassiveTreeExpansionLarge" ? "Large_Cluster_Jewel"
                        : inheritTwo is "JewelPassiveTreeExpansionMedium" ? "Medium_Cluster_Jewel"
                        : inheritTwo is "JewelPassiveTreeExpansionSmall" ? "Small_Cluster_Jewel"
                        : itemClass;

            if (itemClass.Length > 0 && inheritOne is "Armours")
            {
                if (inheritThree.Contain("StrDexInt")) itemClass += "_str_dex_int"; // cascade neeeded
                else if (inheritThree.Contain("StrDex")) itemClass += "_str_dex";
                else if (inheritThree.Contain("StrInt")) itemClass += "_str_int";
                else if (inheritThree.Contain("DexInt")) itemClass += "_dex_int";
                else if (inheritThree.Contain("Str")) itemClass += "_str";
                else if (inheritThree.Contain("Dex")) itemClass += "_dex";
                else if (inheritThree.Contain("Int")) itemClass += "_int";
                else if (inheritThree.Contain("HelmetExpedition")) itemClass = "Runic_Crown";
                else if (inheritThree.Contain("BootsExpedition")) itemClass = "Runic_Sabatons";
                else if (inheritThree.Contain("GlovesExpedition")) itemClass = "Runic_Gauntlets";
            }
            if (itemClass.Length is 0)
            {
                itemClass = inheritTwo is "AbstractSceptre" ? "Sceptres"
                        : inheritThree is "Claws" ? "Claws"
                        : inheritThree is "Daggers" ? "Daggers"
                        : inheritThree is "Wands" ? inheritFour is "WandAtlas1" ? "Convoking_Wand" : "Wands"
                        : inheritThree is "OneHandSwords" ? "One_Hand_Swords"
                        : inheritThree is "OneHandAxes" ? "One_Hand_Axes"
                        : inheritThree is "OneHandMaces" ? "One_Hand_Maces"
                        : inheritThree is "OneHandSpears" ? "Spears"
                        : inheritThree is "OneHandFlails" ? "Flails" // to test
                        : inheritThree is "Bows" ? "Bows"
                        : inheritThree is "TwoHandSwords" ? "Two_Hand_Swords"
                        : inheritThree is "TwoHandAxes" ? "Two_Hand_Axes"
                        : inheritThree is "TwoHandMaces" ? "Two_Hand_Maces"
                        : inheritThree is "FishingRods" ? "Fishing_Rods"
                        : inheritThree is "Crossbows" ? "Crossbows"
                        : inheritThree is "Spears" ? "Spears"
                        : inheritThree is "Flails" ? "Flails"
                        : inheritThree is "Foci" ? "Foci"
                        : inheritThree is "Charms" ? "Charms"
                        : inheritFour is "AbstractOneHandSwordThrusting" ? "Thrusting_One_Hand_Swords"
                        : inheritFour is "AbstractSceptre" ? "Sceptres"
                        : inheritFour is "AbstractRuneDagger" ? "Rune_Daggers"
                        : inheritFour is "AbstractStaff" ? "Staves"
                        : inheritFour is "AbstractWarstaff" ? "Warstaves"
                        : inheritFour.StartWith("FourQuarterstaff") ? "Quarterstaves"
                        : inheritFour.StartWith("FourCrossbow") ? "Crossbows"
                        : itemClass;
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
