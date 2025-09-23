using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Models.Ninja.Domain;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class NinjaViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private readonly MainViewModel _vm;
    private readonly DataManagerService _dm;
    private readonly PoeNinjaService _ninja;

    [ObservableProperty]
    private string price;

    [ObservableProperty]
    private double valWidth = 0;

    [ObservableProperty]
    private double btnWidth = 0;

    [ObservableProperty]
    private string imageName;

    [ObservableProperty]
    private string imgLeftRightMargin;

    public NinjaViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _vm = _serviceProvider.GetRequiredService<MainViewModel>();
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
        _ninja = _serviceProvider.GetRequiredService<PoeNinjaService>();
    }

    /// <summary>
    /// Get the generated poeninja URL of the item.
    /// </summary>
    /// <returns></returns>
    internal string GetFullUrl() => 
        Strings.UrlPoeNinja + GetLink(GetNinjaInfo(), _vm.Form.GetXiletradeItem());

    /// <summary>
    /// Try to update poeninja price with the given parameter and refresh poeninja data cache.
    /// </summary>
    /// <param name="xiletradeItem"></param>
    internal Task TryUpdatePriceTask(XiletradeItem xiletradeItem)
    {
        return Task.Run(() =>
        {
            try
            {
                var nInfo = GetNinjaInfo();
                var item = GetLink(nInfo, xiletradeItem).Split('/');
                if (item.Length is not 3)
                {
                    return;
                }

                bool allflame = item[2].StartsWith("allflame");
                bool apiKind = !(item[1] is "currency" or "fragments" && !allflame);
                string ninjaApi = apiKind ? Strings.ApiNinjaItem : Strings.ApiNinjaCur;
                string type = item[1] switch
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
                    "blighted-maps" => Strings.NinjaTypeOne.Map,
                    "blight-ravaged-maps" => Strings.NinjaTypeOne.Map,
                    "scourged-maps" => Strings.NinjaTypeOne.Map,
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
                    //"allflame-embers" => "AllflameEmber",
                    "kalguuran-runes" => Strings.NinjaTypeOne.Runegraft,
                    "memorylines" => Strings.NinjaTypeOne.Memory,
                    "artifact" => Strings.NinjaTypeOne.Artifact,
                    _ => Strings.NinjaTypeOne.Currency,
                };

                if (allflame)
                {
                    type = Strings.NinjaTypeOne.AllflameEmber;
                }

                if (type is Strings.NinjaTypeOne.Map && item.Length >= 2)
                {
                    type = item[2].StartWith("blighted") ? Strings.NinjaTypeOne.BlightedMap
                        : item[2].StartWith("blight-ravaged") ? Strings.NinjaTypeOne.BlightRavagedMap
                        : nInfo.ScourgedMap ? Strings.NinjaTypeOne.ScourgedMap : type;
                }

                string url = ninjaApi + nInfo.League + "&type=" + type;

                NinjaValue ninja = new();
                if (apiKind)
                {
                    var jsonItem = _ninja.GetNinjaItem<NinjaItemContract>(nInfo.League, type, url);
                    if (jsonItem is null)
                    {
                        return;
                    }
                    var line = jsonItem.Lines.FirstOrDefault(
                        x => allflame ? x.Id.StartWith(item[2]) : x.Id == item[2]);
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
                    var jsonItem = _ninja.GetNinjaItem<NinjaCurrencyContract>(nInfo.League, type, url);
                    if (jsonItem is null)
                    {
                        return;
                    }
                    var line = jsonItem.Lines.FirstOrDefault(x => x.Id == item[2]);
                    if (line is not null)
                    {
                        ninja.Id = line.Id;
                        ninja.Name = line.Name;
                        ninja.ChaosPrice = line.ChaosPrice;
                    }
                }

                if (ninja.ChaosPrice > 0)
                {
                    double value = ninja.DivinePrice > 1 ? Math.Round(ninja.DivinePrice, 1) : Math.Round(ninja.ChaosPrice, 1);
                    ImageName = ninja.DivinePrice > 1 ? "divine" : "chaos";

                    string valueString = value.ToString();
                    double nbDigit = valueString.Length - 1;
                    double charLength = 6;
                    double leftPad = 63 + nbDigit * charLength;
                    double rightPad = 38 - nbDigit * charLength;

                    ImgLeftRightMargin = leftPad + "." + rightPad;
                    Price = valueString;
                    ValWidth = 76 + nbDigit * charLength;
                    BtnWidth = 90 + nbDigit * charLength;
                    _vm.Form.Visible.Ninja = true;
                }
                else
                {
                    _vm.Form.Visible.Ninja = false;
                }
            }
            catch//(WebException ex)
            {
                // we can't know if task goes wrong.
            }
        });
    }

    /// <summary>
    /// Get the chaos equivalent of the currency item.
    /// </summary>
    /// <param name="league"></param>
    /// <param name="NameCur"></param>
    /// <param name="tier"></param>
    /// <returns></returns>
    internal double GetChaosEq(string league, string NameCur, string tier)
    {
        double error = -1;
        string type = GetNinjaType(NameCur);
        if (type is not null)
        {
            var isCurrency = type is Strings.NinjaTypeOne.Currency or Strings.NinjaTypeOne.Fragment;
            string api = isCurrency ? Strings.ApiNinjaCur : Strings.ApiNinjaItem;
            string urlNinja = api + league + "&type=" + type;

            if (isCurrency)
            {
                var currency = _ninja.GetNinjaItem<NinjaCurrencyContract>(league, type, urlNinja);
                if (currency is not null)
                {
                    var line = currency.Lines.FirstOrDefault(x => x.Name == NameCur);
                    return line is not null ? line.ChaosPrice : error;
                }
                return error;
            }
            var item = _ninja.GetNinjaItem<NinjaItemContract>(league, type, urlNinja);
            if (item is not null)
            {
                if (type is Strings.NinjaTypeOne.Map && tier is not null)
                {
                    var line = item.Lines.FirstOrDefault(x => x.Name == NameCur && x.Id.Contain("-" + tier + "-"));
                    return line is not null ? line.ChaosPrice : error;
                }
                if (type is Strings.NinjaTypeOne.UniqueMap)
                {
                    var split = NameCur.Split('(');
                    if (split.Length is 2)
                    {
                        string mapName = split[0].Trim();
                        string tierUnique = "-t" + split[1].Replace("Tier ", string.Empty).Replace(")", string.Empty).Trim();
                        var line = item.Lines.FirstOrDefault(x => x.Name == mapName && x.Id.EndWith(tierUnique));
                        return line is not null ? line.ChaosPrice : error;
                    }
                }
                var lineDef = item.Lines.FirstOrDefault(x => x.Name == NameCur);
                return lineDef is not null ? lineDef.ChaosPrice : error;
            }
        }
        return error;
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

    private NinjaInfo GetNinjaInfo()
    {
        string influences = _vm.Form.Influence.GetSate("/");
        string level = string.Empty, quality = string.Empty;
        if (influences.Length is 0) influences = Resources.Resources.Main036_None;
        var iLvl = _vm.Form.Panel.Row.FirstRow.FirstOrDefault(x => x.Id is StatPanel.CommonItemLevel);
        if (iLvl is not null && iLvl.Min.Length > 0)
        {
            level = iLvl.Min.Trim();
        }
        var qual = _vm.Form.Panel.Row.FirstRow.FirstOrDefault(x => x.Id is StatPanel.CommonQuality);
        if (qual is not null && qual.Min.Length > 0)
        {
            quality = qual.Min.Trim();
        }
        return new NinjaInfo(_vm.Form.League[_vm.Form.LeagueIndex]
            , _vm.Form.Rarity.Item
            , level
            , quality
            , _vm.Form.Panel.SynthesisBlight
            , _vm.Form.Panel.BlighRavaged
            , influences);
    }

    private string GetLink(NinjaInfo nInfo, XiletradeItem xiletradeItem)
    {
        string tab = string.Empty;
        bool useBase = false, useName = false, useLvl = false, useInfluence = false, is_unique = false;
        var item = _vm.Item;
        StringBuilder sbName = new(item.NameEn.Length > 0 ? item.NameEn : item.TypeEn);
        sbName.Replace(" ", "-").Replace("'", string.Empty).Replace(",", string.Empty).Replace("\"", string.Empty).Replace("ö", "o"); // maybe use sb in whole method
        string itemName = sbName.ToString().ToLowerInvariant();
        string itemBaseType = item.TypeEn.Replace(" ", "-").Replace("'", string.Empty).ToLowerInvariant();
        string itemInherit = item.Inherits.Split('/')[0].ToLowerInvariant();
        if (itemInherit.Length is 0)
        {
            itemInherit = itemBaseType;
        }
        // do stringbuilder for itemName and other strings
        if (itemName is "voices" && xiletradeItem.ItemFilters.Count is 2)
        {
            var seekFilter = xiletradeItem.ItemFilters.FirstOrDefault(x => x.Id is "explicit.stat_1085446536");
            if (seekFilter is not null)
            {
                if (seekFilter.Min > 1)
                {
                    itemName += "-" + seekFilter.Min + "-passives";
                }
            }
        }
        else if (itemName is "vessel-of-vinktar" && xiletradeItem.ItemFilters.Count is 5)
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
        }
        else if (itemName is "impresence" && xiletradeItem.ItemFilters.Count > 0)
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
        }
        else if (itemName is "yriels-fostering" && xiletradeItem.ItemFilters.Count > 0)
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
        }
        else if (itemName is "volkuurs-guidance" && xiletradeItem.ItemFilters.Count > 0)
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
        }
        else if (itemBaseType.Contain("cluster"))
        {
            // fucking ugly, to redo with linq request
            bool doIt = false;
            int option = -1;
            double passives = 0;
            foreach (var filter in xiletradeItem.ItemFilters)
            {
                if (filter.Id.Contain(Strings.Stat.Generic.PassiveSkill))
                {
                    passives = filter.Max;
                }
                else if (filter.Id is Strings.Stat.Option.SmallPassive)
                {
                    doIt = true;
                    option = filter.Option;
                }
            }

            if (doIt)
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

        int idLang = _dm.Config.Options.Language;
        if (nInfo.Rarity == Resources.Resources.General006_Unique) is_unique = true;

        string leagueKind = _dm.League.Result[0].Id.ToLowerInvariant();
        string ninjaLeague = "standard/";

        var leagueSelect = _dm.League.Result.FirstOrDefault(x => x.Text == nInfo.League);
        if (leagueSelect is not null)
        {
            var league = _dm.NinjaState.Leagues.Where(x => x.Name == leagueSelect.Text).FirstOrDefault();
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
                        : itemName.Contain("simulacrum")
                        || itemName.Contain("splinter") && itemName.Contain("timeless") ? "fragments"
                        : itemName.Contain("delirium-orb") ? "delirium-orbs"
                        : itemName.Contain("vial-of") ? "vials"
                        : itemName.Contain("omen") ? "omens"
                        : itemName.Contain("tattoo") ? "tattoos"
                        : itemName.Contain("rune") ? "kalguuran-runes"
                        : itemName.Contain("resonator") ? "resonators"
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
                    if (is_unique)
                    {
                        tab = "unique-maps/" + itemName + "-t" + nInfo.LvlMin;
                    }
                    else
                    {
                        string mapKind = nInfo.SynthBlight && !itemBaseType.Contain("blighted") ? "blighted-"
                            : nInfo.Ravaged && !itemBaseType.Contain("blight") ? "blight-ravaged-"
                            : nInfo.ScourgedMap ? "scourged-"
                            : string.Empty;
                        //scourged
                        var mapGen = _dm.Config.Options.NinjaMapGeneration;
                        tab = mapKind + "maps/" + mapKind + itemBaseType + "-t" + nInfo.LvlMin + "-" + (mapGen is not null && mapGen.Length > 0 ? mapGen : leagueKind);
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

            if (itemInherit is "gems" && nInfo.AltIdx > 0)
            {
                tab += nInfo.AltIdx is 1 ? "anomalous-"
                    : nInfo.AltIdx is 2 ? "divergent-"
                    : nInfo.AltIdx is 3 ? "phantasmal-"
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
                int lvlTemp = int.Parse(nInfo.LvlMin, CultureInfo.InvariantCulture);
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

            if (useInfluence && nInfo.Influences != Resources.Resources.Main036_None)
            {
                var influence = nInfo.Influences.Split('/');
                for (int i = 0; i < influence.Length; i++)
                {
                    tab += influence[i] == Resources.Resources.Main037_Shaper ? "-" + rm.GetString("Main037_Shaper", cultureEn).ToLowerInvariant()
                        : influence[i] == Resources.Resources.Main038_Elder ? "-" + rm.GetString("Main038_Elder", cultureEn).ToLowerInvariant()
                        : influence[i] == Resources.Resources.Main039_Crusader ? "-" + rm.GetString("Main039_Crusader", cultureEn).ToLowerInvariant()
                        : influence[i] == Resources.Resources.Main040_Redeemer ? "-" + rm.GetString("Main040_Redeemer", cultureEn).ToLowerInvariant()
                        : influence[i] == Resources.Resources.Main041_Hunter ? "-" + rm.GetString("Main041_Hunter", cultureEn).ToLowerInvariant()
                        : influence[i] == Resources.Resources.Main042_Warlord ? "-" + rm.GetString("Main042_Warlord", cultureEn).ToLowerInvariant()
                        : string.Empty;
                }
            }

            if (itemInherit is "gems")
            {
                string addC = string.Empty;
                int lvlTemp = int.Parse(nInfo.LvlMin, CultureInfo.InvariantCulture);
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
                else if (lvlTemp == 20)
                    tab += "-20";
                else if (lvlTemp == 21)
                {
                    tab += "-21";
                    addC = "c";
                }

                if (!bigSup)
                {
                    if (int.TryParse(nInfo.QualMin, NumberStyles.Any, CultureInfo.InvariantCulture, out int qualTemp))
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

    private string GetNinjaType(string NameCur)
    {
        var curId = _dm.CurrenciesEn.Where(currency => currency.Entries.Any(entry => entry.Text == NameCur))
            .Select(currency => currency.Id);
        if (curId.Any())
        {
            if (curId.First().Contain(Strings.CurrencyTypePoe1.Maps))
            {
                return Strings.NinjaTypeOne.Map;
            }
            var cur = curId.First();
            return cur is "Currency" or "Catalysts" or "ExoticCurrency" ? Strings.NinjaTypeOne.Currency
                : cur is "Splinters" or "Fragments" ? Strings.NinjaTypeOne.Fragment
                : cur is "DeliriumOrbs" ? Strings.NinjaTypeOne.DeliriumOrb
                : cur is "Oils" ? Strings.NinjaTypeOne.Oil
                : cur is "Incubators" ? Strings.NinjaTypeOne.Incubator
                : cur is "Scarabs" ? Strings.NinjaTypeOne.Scarab
                : cur is "DelveResonators" ? Strings.NinjaTypeOne.Resonator
                : cur is "DelveFossils" ? Strings.NinjaTypeOne.Fossil
                : cur is "Essences" ? Strings.NinjaTypeOne.Essence
                : cur is "Cards" ? Strings.NinjaTypeOne.DivinationCard
                : cur is "Prophecies" ? Strings.NinjaTypeOne.Prophecy
                : cur is "MapsUnique" ? Strings.NinjaTypeOne.UniqueMap
                : cur is "MapsBlighted" ? Strings.NinjaTypeOne.BlightedMap
                : cur is "Runes" ? Strings.NinjaTypeOne.Runegraft
                : cur is "MemoryLine" ? Strings.NinjaTypeOne.Memory
                : cur is "Artifact" ? Strings.NinjaTypeOne.Artifact
                : cur is "Ancestor" ? NameCur.StartWith("Omen") ? Strings.NinjaTypeOne.Omen : Strings.NinjaTypeOne.Tattoo
                //: cur is "Expedition" or "Misc" ? null
                : null;
        }
        return null;
    }
}
