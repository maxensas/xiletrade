using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.ViewModels.Main;

namespace Xiletrade.Library.Shared;

/// <summary>Static helper class containing methods used by Xiletrade to interact with other third apps.</summary>
internal static class Addons
{
    private static IServiceProvider _serviceProvider;

    internal static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static ItemBaseName GetCurrentItem() 
    {
        return _serviceProvider.GetRequiredService<MainViewModel>().Logic.CurrentItem;
    }
    //poedb
    internal static string GetPoeDbLink()
    {
        StringBuilder url = new("https://poedb.tw/");
        var culture = Strings.Culture[DataManager.Config.Options.Language];
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
        var currenItem = GetCurrentItem();
        string Inherit = currenItem.Inherits.Length > 0 ? currenItem.Inherits[0] : string.Empty;
        string Inherit2 = currenItem.Inherits.Length > 1 ? currenItem.Inherits[1] : string.Empty;
        string Inherit3 = currenItem.Inherits.Length > 2 ? currenItem.Inherits[2] : string.Empty;
        string Inherit4 = currenItem.Inherits.Length > 3 ? currenItem.Inherits[3] : string.Empty;

        if (Inherit is "Amulets" or "Rings" or "Belts" or "Quivers" or "Trinkets")
        {
            itemClass = Inherit;
        }

        if (itemClass.Length == 0)
        {
            // use dictionnay ?  Strings.lPoeDbInherit.TryGetValue(Inherit2, out string itemClass)
            itemClass = Inherit2 is "BodyArmours" ? "Body_Armours"
                        : Inherit2 is "Helmets" ? "Helmets"
                        : Inherit2 is "Boots" ? "Boots"
                        : Inherit2 is "Gloves" ? "Gloves"
                        : Inherit2 is "Shields" ? "Shields"
                        : Inherit2 is "HeistBlueprint" ? "Blueprints"
                        : Inherit2 is "HeistContract" ? "Contracts"
                        : Inherit2 is "AbstractLifeFlask" ? "Life_Flasks"
                        : Inherit2 is "AbstractManaFlask" ? "Mana_Flasks"
                        : Inherit2 is "AbstractHybridFlask" ? "Hybrid_Flasks"
                        : Inherit2 is "AbstractUtilityFlask" ? "Utility_Flasks"
                        : Inherit2 is "JewelStr" ? "Crimson_Jewel"
                        : Inherit2 is "JewelDex" ? "Viridian_Jewel"
                        : Inherit2 is "JewelInt" ? "Cobalt_Jewel"
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
                if (Inherit3.Contains("StrDexInt", StringComparison.Ordinal)) itemClass += "_str_dex_int"; // cascade neeeded
                else if (Inherit3.Contains("StrDex", StringComparison.Ordinal)) itemClass += "_str_dex";
                else if (Inherit3.Contains("StrInt", StringComparison.Ordinal)) itemClass += "_str_int";
                else if (Inherit3.Contains("DexInt", StringComparison.Ordinal)) itemClass += "_dex_int";
                else if (Inherit3.Contains("Str", StringComparison.Ordinal)) itemClass += "_str";
                else if (Inherit3.Contains("Dex", StringComparison.Ordinal)) itemClass += "_dex";
                else if (Inherit3.Contains("Int", StringComparison.Ordinal)) itemClass += "_int";
                else if (Inherit3.Contains("HelmetExpedition", StringComparison.Ordinal)) itemClass = "Runic_Crown";
                else if (Inherit3.Contains("BootsExpedition", StringComparison.Ordinal)) itemClass = "Runic_Sabatons";
                else if (Inherit3.Contains("GlovesExpedition", StringComparison.Ordinal)) itemClass = "Runic_Gauntlets";
            }
            if (itemClass.Length == 0)
            {
                itemClass = Inherit3 is "Claws" ? "Claws"
                        : Inherit3 is "Daggers" ? "Daggers"
                        : Inherit3 is "Wands" ? Inherit4 is "WandAtlas1" ? "Convoking_Wand" : "Wands"
                        : Inherit3 is "OneHandSwords" ? "One_Hand_Swords"
                        : Inherit3 is "OneHandAxes" ? "One_Hand_Axes"
                        : Inherit3 is "OneHandMaces" ? "One_Hand_Maces"
                        : Inherit3 is "Bows" ? "Bows"
                        : Inherit3 is "TwoHandSwords" ? "Two_Hand_Swords"
                        : Inherit3 is "TwoHandAxes" ? "Two_Hand_Axes"
                        : Inherit3 is "TwoHandMaces" ? "Two_Hand_Maces"
                        : Inherit3 is "FishingRods" ? "Fishing_Rods"
                        : Inherit4 is "AbstractOneHandSwordThrusting" ? "Thrusting_One_Hand_Swords"
                        : Inherit4 is "AbstractSceptre" ? "Sceptres"
                        : Inherit4 is "AbstractRuneDagger" ? "Rune_Daggers"
                        : Inherit4 is "AbstractStaff" ? "Staves"
                        : Inherit4 is "AbstractWarstaff" ? "Warstaves"
                        : itemClass;
            }
        }
        if (itemClass.Length == 0)
        {
            itemClass = currenItem.TypeEn.Replace(" ", "_");
        }

