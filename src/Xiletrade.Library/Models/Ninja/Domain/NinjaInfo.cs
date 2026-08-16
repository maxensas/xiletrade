using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed record NinjaInfo : NinjaInfoBase
{
    private readonly string _lvlMin;
    private readonly string _qualMin;

    internal string SubType { get; }
    internal bool Map { get; }

    internal NinjaInfo(DataManagerService dm, PoeNinjaService ninja, XiletradeItem xItem, 
        ItemData item, string league) : base(dm, ninja)
    {
        _lvlMin = xItem.Lvl.Min.ToStr();
        _qualMin = xItem.Quality.Min.ToStr();

        League = league;
        Map = item.Flag.Map;

        var subLink = GetSubLink(xItem, item, out bool isForbidden);
        var itemLink = subLink.Split('/');
        if (itemLink.Length is not 3)
        {
            return;
        }
        Link = Strings.UrlPoeNinja + subLink;
        VerifiedLink = true;
        Type = GetNinjaType(item, isForbidden);
        SubType = itemLink[2];
        Url = Strings.ApiNinjaItem + League + "&type=" + Type;
    }

    private string GetSubLink(XiletradeItem xItem, ItemData item, out bool isForbidden)
    {
        bool useBase = false, useName = false, useLvl = false, useInfluence = false;
        var tab = string.Empty;
        var type = string.Empty;
        var itemBaseType = item.TypeEn.Replace(" ", "-").Replace("'", string.Empty)
            .Replace("(", string.Empty).Replace(")", string.Empty).ToLowerInvariant();
        var itemName = GetFormatedNinjaName(xItem, item);
        
        isForbidden = item.Flag.Unique && itemName is "forbidden-flame" or "forbidden-flesh";

        var leagueKind = _dm.League.Result[0].Id.ToLowerInvariant();
        var ninjaLeague = "standard/";

        var leagueSelect = _dm.League.Result.FirstOrDefault(x => x.Text == League);
        if (leagueSelect is not null)
        {
            var leagueUrl = _ninja.GetLeagueUrl(leagueSelect.Text);
            if (leagueUrl.Length > 0)
            {
                leagueKind = leagueUrl;
                ninjaLeague = leagueUrl + "/";
            }
        }

        if (item.Flag.Wombgift)
        {
            tab = "wombgifts";
            useBase = true;
            useLvl = true;
        }
        if (item.Flag.Invitation)
        {
            tab = "invitations";
            useBase = true;
        }
        if (item.Flag.Vial)
        {
            tab = "vials";
            useBase = true;
        }
        if (item.Flag.CapturedBeast)
        {
            tab = "beasts";
            useBase = true;
        }
        if (item.Flag.MapFragment)
        {
            tab = "fragments";
            useBase = true;
        }
        if (item.Flag.Chronicle)
        {
            tab = "temples";
            useName = true;
        }
        if (item.Flag.Gems)
        {
            tab = "skill-gems";
            useBase = true;
        }
        if (item.Flag.Map)
        {
            if (item.Flag.Unique)
            {
                tab = "unique-maps/" + itemName + "-t" + (string.IsNullOrEmpty(_lvlMin) ? 0 : _lvlMin);
            }
            else
            {
                var blight = item.Flag.MapBlightRavaged ? "blight-ravaged-" : item.Flag.MapBlight ? "blighted-" : string.Empty;

                var mapGen = _dm.Config.Options.NinjaMapGeneration;
                var isVaalTemple = itemBaseType.Contain("vaal-temple");
                var suffix = (mapGen is not null && mapGen.Length > 0 ? mapGen : leagueKind);

                bool isGuardian = false;
                var guardian = new MapInfluence(xItem).GuardianName;
                if (!string.IsNullOrEmpty(guardian))
                {
                    guardian = guardian.Replace(" ", "-").ToLowerInvariant() + "-";
                    isGuardian = true;
                }
                tab = blight + "maps/" + guardian + blight + itemBaseType + "-t" + (string.IsNullOrEmpty(_lvlMin) ? 0 : _lvlMin) 
                    + (isGuardian ? "-" : string.Empty) // fix
                    + "-" + (isVaalTemple ? "atlas" : suffix);
            }
        }
        if (item.Flag.Flask)
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
        }
        if (item.Flag.Jewel)
        {
            if (item.Flag.Unique)
            {
                tab = isForbidden ? "forbidden-jewels" : "unique-jewels";
                useBase = !isForbidden;
                useName = true;
            }
            else if (item.Flag.Cluster)
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
        }
        if (item.Flag.Weapon)
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
        }
        if (item.Flag.ArmourPiece)
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
        }
        if (item.Flag.Jewellery)
        {
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
        }
        if (item.Flag.SanctumRelic)
        {
            tab = "unique-relics";
            itemName += "-relic";
            useName = true;
        }
        if (item.Flag.Tincture)
        {
            tab = "unique-tinctures";
            useName = true;
        }

        if (!item.Flag.Map)
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
                int lvlTemp = 1;
                if (int.TryParse(_lvlMin, CultureInfo.InvariantCulture, out int lvl))
                {
                    lvlTemp = lvl;
                }
                if (!item.Flag.Cluster && !item.Flag.Wombgift)
                {
                    tab += lvlTemp <= 82 ? "-82"
                        : lvlTemp == 83 ? "-83"
                        : lvlTemp == 84 ? "-84"
                        : lvlTemp == 85 ? "-85"
                        : lvlTemp >= 86 ? "-86"
                        : string.Empty;
                }
                if (item.Flag.Cluster)
                {
                    tab += lvlTemp >= 84 ? "-84"
                        : lvlTemp >= 75 ? "-75"
                        : lvlTemp >= 68 ? "-68"
                        : lvlTemp >= 50 ? "-50"
                        : "-1";
                }
                if (item.Flag.Wombgift)
                {
                    tab += lvlTemp >= 84 ? "-84"
                        : lvlTemp >= 75 ? "-75"
                        : lvlTemp >= 68 ? "-68"
                        : lvlTemp >= 40 ? "-40"
                        : lvlTemp >= 1 ? "-1"
                        : string.Empty;
                }
            }

            if (item.Flag.Tincture)
            {
                tab += "-tincture";
            }

            if (useInfluence && xItem.IsInfluenced)
            {
                var influences = new[]
                { 
                    (xItem.InfShaper, nameof(Resources.Resources.Main037_Shaper)),
                    (xItem.InfElder, nameof(Resources.Resources.Main038_Elder)),
                    (xItem.InfCrusader, nameof(Resources.Resources.Main039_Crusader)),
                    (xItem.InfRedeemer, nameof(Resources.Resources.Main040_Redeemer)),
                    (xItem.InfHunter, nameof(Resources.Resources.Main041_Hunter)),
                    (xItem.InfWarlord, nameof(Resources.Resources.Main042_Warlord))
                };

                var rm = Resources.Resources.ResourceManager;
                var cult = CultureInfo.InvariantCulture;
                foreach ((var influence, var txt) in influences)
                {
                    if (influence)
                    {
                        tab += "-" + rm.GetString(txt, cult).ToLowerInvariant();
                    }
                }
            }

            if (item.Flag.Gems)
            {
                string addC = string.Empty;
                int lvlTemp = 1;
                if (_lvlMin.Length > 0 && int.TryParse(_lvlMin, CultureInfo.InvariantCulture, out int val))
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
                    if (int.TryParse(_qualMin, NumberStyles.Any, CultureInfo.InvariantCulture, out int qualTemp))
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

            if (isForbidden && xItem.ItemFilters.Count is 1)
            {
                var txt = _dm.Config.Options.Language is 0 ? xItem.ItemFilters[0].Text 
                    : _dm.FilterEn.GetFilterDataEntry(xItem.ItemFilters[0].Id)?.Text;
                if (!string.IsNullOrEmpty(txt))
                {
                    var kind = ExtractForbiddenKind(txt);
                    if (!string.IsNullOrEmpty(kind))
                    {
                        tab += "-" + kind.ToLowerInvariant().Replace(" ", "-");
                    }
                }
            }
        }

        return ninjaLeague + tab;
    }

    private static string ExtractForbiddenKind(ReadOnlySpan<char> text)
    {
        var start = "Allocates";
        var end = "if you have the matching";

        var startIndex = text.IdxOf(start);

        if (startIndex < 0)
            return string.Empty;

        startIndex += start.Length;

        var endIndex = text[startIndex..].IdxOf(end);

        if (endIndex < 0)
            return string.Empty;

        var result = text.Slice(startIndex, endIndex).Trim();

        return result.IsEmpty ? string.Empty : result.ToString();
    }

    private string GetFormatedNinjaName(XiletradeItem xItem, ItemData item)
    {
        var itemName = GetFormatedEnglishName(xItem, item);
        if (item.Flag.Unique)
        {
            if (itemName is "voices" && xItem.ItemFilters.Count is 2)
            {
                var seekFilter = xItem.ItemFilters.FirstOrDefault(x => x.Id is "explicit.stat_1085446536");
                if (seekFilter?.Min > 1)
                {
                    itemName += "-" + seekFilter.Min + "-passives";
                }
                return itemName;
            }
            if (itemName is "vessel-of-vinktar" && xItem.ItemFilters.Count is 5)
            {
                string stat_attack = "explicit.stat_4292531291";
                string stat_spells = "explicit.stat_4108305628";
                string stat_conv = "explicit.stat_660386148";
                //string stat_pen = "explicit.stat_4164990693";
                List<string> stats = new() { stat_attack, stat_spells, stat_conv, /*stat_pen*/ };

                var seekFilter = xItem.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
                if (seekFilter is not null)
                {
                    itemName += seekFilter.Id == stat_attack ? "-added-attacks"
                        : seekFilter.Id == stat_spells ? "-added-spells"
                        : seekFilter.Id == stat_conv ? "-conversion" : string.Empty;
                }
                return itemName;
            }
            if (itemName is "impresence" && xItem.ItemFilters.Count > 0)
            {
                string stat_chaos = "explicit.stat_3531280422";
                string stat_physical = "explicit.stat_960081730";
                string stat_fire = "explicit.stat_321077055";
                string stat_lightning = "explicit.stat_1334060246";
                string stat_cold = "explicit.stat_2387423236";
                List<string> stats = new() { stat_chaos, stat_physical, stat_fire, stat_lightning, stat_cold };

                var seekFilter = xItem.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
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
            if (itemName is "yriels-fostering" && xItem.ItemFilters.Count > 0)
            {
                string stat_chaos = "explicit.stat_2152491486";
                string stat_physical = "explicit.stat_242822230";
                string stat_speed = "explicit.stat_3597737983";
                List<string> stats = new() { stat_chaos, stat_physical, stat_speed };

                var seekFilter = xItem.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
                if (seekFilter is not null)
                {
                    itemName += seekFilter.Id == stat_chaos ? "-poison"
                        : seekFilter.Id == stat_physical ? "-bleeding"
                        : seekFilter.Id == stat_speed ? "-maim"
                        : string.Empty;
                }
                return itemName;
            }
            if (itemName is "volkuurs-guidance" && xItem.ItemFilters.Count > 0)
            {
                string stat_cold = "explicit.stat_1917124426";
                string stat_lightning = "explicit.stat_1604984482";
                string stat_fire = "explicit.stat_1985969957";
                List<string> stats = new() { stat_cold, stat_lightning, stat_fire };

                var seekFilter = xItem.ItemFilters.FirstOrDefault(x => stats.Contains(x.Id));
                if (seekFilter is not null)
                {
                    itemName += seekFilter.Id == stat_cold ? "-cold"
                        : seekFilter.Id == stat_lightning ? "-lightning"
                        : seekFilter.Id == stat_fire ? "-fire"
                        : string.Empty;
                }
                return itemName;
            }
        }
        if (item.Flag.Chronicle && xItem.ItemFilters.Count > 0) // chronicle-of-atzoatl
        {
            List<string> stats = new() { Strings.Stat.Temple.Room17, Strings.Stat.Temple.Room11 }; // dory, locus

            var seekStat = stats.FirstOrDefault(stat => xItem.ItemFilters.Any(x => x.Id.Contains(stat)));
            if (seekStat is not null)
            {
                itemName = seekStat is Strings.Stat.Temple.Room17 ? "locus-of-corruption-tier-3-temple"
                    : seekStat is Strings.Stat.Temple.Room11 ? "doryanis-institute-tier-3-temple"
                    : string.Empty;
            }
            return itemName;
        }
        if (item.Flag.Cluster)
        {
            int passives = 0;
            ItemFilter itemFilter = null;
            foreach (var filter in xItem.ItemFilters)
            {
                if (filter.Id.Contain(Strings.Stat.Generic.PassiveSkill))
                {
                    passives = Convert.ToInt32(filter.Max);
                    continue;
                }
                if (filter.Id.StartWith(Strings.Stat.Option.SmallClusterPassive))
                {
                    itemFilter = filter;
                }
            }
            if (itemFilter is not null)
            {
                var text = _dm.Config.Options.Language is 0 ? itemFilter.Text :
                    _dm.FilterEn.GetFilterDataEntry(itemFilter.Id)?.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    itemName = new StringBuilder(text).Replace("Added Small Passive Skills grant: ", string.Empty)
                        .Replace("%", string.Empty).Replace(" ", "-").Replace('\n', '-')
                        .Append('-').Append(passives).Append("-passives")
                        .ToString().ToLowerInvariant();
                }
            }
        }
        return itemName;
    }

    private string GetFormatedEnglishName(XiletradeItem xItem, ItemData item)
    {
        var name = string.Empty;
        if (!string.IsNullOrEmpty(xItem.UniqueName))
        {
            name = _dm.Config.Options.Language is 0 ? xItem.UniqueName
                : _dm.Words.FindWordByName(xItem.UniqueName)?.NameEn;
        }
        if (string.IsNullOrEmpty(name))
        {
            name = item.NameEn.Length > 0 ? item.NameEn : item.TypeEn;
        }
        return new StringBuilder(name).Replace(" ", "-").Replace("'", string.Empty).Replace(",", string.Empty)
            .Replace("\"", string.Empty).Replace("ö", "o").ToString().ToLowerInvariant();
    }

    private static string GetNinjaType(ItemData item, bool isForbidden)
    {
        return isForbidden ? Strings.NinjaTypeOne.ForbiddenJewel
            : item.Flag.MapBlightRavaged ? Strings.NinjaTypeOne.BlightRavagedMap
            : item.Flag.MapBlight ? Strings.NinjaTypeOne.BlightedMap
            : item.Flag.Map && !item.Flag.Unique ? Strings.NinjaTypeOne.Map
            : item.Flag.Map && item.Flag.Unique ? Strings.NinjaTypeOne.UniqueMap
            : item.Flag.Cluster && !item.Flag.Unique ? Strings.NinjaTypeOne.ClusterJewel
            : item.Flag.CapturedBeast ? Strings.NinjaTypeOne.Beast
            : item.Flag.Chronicle ? Strings.NinjaTypeOne.IncursionTemple
            : item.Flag.Jewel && item.Flag.Unique ? Strings.NinjaTypeOne.UniqueJewel
            : item.Flag.Flask && item.Flag.Unique ? Strings.NinjaTypeOne.UniqueFlask
            : item.Flag.Weapon && item.Flag.Unique ? Strings.NinjaTypeOne.UniqueWeapon
            : item.Flag.ArmourPiece && item.Flag.Unique ? Strings.NinjaTypeOne.UniqueArmour
            : item.Flag.Jewellery && item.Flag.Unique ? Strings.NinjaTypeOne.UniqueAccessory
            : item.Flag.SanctumRelic && item.Flag.Unique ? Strings.NinjaTypeOne.UniqueRelic
            : item.Flag.Tincture && item.Flag.Unique ? Strings.NinjaTypeOne.UniqueTincture
            : item.Flag.Wombgift ? Strings.NinjaTypeOne.Wombgift
            : item.Flag.Gems ? Strings.NinjaTypeOne.SkillGem
            : item.Flag.Invitation ? Strings.NinjaTypeOne.Invitation
            : item.Flag.Vial ? Strings.NinjaTypeOne.Vial
            : Strings.NinjaTypeOne.BaseType;
    }
}
