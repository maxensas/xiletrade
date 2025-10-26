using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed record NinjaInfo : NinjaInfoBase
{
    internal string SubType { get; private set; }
    internal bool UseItemApi { get; private set; }
    internal bool Map { get; private set; }
    internal bool BlightMap { get; private set; }
    internal bool BlightRavagedMap { get; private set; }
    internal bool ScourgedMap { get; private set; }
    internal bool IsAllFlame { get; private set; }
    internal string Influences { get; private set; }
    internal string LvlMin { get; private set; }
    internal string QualMin { get; private set; }

    internal NinjaInfo(DataManagerService dm, PoeNinjaService ninja, XiletradeItem xiletradeItem, ItemData item
        , string league, string lvlMin, string qualMin, string influences) : base(dm, ninja)
    {
        League = league;
        LvlMin = lvlMin;
        QualMin = qualMin;
        Map = item.Flag.Map;
        BlightMap = item.IsBlightMap;
        BlightRavagedMap = item.IsBlightRavagedMap;
        ScourgedMap = false;
        Influences = influences;
        
        IsAllFlame = item.Flag.AllflameEmber;
        //temp
        var subLink = GetSubLink(xiletradeItem, item);
        var itemLink = subLink.Split('/');
        if (itemLink.Length is not 3)
        {
            return;
        }
        Link = Strings.UrlPoeNinja + subLink;
        VerifiedLink = true;
        Type = GetNinjaType(itemLink[1]);
        SubType = itemLink[2];
        UseItemApi = !(itemLink[1] is "currency" or "fragments" && !item.Flag.AllflameEmber);
        Url = (UseItemApi ? Strings.ApiNinjaItem : Strings.ApiNinjaCur) + League + "&type=" + Type;
    }

    //TOREDO using item flags, remove itemInherit
    private string GetSubLink(XiletradeItem xiletradeItem, ItemData item)
    {
        bool useBase = false, useName = false, useLvl = false, useInfluence = false;
        var tab = string.Empty;
        var type = string.Empty;
        var itemBaseType = item.TypeEn.Replace(" ", "-").Replace("'", string.Empty).ToLowerInvariant();
        var itemInherit = item.Inherits.Split('/')[0].ToLowerInvariant();
        if (itemInherit.Length is 0)
        {
            itemInherit = itemBaseType;
        }
        var itemName = GetItemName(item, itemBaseType, xiletradeItem);

        var leagueKind = _dm.League.Result[0].Id.ToLowerInvariant();
        var ninjaLeague = "standard/";

        var leagueSelect = _dm.League.Result.FirstOrDefault(x => x.Text == League);
        if (leagueSelect is not null)
        {
            var league = _ninja.NinjaState.Leagues.Where(x => x.Name == leagueSelect.Text).FirstOrDefault();
            if (league is not null)
            {
                leagueKind = league.Url;
                ninjaLeague = league.Url + "/";
            }
        }

        if (itemInherit is "quivers") itemInherit = "armours";

        switch (itemInherit)
        {
            case "currency":
                {
                    tab = itemName.Contain("oil") ? "oils"
                        : itemName.Contain("fossil") ? "fossils"
                        : itemName.Contain("essence") || itemName.Contain("remnant-of-corruption") ? "essences"
                        : itemName.Contain("simulacrum") || itemName.Contain("valdos-puzzle-box")
                        || itemName.Contain("splinter") && itemName.Contain("timeless") ? "fragments"
                        : itemName.Contain("delirium-orb") ? "delirium-orbs"
                        : itemName.Contain("vial-of") ? "vials"
                        : itemName.Contain("omen") ? "omens"
                        : itemName.Contain("tattoo") ? "tattoos"
                        : itemName.Contain("rune") ? "kalguuran-runes"
                        : itemName.Contain("resonator") ? "resonators"
                        : itemName.Contain("allflame-ember") ? "allflame-ember"
                        : itemName.Contain("astragali") || itemName.Contain("burial-medallion")
                        || itemName.Contain("scrap-metal") || itemName.Contain("exotic-coinage") ? "artifact" // artifacts
                        : itemName.Equal("chaos-orb") ? "currency"
                        : "currency";
                    useBase = !itemName.Equal("chaos-orb");
                    break;
                }
            case "memorylines":
                {
                    tab = "memorylines"; // "memories"
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
                    tab = itemBaseType.Contain("invitation") ? "invitations"
                        : itemBaseType.Contain("scarab") ? "scarabs"
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
                    if (item.Flag.Unique)
                    {
                        tab = "unique-maps/" + itemName + "-t" + LvlMin;
                    }
                    else
                    {
                        string mapKind = BlightMap && !itemBaseType.Contain("blighted") ? "blighted-"
                            : BlightRavagedMap && !itemBaseType.Contain("blight") ? "blight-ravaged-"
                            : ScourgedMap ? "scourged-"
                            : string.Empty;
                        
                        var mapGen = _dm.Config.Options.NinjaMapGeneration;
                        tab = mapKind + "maps/" + mapKind + itemBaseType + "-t" + LvlMin + "-" + (mapGen is not null && mapGen.Length > 0 ? mapGen : leagueKind);
                    }
                    break;
                }
            case "flasks":
                {
                    if (item.Flag.Unique)
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
                    if (item.Flag.Unique)
                    {
                        tab = "unique-jewels";
                        useBase = true;
                        useName = true;
                    }
                    else if (itemBaseType.Contain("cluster"))
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
                    if (item.Flag.Unique)
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
                    if (item.Flag.Unique)
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
                if (item.Flag.Unique)
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

            if (useName && useBase)
                tab += itemName + "-" + itemBaseType;
            else if (useName && !useBase)
                tab += itemName;
            else if (!useName && useBase)
                tab += itemBaseType;

            if (useLvl)
            {
                int lvlTemp = int.Parse(LvlMin, CultureInfo.InvariantCulture);
                bool cluster = itemBaseType.Contain("cluster");
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

            if (useInfluence && Influences != Resources.Resources.Main036_None)
            {
                var listInfluence = new Dictionary<string, string>
                {
                    { Resources.Resources.Main037_Shaper,   nameof(Resources.Resources.Main037_Shaper) },
                    { Resources.Resources.Main038_Elder,    nameof(Resources.Resources.Main038_Elder) },
                    { Resources.Resources.Main039_Crusader, nameof(Resources.Resources.Main039_Crusader) },
                    { Resources.Resources.Main040_Redeemer, nameof(Resources.Resources.Main040_Redeemer) },
                    { Resources.Resources.Main041_Hunter,   nameof(Resources.Resources.Main041_Hunter) },
                    { Resources.Resources.Main042_Warlord,  nameof(Resources.Resources.Main042_Warlord) },
                };
                var influence = Influences.Split('/');
                foreach (var inf in influence)
                {
                    if (listInfluence.TryGetValue(inf, out var resourceKey))
                    {
                        tab += "-" + rm.GetString(resourceKey, cultureEn).ToLowerInvariant();
                    }
                }
            }

            if (itemInherit is "gems")
            {
                string addC = string.Empty;
                int lvlTemp = 1;
                if (LvlMin.Length > 0 && int.TryParse(LvlMin, CultureInfo.InvariantCulture, out int val))
                {
                    lvlTemp = val;
                }
                bool awakened = itemName.Contain("awakened");
                bool bigSup = itemName.Contain("empower-support") || itemName.Contain("enlighten-support") || itemName.Contain("enhance-support");
                if (lvlTemp is 6 && awakened)
                {
                    tab += "-6";
                    addC = "c";
                }
                else if (lvlTemp is 4 && bigSup)
                {
                    tab += "-4";
                    addC = "c";
                }
                else if (lvlTemp < 20)
                {
                    tab += "-1";
                }
                else if (lvlTemp is 20)
                    tab += "-20";
                else if (lvlTemp is 21)
                {
                    tab += "-21";
                    addC = "c";
                }

                if (!bigSup)
                {
                    if (int.TryParse(QualMin, NumberStyles.Any, CultureInfo.InvariantCulture, out int qualTemp))
                    {
                        if (qualTemp >= 20 && qualTemp < 23)
                            tab += "-20";
                        else if (qualTemp is 23)
                        {
                            tab += "-23";
                            addC = "c";
                        }
                    }
                }

                if (addC.Length is 0 && itemName.Contain("vaal"))
                {
                    addC = "c";
                }
                tab += addC;

            }

            if (itemName.Contain("a-master-seeks-help"))
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

    private string GetItemName(ItemData item, string itemBaseType, XiletradeItem xiletradeItem)
    {
        StringBuilder sbName = new(item.NameEn.Length > 0 ? item.NameEn : item.TypeEn);
        sbName.Replace(" ", "-").Replace("'", string.Empty).Replace(",", string.Empty).Replace("\"", string.Empty).Replace("ö", "o");
        
        string itemName = sbName.ToString().ToLowerInvariant();
        if (itemName is "voices" && xiletradeItem.ItemFilters.Count is 2)
        {
            var seekFilter = xiletradeItem.ItemFilters.FirstOrDefault(x => x.Id is "explicit.stat_1085446536");
            if (seekFilter?.Min > 1)
            {
                itemName += "-" + seekFilter.Min + "-passives";
            }
            return itemName;
        }
        if (itemName is "vessel-of-vinktar" && xiletradeItem.ItemFilters.Count is 5)
        {
            string stat_attack = "explicit.stat_4292531291";
            string stat_spells = "explicit.stat_4108305628";
            string stat_conv = "explicit.stat_660386148";
            //string stat_pen = "explicit.stat_4164990693";
            List<string> stats = new() { stat_attack, stat_spells, stat_conv, /*stat_pen*/ };

            var seekFilter = xiletradeItem.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
            if (seekFilter is not null)
            {
                itemName += seekFilter.Id == stat_attack ? "-added-attacks"
                    : seekFilter.Id == stat_spells ? "-added-spells"
                    : seekFilter.Id == stat_conv ? "-conversion" : string.Empty;
            }
            return itemName;
        }
        if (itemName is "impresence" && xiletradeItem.ItemFilters.Count > 0)
        {
            string stat_chaos = "explicit.stat_3531280422";
            string stat_physical = "explicit.stat_960081730";
            string stat_fire = "explicit.stat_321077055";
            string stat_lightning = "explicit.stat_1334060246";
            string stat_cold = "explicit.stat_2387423236";
            List<string> stats = new() { stat_chaos, stat_physical, stat_fire, stat_lightning, stat_cold };

            var seekFilter = xiletradeItem.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
            if (seekFilter is not null)
            {
                itemName += seekFilter.Id == stat_chaos ? "-chaos"
                    : seekFilter.Id == stat_physical ? "-physical"
                    : seekFilter.Id == stat_fire ? "-fire"
                    : seekFilter.Id == stat_lightning ? "-lightning"
                    : seekFilter.Id == stat_cold ? "-cold"
                    : string.Empty;
            }
            return itemName;
        }
        if (itemName is "yriels-fostering" && xiletradeItem.ItemFilters.Count > 0)
        {
            string stat_chaos = "explicit.stat_2152491486";
            string stat_physical = "explicit.stat_242822230";
            string stat_speed = "explicit.stat_3597737983";
            List<string> stats = new() { stat_chaos, stat_physical, stat_speed };

            var seekFilter = xiletradeItem.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
            if (seekFilter is not null)
            {
                itemName += seekFilter.Id == stat_chaos ? "-poison"
                    : seekFilter.Id == stat_physical ? "-bleeding"
                    : seekFilter.Id == stat_speed ? "-maim"
                    : string.Empty;
            }
            return itemName;
        }
        if (itemName is "volkuurs-guidance" && xiletradeItem.ItemFilters.Count > 0)
        {
            string stat_cold = "explicit.stat_1917124426";
            string stat_lightning = "explicit.stat_1604984482";
            string stat_fire = "explicit.stat_1985969957";
            List<string> stats = new() { stat_cold, stat_lightning, stat_fire };

            var seekFilter = xiletradeItem.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
            if (seekFilter is not null)
            {
                itemName += seekFilter.Id == stat_cold ? "-cold"
                    : seekFilter.Id == stat_lightning ? "-lightning"
                    : seekFilter.Id == stat_fire ? "-fire"
                    : string.Empty;
            }
            return itemName;
        }
        if (itemBaseType.Contain("cluster"))
        {
            bool isSmallPassive = false;
            int option = -1;
            double passives = 0;
            foreach (var filter in xiletradeItem.ItemFilters)
            {
                if (filter.Id.Contain(Strings.Stat.Generic.PassiveSkill))
                {
                    passives = filter.Max;
                    continue;
                }
                if (filter.Id is Strings.Stat.Option.SmallPassive)
                {
                    isSmallPassive = true;
                    option = filter.Option;
                }
            }

            if (isSmallPassive)
            {
                var options = _dm.FilterEn.Result.SelectMany(r => r.Entries)
                    .FirstOrDefault(e => e.ID == Strings.Stat.Option.SmallPassive)?.Option.Options;
                if (options is not null)
                {
                    var text = options
                        .FirstOrDefault(o => Convert.ToInt32(o.ID) == option)?.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        itemName = new StringBuilder(text)
                            .Replace("%", string.Empty).Replace(" ", "-").Replace('\n', '-')
                            .Append('-').Append(passives).Append("-passives")
                            .ToString().ToLowerInvariant();
                    }
                }
            }
        }
        return itemName;
    }

    //temp
    private string GetNinjaType(string item)
    {
        return BlightMap ? Strings.NinjaTypeOne.BlightedMap
            : BlightRavagedMap ? Strings.NinjaTypeOne.BlightRavagedMap
            : ScourgedMap ? Strings.NinjaTypeOne.ScourgedMap
            : item switch
            {
                "currency" => Strings.NinjaTypeOne.Currency,
                "fragments" => Strings.NinjaTypeOne.Fragment,
                "oils" => Strings.NinjaTypeOne.Oil,
                "incubators" => Strings.NinjaTypeOne.Incubator,
                "invitations" => Strings.NinjaTypeOne.Invitation,
                "scarabs" => Strings.NinjaTypeOne.Scarab,
                "fossils" => Strings.NinjaTypeOne.Fossil,
                "resonators" => Strings.NinjaTypeOne.Resonator,
                "essences" => Strings.NinjaTypeOne.Essence,
                "divination-cards" => Strings.NinjaTypeOne.DivinationCard,
                "prophecies" => Strings.NinjaTypeOne.Prophecy,
                "skill-gems" => Strings.NinjaTypeOne.SkillGem,
                "base-types" => Strings.NinjaTypeOne.BaseType,
                "unique-maps" => Strings.NinjaTypeOne.UniqueMap,
                "maps" => Strings.NinjaTypeOne.Map,
                //"blighted-maps" => Strings.NinjaTypeOne.Map,
                //"blight-ravaged-maps" => Strings.NinjaTypeOne.Map,
                //"scourged-maps" => Strings.NinjaTypeOne.Map,
                "unique-jewels" => Strings.NinjaTypeOne.UniqueJewel,
                "unique-flasks" => Strings.NinjaTypeOne.UniqueFlask,
                "unique-weapons" => Strings.NinjaTypeOne.UniqueWeapon,
                "unique-armours" => Strings.NinjaTypeOne.UniqueArmour,
                "unique-accessories" => Strings.NinjaTypeOne.UniqueAccessory,
                "beasts" => Strings.NinjaTypeOne.Beast,
                "delirium-orbs" => Strings.NinjaTypeOne.DeliriumOrb,
                "vials" => Strings.NinjaTypeOne.Vial,
                "watchstones" => Strings.NinjaTypeOne.Watchstone,
                "cluster-jewels" => Strings.NinjaTypeOne.ClusterJewel,
                "omens" => Strings.NinjaTypeOne.Omen,
                "tattoos" => Strings.NinjaTypeOne.Tattoo,
                "unique-relics" => Strings.NinjaTypeOne.UniqueRelic,
                "coffins" => Strings.NinjaTypeOne.Coffin,
                "allflame-ember" => Strings.NinjaTypeOne.AllflameEmber,
                "kalguuran-runes" => Strings.NinjaTypeOne.Runegraft,
                "memorylines" => Strings.NinjaTypeOne.Memory,
                "artifact" => Strings.NinjaTypeOne.Artifact,
                _ => Strings.NinjaTypeOne.Currency,
            };
    }
}