        if (itemClass.Length == 0)
        {
            return url.Append("Modifiers").ToString();
        }
        return url.Append(itemClass).Append("#ModifiersCalc").ToString();
    }

    //poewiki
    internal static string GetPoeWikiLink(string rarity) // Poe Wiki only well done in english and russian.
    {
        var currentItem = GetCurrentItem();
        string name = DataManager.Config.Options.Language is 0 or 6 ? currentItem.Name : currentItem.NameEn;
        string type = DataManager.Config.Options.Language is 0 or 6 ? currentItem.Type : currentItem.TypeEn;
        string url = DataManager.Config.Options.Language is 6 ? Strings.UrlPoeWikiRu : Strings.UrlPoeWiki;
        url += (rarity == Resources.Resources.General006_Unique && name.Length > 0 ? name : type).Replace(' ', '_');

        return url;
    }

    //ninja
    internal static void CheckNinja(MainViewModel vm, string league, string itemRarity, string influence, string lvlMin, string qualMin, int alternIdx, bool synthBlight, bool blightRavaged, bool scourgedMap, XiletradeItem itemOptions, CancellationToken token)
    {
        string[] item = GetNinjaLink(league, itemRarity, influence, lvlMin, qualMin, alternIdx, synthBlight, blightRavaged, scourgedMap, itemOptions).Split('/');
        if (item.Length == 3)
        {
            try
            {
                bool apiKind = !(item[1] is "currency" or "fragments");
                string ninjaApi = apiKind ? Strings.ApiNinjaItem : Strings.ApiNinjaCur;
                string type = item[1] switch
                {
                    "currency" => "Currency",
                    "fragments" => "Fragment",
                    "oils" => "Oil",
                    "incubators" => "Incubator",
                    "invitations" => "Invitation",
                    "scarabs" => "Scarab",
                    "fossils" => "Fossil",
                    "resonators" => "Resonator",
                    "essences" => "Essence",
                    "divination-cards" => "DivinationCard",
                    "prophecies" => "Prophecy",
                    "skill-gems" => "SkillGem",
                    "base-types" => "BaseType",
                    "unique-maps" => "UniqueMap",
                    "maps" => "Map",
                    "blighted-maps" => "Map",
                    "blight-ravaged-maps" => "Map",
                    "scourged-maps" => "Map",
                    "unique-jewels" => "UniqueJewel",
                    "unique-flasks" => "UniqueFlask",
                    "unique-weapons" => "UniqueWeapon",
                    "unique-armours" => "UniqueArmour",
                    "unique-accessories" => "UniqueAccessory",
                    "beasts" => "Beast",
                    "delirium-orbs" => "DeliriumOrb",
                    "vials" => "Vial",
                    "watchstones" => "Watchstone",
                    "cluster-jewels" => "ClusterJewel",
                    "omens" => "Omen",
                    "tattoos" => "Tattoo",
                    "unique-relics" => "UniqueRelic",
                    "coffins" => "Coffin",
                    "allflame-embers" => "AllflameEmber",
                    "kalguuran-runes" => "KalguuranRune",
                    "memorylines" => "Memory",
                    "artifact" => "Artifact",
                    _ => "Currency",
                };

                if (type is "Map" && item.Length >= 2)
                {
                    type = item[2].StartsWith("blighted", StringComparison.Ordinal) ? "BlightedMap"
                        : item[2].StartsWith("blight-ravaged", StringComparison.Ordinal) ? "BlightRavagedMap"
                        : scourgedMap ? "ScourgedMap" : type;
                }

                string url = ninjaApi + league + "&type=" + type;

                NinjaValue ninja = new();
                if (apiKind)
                {
                    NinjaItemContract jsonItem = (NinjaItemContract)GetNinjaItem(league, type, url);
                    if (jsonItem is null)
                    {
                        return;
                    }
                    NinjaItemLines line = jsonItem.Lines.FirstOrDefault(x => x.Id == item[2]);
                    if (line is not null)
                    {
                        ninja.Id = line.Id;
                        ninja.Name = line.Name;
                        ninja.ChaosPrice = line.ChaosPrice;
                        ninja.ExaltPrice = line.ExaltPrice;
                        ninja.DivinePrice = line.DivinePrice;
                    }
                }
                else
                {
                    NinjaCurrencyContract jsonItem = (NinjaCurrencyContract)GetNinjaItem(league, type, url);
                    if (jsonItem is null)
                    {
                        return;
                    }
                    NinjaCurLines line = jsonItem.Lines.FirstOrDefault(x => x.Id == item[2]);
                    if (line is not null)
                    {
                        ninja.Id = line.Id;
                        ninja.Name = line.Name;
                        ninja.ChaosPrice = line.ChaosPrice;
                    }
                }

                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (ninja.ChaosPrice > 0)
                {
                    double value = 0;

                    value = ninja.DivinePrice > 1 ? Math.Round(ninja.DivinePrice, 1) : Math.Round(ninja.ChaosPrice, 1);
                    vm.NinjaButton.ImageName = ninja.DivinePrice > 1 ? "divine" : "chaos";

                    string valueString = value.ToString();
                    double nbDigit = valueString.Length - 1;
                    double charLength = 6;
                    double leftPad = 63 + nbDigit * charLength;
                    double rightPad = 38 - nbDigit * charLength;
                    vm.NinjaButton.ImgLeftRightMargin = leftPad + "." + rightPad;
                    /*
                   double leftPad = imgNinja.Margin.Left; // 97
                   double rightPad = imgNinja.Margin.Right; // 1
                   */
                    vm.NinjaButton.Price = valueString;
                    vm.NinjaButton.ValWidth = 76 + nbDigit * charLength;
                    vm.NinjaButton.BtnWidth = 90 + nbDigit * charLength;
                    //vm.NinjaButton.ImgMargin = new Thickness(leftPad, 0, rightPad, -1); //"97,2,1,2";
                    vm.NinjaButton.Visible = true;
                }
                else
                {
                    vm.NinjaButton.Visible = false;
                }
            }
            catch//(WebException ex)
            {

            }
        }
    }

    internal static string GetNinjaLink(string league, string itemRarity, string influence, string lvlMin, string qualMin, int alternIdx, bool synthBlight, bool blightRavaged, bool scourgedMap, XiletradeItem itemOptions) // Poe NINJA
    {
        string tab = string.Empty;
        bool useBase = false, useName = false, useLvl = false, useInfluence = false, is_unique = false;
        var currentItem = GetCurrentItem();
        StringBuilder sbName = new(currentItem.NameEn.Length > 0 ? currentItem.NameEn : currentItem.TypeEn);
        sbName.Replace(" ", "-").Replace("'", string.Empty).Replace(",", string.Empty).Replace("\"", string.Empty).Replace("ö", "o"); // maybe use sb in whole method
        string itemName = sbName.ToString().ToLowerInvariant();
        string itemBaseType = currentItem.TypeEn.Replace(" ", "-").Replace("'", string.Empty).ToLowerInvariant();
        string itemInherit = currentItem.Inherits[0].ToLowerInvariant();
        if (itemInherit.Length is 0)
        {
            itemInherit = itemBaseType;
        }
        // do stringbuilder for itemName and other strings
        if (itemName is "voices" && itemOptions.ItemFilters.Count == 2)
        {
            ItemFilter seekFilter = itemOptions.ItemFilters.FirstOrDefault(x => x.Id is "explicit.stat_1085446536");
            if (seekFilter is not null)
            {
                if (seekFilter.Min > 1)
                {
                    itemName += "-" + seekFilter.Min + "-passives";
                }
            }
        }
        else if (itemName is "vessel-of-vinktar" && itemOptions.ItemFilters.Count == 5)
        {
            string stat_attack = "explicit.stat_4292531291";
            string stat_spells = "explicit.stat_4108305628";
            string stat_conv = "explicit.stat_660386148";
            //string stat_pen = "explicit.stat_4164990693";
            List<string> stats = new() { stat_attack, stat_spells, stat_conv, /*stat_pen*/ };

            ItemFilter seekFilter = itemOptions.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
            if (seekFilter != null)
            {
                itemName += seekFilter.Id == stat_attack ? "-added-attacks"
                    : seekFilter.Id == stat_spells ? "-added-spells"
                    : seekFilter.Id == stat_conv ? "-conversion" : string.Empty;
            }
        }
        else if (itemName is "impresence" && itemOptions.ItemFilters.Count == 7)
        {
            string stat_chaos = "explicit.stat_3531280422";
            string stat_physical = "explicit.stat_960081730";
            string stat_fire = "explicit.stat_321077055";
            string stat_lightning = "explicit.stat_1334060246";
            string stat_cold = "explicit.stat_2387423236";
            List<string> stats = new() { stat_chaos, stat_physical, stat_fire, stat_lightning, stat_cold };

            ItemFilter seekFilter = itemOptions.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
            if (seekFilter != null)
            {
                itemName += seekFilter.Id == stat_chaos ? "-chaos"
                    : seekFilter.Id == stat_physical ? "-physical"
                    : seekFilter.Id == stat_fire ? "-fire"
                    : seekFilter.Id == stat_lightning ? "-lightning"
                    : seekFilter.Id == stat_cold ? "-cold"
                    : string.Empty;
            }
        }
        else if (itemBaseType.Contains("cluster", StringComparison.Ordinal))
        {
            // fucking ugly, to redo with linq request
            bool doIt = false;
            int option = -1;
            double passives = 0;
            foreach (ItemFilter filter in itemOptions.ItemFilters)
            {
                if (filter.Id.Contains(Strings.Stat.PassiveSkill, StringComparison.Ordinal))
                {
                    passives = filter.Max;
                }
                else if (filter.Id is Strings.Stat.SmallPassive)
                {
                    doIt = true;
                    option = filter.Option;
                }
            }

            if (doIt)
            {
                var result =
                    from resultEnglish in DataManager.FilterEn.Result
                    from filterEnglish in resultEnglish.Entries
                    where filterEnglish.ID == Strings.Stat.SmallPassive
                    select filterEnglish.Option.Options;
                if (result.Any())
                {
                    IEnumerable<FilterResultOptions> options = result.First();
                    var result2 =
                    from filterOption in options
                    where Convert.ToInt32(filterOption.ID) == option
                    select filterOption.Text;
                    if (result2.Any())
                    {
                        StringBuilder sbItem = new(result2.First());
                        sbItem.Replace("%", string.Empty).Replace(" ", "-").Replace('\n','-');
                        sbItem.Append('-').Append(passives).Append("-passives");

                        itemName = sbItem.ToString().ToLowerInvariant();
                    }
                }
            }
        }

        int idLang = DataManager.Config.Options.Language;
        if (itemRarity == Resources.Resources.General006_Unique) is_unique = true;

        string ninjaLeague = "standard/";
        string leagueKind = DataManager.League.Result[0].Id.ToLowerInvariant();
        /*
        if (leagueKind is "expedition")
        {
            leagueKind = "gen-11";
        }
        else if (leagueKind is "scourge")
        {
            leagueKind = "gen-12";
        }*/

        LeagueResult leagueSelect = DataManager.League.Result.FirstOrDefault(x => x.Text == league);
        if (leagueSelect != null)
        {
            ninjaLeague = leagueSelect.Id is "Standard" ? "standard/"
                : leagueSelect.Id is "Hardcore" ? "hardcore/"
                : leagueSelect.Id.Contains('(', StringComparison.Ordinal)
                && leagueSelect.Id.Contains(')', StringComparison.Ordinal)
                && leagueSelect.Id.Contains("00", StringComparison.Ordinal)
                && leagueSelect.Id.Contains("HC", StringComparison.Ordinal) ? "eventhc/"
                : leagueSelect.Id.Contains('(', StringComparison.Ordinal)
                && leagueSelect.Id.Contains(')', StringComparison.Ordinal)
                && leagueSelect.Id.Contains("00", StringComparison.Ordinal) ? "event/"
                : leagueSelect.Id.Contains("Hardcore", StringComparison.Ordinal) ? "challengehc/" : "challenge/";
        }

        if (itemInherit is "quivers") itemInherit = "armours";

        switch (itemInherit)
        {
            case "currency":
                {
                    tab = itemName.Contains("oil", StringComparison.Ordinal) ? "oils"
                        : itemName.Contains("fossil", StringComparison.Ordinal) ? "fossils"
                        : itemName.Contains("essence", StringComparison.Ordinal) || itemName.Contains("remnant-of-corruption", StringComparison.Ordinal) ? "essences"
                        : itemName.Contains("simulacrum", StringComparison.Ordinal)
                        || itemName.Contains("splinter", StringComparison.Ordinal) && itemName.Contains("timeless", StringComparison.Ordinal) ? "fragments"
                        : itemName.Contains("delirium-orb", StringComparison.Ordinal) ? "delirium-orbs"
                        : itemName.Contains("vial-of", StringComparison.Ordinal) ? "vials"
                        : itemName.Contains("omen", StringComparison.Ordinal) ? "omens"
                        : itemName.Contains("tattoo", StringComparison.Ordinal) ? "tattoos"
                        : itemName.Contains("rune", StringComparison.Ordinal) ? "kalguuran-runes"
                        : itemName.Contains("resonator", StringComparison.Ordinal) ? "resonators"
                        : itemName.Contains("astragali", StringComparison.Ordinal) || itemName.Contains("burial-medallion", StringComparison.Ordinal)
                        || itemName.Contains("scrap-metal", StringComparison.Ordinal) || itemName.Contains("exotic-coinage", StringComparison.Ordinal) ? "artifact"
                        : itemName.Equals("chaos-orb", StringComparison.Ordinal) ? "currency"
                        : "currency";
                    useBase = !itemName.Equals("chaos-orb", StringComparison.Ordinal);
                    break;
                }
            case "memorylines":
                {
                    tab = "memorylines";
                    useName = true;
                    break;
                }
            case "leaguebestiary":
                {
                    tab = "beasts";
                    useName = true;
                    break;
                }
            case "mapfragments":
                {
                    tab = itemBaseType.Contains("invitation", StringComparison.Ordinal) ? "invitations" 
                        : itemBaseType.Contains("scarab", StringComparison.Ordinal) ? "scarabs"
                        : "fragments";
                    useBase = true;
                    break;
                }
            case "atlasupgrades":
                {
                    tab = "watchstones";
                    string uses = "12";
                    uses = itemName switch
                    {
                        "irresistable-temptation" => "18",
                        "booming-populace" => "15",
                        "territories-unknown" => "18",
                        _ => "12",
                    };
                    tab += "/" + itemName + "-" + itemBaseType + "-" + uses;
                    break;
                }
            case "legion":
                {
                    tab = "incubators";
                    useBase = true;
                    break;
                }
            case "divinationcards":
                {
                    tab = "divination-cards";
                    useBase = true;
                    break;
                }
            case "prophecies":
                {
                    tab = "prophecies";
                    useBase = true;
                    break;
                }
            case "gems":
                {
                    tab = "skill-gems";
                    useBase = true;
                    break;
                }
            case "maps":
                {
                    if (is_unique)
                    {
                        tab = "unique-maps/" + itemName + "-t" + lvlMin;
                    }
                    else
                    {
                        string mapKind = synthBlight && !itemBaseType.Contains("blighted", StringComparison.Ordinal) ? "blighted-"
                            : blightRavaged && !itemBaseType.Contains("blight", StringComparison.Ordinal) ? "blight-ravaged-"
                            : scourgedMap ? "scourged-"
                            : string.Empty;
                        //scourged
                        var mapGen = DataManager.Config.Options.NinjaMapGeneration;
                        tab = mapKind + "maps/" + mapKind + itemBaseType + "-t" + lvlMin + "-" + (mapGen is not null && mapGen.Length > 0 ? mapGen : leagueKind);
                    }
                    break;
                }
            case "flasks":
                {
                    if (is_unique)
                    {
                        tab = "unique-flasks";
                        useName = true;
                    }
                    else
                    {
                        tab = "base-types";
                    }
                    break;
                }
            case "jewels":
                {
                    if (is_unique)
                    {
                        tab = "unique-jewels";
                        useBase = true;
                        useName = true;
                    }
                    else if (itemBaseType.Contains("cluster", StringComparison.Ordinal))
                    {
                        tab = "cluster-jewels";
                        //itemBaseType
                        useBase = false;
                        useName = true;
                        useLvl = true;
                    }
                    else
                    {
                        tab = "base-types";
                        useBase = true;
                        useLvl = true;
                    }
                    break;
                }
            case "weapons":
                {
                    useBase = true;
                    if (is_unique)
                    {
                        tab = "unique-weapons";
                        useName = true;
                    }
                    else
                    {
                        tab = "base-types";
                        useLvl = true;
                        useInfluence = true;
                    }
                    break;
                }
            case "armours":
                {
                    useBase = true;
                    if (is_unique)
                    {
                        tab = "unique-armours";
                        useName = true;
                    }
                    else
                    {
                        tab = "base-types";
                        useLvl = true;
                        useInfluence = true;
                    }
                    break;
                }
            case "amulets":
            case "rings":
            case "belts":
                useBase = true;
                if (is_unique)
                {
                    tab = "unique-accessories";
                    useName = true;
                }
                else
                {
                    tab = "base-types";
                    useLvl = true;
                    useInfluence = true;
                }
                break;
            case "relics":
                tab = "unique-relics";
                itemName += "-relic";
                useName = true;
                break;
            case "filled-coffin":
                tab = "coffins";
                useLvl = true;
                useName = true;
                break;
            case "necropolispack":
                tab = "allflame-embers";
                useLvl = true;
                useName = true;
                break;
        }

        if (itemInherit is not "maps" and not "atlasupgrades")
        {
            if (!(!useName && !useBase))
            {
                tab += "/";
            }

            if (itemInherit is "gems" && alternIdx > 0)
            {
                tab += alternIdx is 1 ? "anomalous-"
                    : alternIdx is 2 ? "divergent-"
                    : alternIdx is 3 ? "phantasmal-"
                    : string.Empty;
            }

            if (useName && useBase)
                tab += itemName + "-" + itemBaseType;
            else if (useName && !useBase)
                tab += itemName;
            else if (!useName && useBase)
                tab += itemBaseType;

            if (useLvl)
            {
                int lvlTemp = int.Parse(lvlMin, CultureInfo.InvariantCulture);
                bool cluster = itemBaseType.Contains("cluster", StringComparison.Ordinal);
                bool allflame = itemInherit is "necropolispack";
                bool coffin = itemInherit is "filled-coffin";

                if (!cluster && !allflame && !coffin)
                {
                    tab += lvlTemp <= 82 ? "-82"
                        : lvlTemp == 83 ? "-83"
                        : lvlTemp == 84 ? "-84"
                        : lvlTemp == 85 ? "-85"
                        : lvlTemp >= 86 ? "-86"
                        : string.Empty;
                }

                if (cluster)
                {
                    tab += lvlTemp >= 84 ? "-84"
                        : lvlTemp >= 75 ? "-75"
                        : lvlTemp >= 68 ? "-68"
                        : lvlTemp >= 50 ? "-50"
                        : "-1";
                }
                if (allflame)
                {
                    tab += lvlTemp >= 84 ? "-(84-100)"
                        : lvlTemp >= 82 ? "-(82-83)"
                        : lvlTemp >= 76 ? "-(76-81)"
                        : lvlTemp >= 60 ? "-(60-75)"
                        : string.Empty;
                }
                if (coffin)
                {
                    //bool create = itemName.StartsWith("creates-", StringComparison.Ordinal);
                    tab += lvlTemp >= 84 ? "-(84-100)"
                        : lvlTemp >= 80 ? "-(80-83)"
                        : lvlTemp >= 70 ? "-(70-79)"
                        : string.Empty;
                }
            }

            CultureInfo cultureEn = new(Strings.Culture[0]);
            System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

            if (useInfluence && influence != Resources.Resources.Main036_None)
            {
                string[] influ = influence.Split('/');
                for (int i = 0; i < influ.Length; i++)
                {
                    tab += influ[i] == Resources.Resources.Main037_Shaper ? "-" + rm.GetString("Main037_Shaper", cultureEn).ToLowerInvariant()
                        : influ[i] == Resources.Resources.Main038_Elder ? "-" + rm.GetString("Main038_Elder", cultureEn).ToLowerInvariant()
                        : influ[i] == Resources.Resources.Main039_Crusader ? "-" + rm.GetString("Main039_Crusader", cultureEn).ToLowerInvariant()
                        : influ[i] == Resources.Resources.Main040_Redeemer ? "-" + rm.GetString("Main040_Redeemer", cultureEn).ToLowerInvariant()
                        : influ[i] == Resources.Resources.Main041_Hunter ? "-" + rm.GetString("Main041_Hunter", cultureEn).ToLowerInvariant()
                        : influ[i] == Resources.Resources.Main042_Warlord ? "-" + rm.GetString("Main042_Warlord", cultureEn).ToLowerInvariant()
                        : string.Empty;
                }
            }

            if (itemInherit is "gems")
            {
                string addC = string.Empty;
                int lvlTemp = int.Parse(lvlMin, CultureInfo.InvariantCulture);
                bool awakened = itemName.Contains("awakened", StringComparison.Ordinal);
                bool bigSup = itemName.Contains("empower-support", StringComparison.Ordinal) || itemName.Contains("enlighten-support", StringComparison.Ordinal) || itemName.Contains("enhance-support", StringComparison.Ordinal);
                if (lvlTemp == 6 && awakened)
                {
                    tab += "-6";
                    addC = "c";
                }
                else if (lvlTemp == 4 && bigSup)
                {
                    tab += "-4";
                    addC = "c";
                }
                else if (lvlTemp < 20)
                {
                    tab += "-1";
                }
                else if (lvlTemp == 20)
                    tab += "-20";
                else if (lvlTemp == 21)
                {
                    tab += "-21";
                    addC = "c";
                }

                if (!bigSup)
                {
                    if (int.TryParse(qualMin, NumberStyles.Any, CultureInfo.InvariantCulture, out int qualTemp))
                    {
                        if (qualTemp >= 20 && qualTemp < 23)
                            tab += "-20";
                        else if (qualTemp == 23)
                        {
                            tab += "-23";
                            addC = "c";
                        }
                    }
                }

                if (addC.Length == 0 && itemName.Contains("vaal", StringComparison.Ordinal))
                {
                    addC = "c";
                }
                tab += addC;

            }

            if (itemName.Contains("a-master-seeks-help", StringComparison.Ordinal))
            {
                tab = "prophecies?name=a master seeks help";
            }
            /*
            if (tab.Contains("voices-large-cluster-jewel", StringComparison.Ordinal))
            {

            }
            */
        }

        return ninjaLeague + tab;
    }

    internal static double GetNinjaChaosEq(string league, string NameCur, string tier)
    {
        double error = -1;
        string type = GetNinjaType(NameCur);
        if (type is not null)
        {
            string api = type is "Currency" or "Fragment" ? Strings.ApiNinjaCur : Strings.ApiNinjaItem;
            string urlNinja = api + league + "&type=" + type;

            object data = GetNinjaItem(league, type, urlNinja);
            if (data is not null)
            {
                if (data is NinjaCurrencyContract currency)
                {
                    NinjaCurLines line = currency.Lines.FirstOrDefault(x => x.Name == NameCur);
                    return line is not null ? line.ChaosPrice : error;
                }
                if (data is NinjaItemContract item)
                {
                    if (type is "Map" && tier is not null)
                    {
                        NinjaItemLines line = item.Lines.FirstOrDefault(x => x.Name == NameCur && x.Id.Contains("-" + tier + "-", StringComparison.Ordinal));
                        return line is not null ? line.ChaosPrice : error;
                    }
                    if (type is "UniqueMap")
                    {
                        string[] split = NameCur.Split('(');
                        if (split.Length == 2)
                        {
                            string mapName = split[0].Trim();
                            string tierUnique = "-t" + split[1].Replace("Tier ", string.Empty).Replace(")", string.Empty).Trim();
                            NinjaItemLines line = item.Lines.FirstOrDefault(x => x.Name == mapName && x.Id.EndsWith(tierUnique, StringComparison.Ordinal));
                            return line is not null ? line.ChaosPrice : error;
                        }
                    }
                    NinjaItemLines lineDef = item.Lines.FirstOrDefault(x => x.Name == NameCur);
                    return lineDef is not null ? lineDef.ChaosPrice : error;
                }
            }
        }
        return error;
    }

    internal static object GetNinjaItem(string league, string type, string url)
    {
        CheckNinjaLeague(league);
        int cacheTime = 30; // in minutes
        DateTime now = DateTime.UtcNow;
        try
        {
            NinjaCurrency nCurrency = type is "Currency" ? NinjaData.Currency
                : type is "Fragment" ? NinjaData.Fragment
                : null;
            NinjaItem nItem = type is "Oil" ? NinjaData.Oil
                : type is "Incubator" ? NinjaData.Incubator
                : type is "Invitation" ? NinjaData.Invitation
                : type is "Scarab" ? NinjaData.Scarab
                : type is "Fossil" ? NinjaData.Fossil
                : type is "Resonator" ? NinjaData.Resonator
                : type is "Essence" ? NinjaData.Essence
                : type is "DivinationCard" ? NinjaData.DivinationCard
                : type is "Prophecy" ? NinjaData.Prophecy
                : type is "SkillGem" ? NinjaData.SkillGem
                : type is "BaseType" ? NinjaData.BaseType
                : type is "UniqueMap" ? NinjaData.UniqueMap
                : type is "Map" ? NinjaData.Map
                : type is "BlightedMap" ? NinjaData.BlightedMap
                : type is "BlightRavagedMap" ? NinjaData.BlightRavagedMap
                : type is "ScourgedMap" ? NinjaData.ScourgedMap
                : type is "UniqueJewel" ? NinjaData.UniqueJewel
                : type is "UniqueFlask" ? NinjaData.UniqueFlask
                : type is "UniqueWeapon" ? NinjaData.UniqueWeapon
                : type is "UniqueArmour" ? NinjaData.UniqueArmour
                : type is "UniqueAccessory" ? NinjaData.UniqueAccessory
                : type is "Beast" ? NinjaData.Beast
                : type is "DeliriumOrb" ? NinjaData.DeliriumOrb
                : type is "Vial" ? NinjaData.Vial
                : type is "Watchstone" ? NinjaData.Watchstone
                : type is "ClusterJewel" ? NinjaData.ClusterJewel
                : type is "Omen" ? NinjaData.Omen
                : type is "Tattoo" ? NinjaData.Tattoo
                : type is "UniqueRelic" ? NinjaData.UniqueRelic
                : type is "Coffin" ? NinjaData.Coffin
                : type is "AllflameEmber" ? NinjaData.AllflameEmber
                : type is "KalguuranRune" ? NinjaData.KalguuranRune
                : type is "Memory" ? NinjaData.Memory
                : type is "Artifact" ? NinjaData.Artifact
                : null;

            // to refactor with nItem with a new type
            if (nCurrency is not null)
            {
                if (nCurrency.Creation == DateTime.MinValue || nCurrency.Creation.AddMinutes(cacheTime) < now)
                {
                    var service = _serviceProvider.GetRequiredService<NetService>();
                    string sResult = service.SendHTTP(null, url, Client.Ninja).Result;

                    if (sResult.Length == 0)
                    {
                        return null;
                    }
                    nCurrency.Json = Json.Deserialize<NinjaCurrencyContract>(sResult);
                    nCurrency.Creation = DateTime.UtcNow;
                }
                return nCurrency.Json;
            }
            if (nItem is not null)
            {
                if (nItem.Creation == DateTime.MinValue || nItem.Creation.AddMinutes(cacheTime) < now)
                {
                    var service = _serviceProvider.GetRequiredService<NetService>();
                    string sResult = service.SendHTTP(null, url, Client.Ninja).Result;
                    if (sResult.Length == 0)
                    {
                        return null;
                    }
                    nItem.Json = Json.Deserialize<NinjaItemContract>(sResult);
                    nItem.Creation = DateTime.UtcNow;
                }
                return nItem.Json;
            }
        }
        catch (Exception)
        {
            // unmanaged exception : Xiletrade must remain independent of poe.ninja 

            // TODO : Add info label on the UI to see something is going wrong with ninja
        }
        return null;
    }
    /*
    public static double GetChaosEquivalent(string league, string tradeNameCur)
    {
        string urlNinja = StringsTable.NinjaCurApi + league + "&type=Currency";
        NinjaCurrency jsonItem = (NinjaCurrency)Ninja.GetItem(league, "Currency", urlNinja);
        if (jsonItem != null)
        {
            var curName =
                from detail in jsonItem.Details
                where detail.TradeId == tradeNameCur
                select detail.Name;
            if (curName.Any())
            {
                NinjaCurLines line = jsonItem.Lines.FirstOrDefault(x => x.Name == curName.First());
                if (line != null)
                {
                    return line.ChaosPrice;
                }
            }
        }
        return -1;
    }
    */

    private static string GetNinjaType(string NameCur)
    {
        var curId =
            from currency in DataManager.CurrenciesEn
            from entry in currency.Entries
            where entry.Text == NameCur
            select currency.Id;
        if (curId.Any())
        {
            if (curId.First().Contains(Strings.CurrencyType.Maps, StringComparison.Ordinal))
            {
                return "Map";
            }
            var cur = curId.First();
            return cur is "Currency" or "Catalysts" or "ExoticCurrency" ? "Currency"
                : cur is "Splinters" or "Fragments" ? "Fragment"
                : cur is "DeliriumOrbs" ? "DeliriumOrbs"
                : cur is "Oils" ? "Oil"
                : cur is "Incubators" ? "Incubator"
                : cur is "Scarabs" ? "Scarab"
                : cur is "DelveResonators" ? "Resonator"
                : cur is "DelveFossils" ? "Fossil"
                : cur is "Essences" ? "Essence"
                : cur is "Cards" ? "DivinationCard"
                : cur is "Prophecies" ? "Prophecy"
                : cur is "MapsUnique" ? "UniqueMap"
                : cur is "MapsBlighted" ? "BlightedMap"
                : cur is "Runes" ? "KalguuranRune"
                : cur is "MemoryLine" ? "Memory"
                : cur is "Artifact" ? "Artifact"
                : cur is "Ancestor" ? NameCur.StartsWith("Omen", StringComparison.Ordinal) ? "Omen" : "Tattoo"
                //: cur is "Expedition" or "Misc" ? null
                : null;
        }
        return null;
    }

    private static void CheckNinjaLeague(string league) // TODO move to model
    {
        if (NinjaData.League is null)
        {
            NinjaData.League = league;
            return;
        }
        if (NinjaData.League.Length > 0 && NinjaData.League != league)
        {
            NinjaData.League = league;

            // reset
            NinjaData.Currency.Creation = NinjaData.Fragment.Creation = NinjaData.Oil.Creation = NinjaData.Incubator.Creation =
                NinjaData.Invitation.Creation = NinjaData.Scarab.Creation = NinjaData.Fossil.Creation = NinjaData.Resonator.Creation =
                NinjaData.Essence.Creation = NinjaData.DivinationCard.Creation = NinjaData.Prophecy.Creation = NinjaData.SkillGem.Creation =
                NinjaData.BaseType.Creation = NinjaData.UniqueMap.Creation = NinjaData.Map.Creation = NinjaData.UniqueJewel.Creation =
                NinjaData.UniqueFlask.Creation = NinjaData.UniqueWeapon.Creation = NinjaData.UniqueArmour.Creation =
                NinjaData.UniqueAccessory.Creation = NinjaData.Beast.Creation = NinjaData.DeliriumOrb.Creation = NinjaData.Vial.Creation =
                NinjaData.Watchstone.Creation = NinjaData.ClusterJewel.Creation = NinjaData.Omen.Creation = NinjaData.Tattoo.Creation =
                NinjaData.UniqueRelic.Creation = NinjaData.Coffin.Creation = NinjaData.AllflameEmber.Creation = NinjaData.KalguuranRune.Creation =
                NinjaData.Memory.Creation = NinjaData.Artifact.Creation = DateTime.MinValue;
        }
    }
}
