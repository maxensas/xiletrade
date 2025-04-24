using System;
using System.Text;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal sealed class PoeDb
{
    internal string Link { get; private set; }
    
    internal PoeDb(ItemBase item)
    {
        StringBuilder url = new(Strings.UrlPoedbHost);
        var culture = Strings.Culture[DataManager.Config.Options.Language];
        var isPoe2 = DataManager.Config.Options.GameVersion is 1;
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

        string itemClass = string.Empty;
        var currenItem = item;
        string Inherit = currenItem.Inherits.Length > 0 ? currenItem.Inherits[0] : string.Empty;
        string Inherit2 = currenItem.Inherits.Length > 1 ? currenItem.Inherits[1] : string.Empty;
        string Inherit3 = currenItem.Inherits.Length > 2 ? currenItem.Inherits[2] : string.Empty;
        string Inherit4 = currenItem.Inherits.Length > 3 ? currenItem.Inherits[3] : string.Empty;

        if (Inherit is "Amulets" or "Rings" or "Belts" or "Quivers" or "Trinkets")
        {
            itemClass = Inherit;
        }

        if (Inherit is "Waystones")
        {
            var match = RegexUtil.DecimalNoPlusPattern().Matches(currenItem.TypeEn);
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
            // use dictionnay ?  Strings.lPoeDbInherit.TryGetValue(Inherit2, out string itemClass)
            itemClass = Inherit2 is "BodyArmours" ? "Body_Armours"
                        : Inherit2 is "Helmets" ? "Helmets"
                        : Inherit2 is "Boots" ? "Boots"
                        : Inherit2 is "Gloves" ? "Gloves"
                        : Inherit2 is "Shields" ? "Shields"
                        : Inherit2 is "Focii" ? "Foci"
                        : Inherit2 is "HeistBlueprint" ? "Blueprints"
                        : Inherit2 is "HeistContract" ? "Contracts"
                        : Inherit2 is "AbstractLifeFlask" ? "Life_Flasks"
                        : Inherit2 is "AbstractManaFlask" ? "Mana_Flasks"
                        : Inherit2 is "AbstractHybridFlask" ? "Hybrid_Flasks"
                        : Inherit2 is "AbstractUtilityFlask" ? isPoe2 ? "Charms" : "Utility_Flasks"
                        : Inherit2 is "AbstractStaff" ? "Staves"
                        : Inherit2 is "AbstractWand" ? "Wands"
                        : Inherit2 is "JewelStr" ? isPoe2 ? "Ruby" : "Crimson_Jewel"
                        : Inherit2 is "JewelDex" ? isPoe2 ? "Emerald" : "Viridian_Jewel"
                        : Inherit2 is "JewelInt" ? isPoe2 ? "Sapphire" : "Cobalt_Jewel"
                        : Inherit2 is "JewelPrismatic" ? "Prismatic_Jewel"
                        : Inherit2 is "JewelAbyssMelee" ? "Murderous_Eye_Jewel"
                        : Inherit2 is "JewelAbyssRanged" ? "Searching_Eye_Jewel"
                        : Inherit2 is "JewelAbyssCaster" ? "Hypnotic_Eye_Jewel"
                        : Inherit2 is "JewelAbyssSummoner" ? "Ghastly_Eye_Jewel"
                        : Inherit2 is "JewelTimeless" ? "Timeless_Jewel"
                        : Inherit2 is "JewelPassiveTreeExpansionLarge" ? "Large_Cluster_Jewel"
                        : Inherit2 is "JewelPassiveTreeExpansionMedium" ? "Medium_Cluster_Jewel"
                        : Inherit2 is "JewelPassiveTreeExpansionSmall" ? "Small_Cluster_Jewel"
                        : itemClass;

            if (itemClass.Length > 0 && Inherit is "Armours")
            {
                if (Inherit3.Contain("StrDexInt")) itemClass += "_str_dex_int"; // cascade neeeded
                else if (Inherit3.Contain("StrDex")) itemClass += "_str_dex";
                else if (Inherit3.Contain("StrInt")) itemClass += "_str_int";
                else if (Inherit3.Contain("DexInt")) itemClass += "_dex_int";
                else if (Inherit3.Contain("Str")) itemClass += "_str";
                else if (Inherit3.Contain("Dex")) itemClass += "_dex";
                else if (Inherit3.Contain("Int")) itemClass += "_int";
                else if (Inherit3.Contain("HelmetExpedition")) itemClass = "Runic_Crown";
                else if (Inherit3.Contain("BootsExpedition")) itemClass = "Runic_Sabatons";
                else if (Inherit3.Contain("GlovesExpedition")) itemClass = "Runic_Gauntlets";
            }
            if (itemClass.Length is 0)
            {
                itemClass = Inherit2 is "AbstractSceptre" ? "Sceptres"
                        : Inherit3 is "Claws" ? "Claws"
                        : Inherit3 is "Daggers" ? "Daggers"
                        : Inherit3 is "Wands" ? Inherit4 is "WandAtlas1" ? "Convoking_Wand" : "Wands"
                        : Inherit3 is "OneHandSwords" ? "One_Hand_Swords"
                        : Inherit3 is "OneHandAxes" ? "One_Hand_Axes"
                        : Inherit3 is "OneHandMaces" ? "One_Hand_Maces"
                        : Inherit3 is "OneHandSpears" ? "Spears"
                        : Inherit3 is "OneHandFlails" ? "Flails" // to test
                        : Inherit3 is "Bows" ? "Bows"
                        : Inherit3 is "TwoHandSwords" ? "Two_Hand_Swords"
                        : Inherit3 is "TwoHandAxes" ? "Two_Hand_Axes"
                        : Inherit3 is "TwoHandMaces" ? "Two_Hand_Maces"
                        : Inherit3 is "FishingRods" ? "Fishing_Rods"
                        : Inherit3 is "Crossbows" ? "Crossbows"
                        : Inherit3 is "Spears" ? "Spears"
                        : Inherit3 is "Flails" ? "Flails"
                        : Inherit3 is "Foci" ? "Foci"
                        : Inherit3 is "Charms" ? "Charms"
                        : Inherit4 is "AbstractOneHandSwordThrusting" ? "Thrusting_One_Hand_Swords"
                        : Inherit4 is "AbstractSceptre" ? "Sceptres"
                        : Inherit4 is "AbstractRuneDagger" ? "Rune_Daggers"
                        : Inherit4 is "AbstractStaff" ? "Staves"
                        : Inherit4 is "AbstractWarstaff" ? "Warstaves"
                        : Inherit4.StartWith("FourQuarterstaff") ? "Quarterstaves"
                        : Inherit4.StartWith("FourCrossbow") ? "Crossbows"
                        : itemClass;
            }
        }
        if (itemClass.Length is 0)
        {
            itemClass = currenItem.TypeEn.Replace(" ", "_");
        }

        if (itemClass.Length is 0)
        {
            Link = url.Append("Modifiers").ToString();
        }
        Link = url.Append(itemClass).Append("#ModifiersCalc").ToString();
    }
}
