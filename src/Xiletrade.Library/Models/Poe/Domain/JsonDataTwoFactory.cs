using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Two;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class JsonDataTwoFactory
{
    private readonly DataManagerService _dm;

    public JsonDataTwoFactory(DataManagerService dm)
    {
        _dm = dm;
    }

    /// <summary>
    /// unidentified unique
    /// </summary>
    /// <param name="xiletradeItem"></param>
    /// <param name="unid"></param>
    /// <param name="market"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    public JsonDataTwo Create(XiletradeItem xiletradeItem, UniqueUnidentified unid, string market, string search)
    {
        var json = new JsonDataTwo();

        OptionTxt optTrue = new("true"), optFalse = new("false");

        // Sort
        json.Sort.Price = "asc";

        // Query
        json.Query.Status = new(market);

        if (!string.IsNullOrEmpty(search))
        {
            json.Query.Term = search;
        }
        else if (unid is not null)
        {
            json.Query.Name = unid.Name;
            json.Query.Type = unid.Type;
        }

        // Corrupted / Identified
        if (xiletradeItem.Corrupted is "true")
            json.Query.Filters.Misc.Filters.Corrupted = optTrue;
        if (xiletradeItem.Corrupted is "false")
            json.Query.Filters.Misc.Filters.Corrupted = optFalse;

        if (xiletradeItem.Identified is "true")
            json.Query.Filters.Misc.Filters.Identified = optTrue;
        if (xiletradeItem.Identified is "false")
            json.Query.Filters.Misc.Filters.Identified = optFalse;

        if (json.Query.Filters.Misc.Filters.Identified is not null
            || json.Query.Filters.Misc.Filters.Corrupted is not null)
            json.Query.Filters.Misc.Disabled = false;

        // Rarity
        ApplyRarityFilter(json.Query.Filters.Type, xiletradeItem);

        // Trade filters
        json.Query.Filters.Trade.Disabled = _dm.Config.Options.SearchBeforeDay is 0;

        if (_dm.Config.Options.SearchBeforeDay is not 0)
            json.Query.Filters.Trade.Filters.Indexed =
                new(BeforeDayToString(_dm.Config.Options.SearchBeforeDay));

        json.Query.Filters.Trade.Filters.SaleType = new("priced");

        return json;
    }

    /// <summary>
    /// normal JSON
    /// </summary>
    /// <param name="xiletradeItem"></param>
    /// <param name="item"></param>
    /// <param name="useSaleType"></param>
    /// <param name="market"></param>
    /// <returns></returns>
    public JsonDataTwo Create(XiletradeItem xiletradeItem, ItemData item, bool useSaleType, string market)
    {
        var json = new JsonDataTwo();

        OptionTxt optTrue = new("true"), optFalse = new("false");

        // Sort
        json.Sort.Price = "asc";

        // Status
        json.Query.Status = new(market);

        // Name / Type
        bool simpleMode = xiletradeItem.ByType || item.Name.Length is 0
            || (!item.Flag.Unique && !item.Flag.FoilVariant);

        if (!simpleMode)
        {
            json.Query.Name = item.Name;
            json.Query.Type = item.Type;
        }
        else if (!xiletradeItem.ByType)
        {
            json.Query.Type = item.Type;
        }

        // Filters
        json.Query.Filters.Trade = GetTradeFilters(_dm, xiletradeItem, useSaleType);
        json.Query.Filters.Equipment = GetEquipmentFilters(xiletradeItem);
        json.Query.Filters.Requirement = GetRequirementFilters(xiletradeItem);
        json.Query.Filters.Map = GetMapFilters(xiletradeItem, item);
        json.Query.Filters.Misc = GetMiscFilters(xiletradeItem, item, optTrue, optFalse);
        json.Query.Filters.Type = GetTypeFilters(xiletradeItem, item);

        // Stats
        json.Query.Stats = GetStats(_dm.Filter, xiletradeItem, item.Flag.Weapon);

        return json;
    }

    private static void ApplyRarityFilter(TypeTwo type, XiletradeItem xiletradeItem)
    {
        string rarityEn = GetEnglishRarity(xiletradeItem.Rarity);
        if (rarityEn.Length > 0)
        {
            rarityEn = rarityEn switch
            {
                "Any N-U" => "nonunique",
                "Foil Unique" => "uniquefoil",
                _ => rarityEn.ToLowerInvariant()
            };

            if (rarityEn is not Strings.any)
                type.Filters.Rarity = new(rarityEn);
        }
    }

    private static TypeTwo GetTypeFilters(XiletradeItem xiletradeItem, ItemData item)
    {
        TypeTwo type = new() { Disabled = false };

        string rarityEn = GetEnglishRarity(xiletradeItem.Rarity);
        if (rarityEn.Length > 0)
        {
            rarityEn = rarityEn is "Any N-U" ? "nonunique"
                : rarityEn is "Foil Unique" ? "uniquefoil"
                : rarityEn.ToLowerInvariant();
            if (rarityEn is not Strings.any)
            {
                type.Filters.Rarity = new(rarityEn);
            }
        }
        var category = item.Flag.GetItemCategoryApi();
        if (category.Length > 0)
        {
            type.Filters.Category = new(category);
        }
        if (xiletradeItem.ChkQuality)
        {
            if (xiletradeItem.QualityMin.IsNotEmpty())
                type.Filters.Quality.Min = xiletradeItem.QualityMin;
            if (xiletradeItem.QualityMax.IsNotEmpty())
                type.Filters.Quality.Max = xiletradeItem.QualityMax;
        }
        var useIlvl = item.Flag.Weapon || item.Flag.ArmourPiece || item.Flag.Amulets
            || item.Flag.Belts || item.Flag.Rings || item.Flag.Quivers || item.Flag.UncutGem;
        if (xiletradeItem.ChkLv && useIlvl)
        {
            if (xiletradeItem.LvMin.IsNotEmpty())
                type.Filters.ItemLevel.Min = xiletradeItem.LvMin;
            if (xiletradeItem.LvMax.IsNotEmpty())
                type.Filters.ItemLevel.Max = xiletradeItem.LvMax;
        }

        return type;
    }

    private static MiscTwo GetMiscFilters(XiletradeItem xiletradeItem, ItemData item, OptionTxt optTrue, OptionTxt optFalse)
    {
        MiscTwo misc = new();

        var checkLvl = xiletradeItem.ChkLv && (item.Flag.Gems || item.Flag.Logbook);
        var checkCorrupted = xiletradeItem.Corrupted is not Strings.any;
        var uniqueUnidJewel = item.Flag.Jewel && item.Flag.Unique && item.Flag.Unidentified;

        if (checkLvl || checkCorrupted || uniqueUnidJewel)
        {
            if (item.Flag.Gems)
            {
                if (xiletradeItem.LvMin.IsNotEmpty())
                    misc.Filters.GemLevel.Min = xiletradeItem.LvMin;
                if (xiletradeItem.LvMax.IsNotEmpty())
                    misc.Filters.GemLevel.Max = xiletradeItem.LvMax;

                if (xiletradeItem.ChkGemSockets)
                {
                    if (xiletradeItem.LvMin.IsNotEmpty())
                        misc.Filters.GemSockets.Min = xiletradeItem.GemSocketsMin;
                    if (xiletradeItem.LvMax.IsNotEmpty())
                        misc.Filters.GemSockets.Max = xiletradeItem.GemSocketsMax;
                }
            }
            if (item.Flag.Logbook)
            {
                if (xiletradeItem.LvMin.IsNotEmpty())
                    misc.Filters.AreaLevel.Min = xiletradeItem.LvMin;
                if (xiletradeItem.LvMax.IsNotEmpty())
                    misc.Filters.AreaLevel.Max = xiletradeItem.LvMax;
            }

            if (xiletradeItem.Corrupted is "true")
                misc.Filters.Corrupted = optTrue;
            if (xiletradeItem.Corrupted is "false")
                misc.Filters.Corrupted = optFalse;

            if (uniqueUnidJewel)
            {
                misc.Filters.Identified = optFalse;
            }
            //TODO
            /*
            Query.Filters.Misc.Filters.UnidentifiedTier
            Query.Filters.Misc.Filters.GemSockets
            Query.Filters.Misc.Filters.BaryaSacredWater
            Query.Filters.Misc.Filters.StackSize
            Query.Filters.Misc.Filters.Identified
            Query.Filters.Misc.Filters.Mirrored
            */

            misc.Disabled = false;
        }

        return misc;
    }

    private static TradeTwo GetTradeFilters(DataManagerService dm, XiletradeItem xiletradeItem, bool useSaleType)
    {
        TradeTwo trade = new()
        {
            Disabled = dm.Config.Options.SearchBeforeDay is 0
        };

        if (dm.Config.Options.SearchBeforeDay is not 0)
        {
            trade.Filters.Indexed = new(BeforeDayToString(dm.Config.Options.SearchBeforeDay));
        }
        if (useSaleType)
        {
            trade.Filters.SaleType = new("priced");
        }
        if (xiletradeItem.PriceMin > 0 && xiletradeItem.PriceMin.IsNotEmpty())
        {
            trade.Filters.Price.Min = xiletradeItem.PriceMin;
        }
        if (xiletradeItem.ExaltOnly && !xiletradeItem.ChaosOnly)
        {
            trade.Disabled = false;
            trade.Filters.Price.Option = "exalted";
        }
        if (xiletradeItem.ChaosOnly && !xiletradeItem.ExaltOnly)
        {
            trade.Disabled = false;
            trade.Filters.Price.Option = "chaos";
        }
        //TODO: Query.Filters.Trade.Filters.Collapse
        return trade;
    }

    private static MapTwo GetMapFilters(XiletradeItem xiletradeItem, ItemData item)
    {
        MapTwo map = new();

        if (xiletradeItem.ChkLv && item.Flag.Waystones)
        {
            map.Disabled = false;

            var tierMin = xiletradeItem.LvMin.IsNotEmpty();
            var tierMax = xiletradeItem.LvMax.IsNotEmpty();
            if (tierMin || tierMax)
            {
                map.Filters.Tier = new();
                if (tierMin)
                    map.Filters.Tier.Min = xiletradeItem.LvMin;
                if (tierMax)
                    map.Filters.Tier.Max = xiletradeItem.LvMax;
            }

            var iiqMin = xiletradeItem.MapItemQuantityMin.IsNotEmpty();
            var iiqMax = xiletradeItem.MapItemQuantityMax.IsNotEmpty();
            if (iiqMin || iiqMax)
            {
                map.Filters.Quantity = new();
                if (iiqMin)
                    map.Filters.Quantity.Min = xiletradeItem.MapItemQuantityMin;
                if (iiqMax)
                    map.Filters.Quantity.Max = xiletradeItem.MapItemQuantityMax;
            }

            var iirMin = xiletradeItem.MapItemRarityMin.IsNotEmpty();
            var iirMax = xiletradeItem.MapItemRarityMax.IsNotEmpty();
            if (iirMin || iirMax)
            {
                map.Filters.Rarity = new();
                if (iirMin)
                    map.Filters.Rarity.Min = xiletradeItem.MapItemRarityMin;
                if (iirMax)
                    map.Filters.Rarity.Max = xiletradeItem.MapItemRarityMax;
            }

            var packMin = xiletradeItem.MapPackSizeMin.IsNotEmpty();
            var packMax = xiletradeItem.MapPackSizeMax.IsNotEmpty();
            if (packMin || packMax)
            {
                map.Filters.PackSize = new();
                if (packMin)
                    map.Filters.PackSize.Min = xiletradeItem.MapPackSizeMin;
                if (packMax)
                    map.Filters.PackSize.Max = xiletradeItem.MapPackSizeMax;
            }

            var rareMin = xiletradeItem.MapRareMonsterMin.IsNotEmpty();
            var rareMax = xiletradeItem.MapRareMonsterMax.IsNotEmpty();
            if (rareMin || rareMax)
            {
                map.Filters.RareMonsters = new();
                if (rareMin)
                    map.Filters.RareMonsters.Min = xiletradeItem.MapRareMonsterMin;
                if (rareMax)
                    map.Filters.RareMonsters.Max = xiletradeItem.MapRareMonsterMax;
            }

            var magicMin = xiletradeItem.MapMagicMonsterMin.IsNotEmpty();
            var magicMax = xiletradeItem.MapMagicMonsterMax.IsNotEmpty();
            if (magicMin || magicMax)
            {
                map.Filters.MagicMonsters = new();
                if (magicMin)
                    map.Filters.MagicMonsters.Min = xiletradeItem.MapMagicMonsterMin;
                if (magicMax)
                    map.Filters.MagicMonsters.Max = xiletradeItem.MapMagicMonsterMax;
            }
        }
        return map;
    }

    private static Requirement GetRequirementFilters(XiletradeItem xiletradeItem)
    {
        Requirement requirement = new();
        if (xiletradeItem.ChkReqLevel)
        {
            requirement.Disabled = false;

            if (xiletradeItem.ReqLevelMin.IsNotEmpty())
                requirement.Filters.Level.Min = xiletradeItem.ReqLevelMin;
            if (xiletradeItem.ReqLevelMax.IsNotEmpty())
                requirement.Filters.Level.Max = xiletradeItem.ReqLevelMax;
        }
        return requirement;
    }

    private static Equipment GetEquipmentFilters(XiletradeItem xiletradeItem)
    {
        Equipment equipment = new();

        if (xiletradeItem.ChkArmour || xiletradeItem.ChkEnergy || xiletradeItem.ChkEvasion
                    || xiletradeItem.ChkDpsTotal || xiletradeItem.ChkDpsPhys || xiletradeItem.ChkDpsElem
                    || xiletradeItem.ChkRuneSockets)
        {
            equipment.Disabled = false;

            if (xiletradeItem.ChkArmour)
            {
                if (xiletradeItem.ArmourMin.IsNotEmpty())
                    equipment.Filters.Armour.Min = xiletradeItem.ArmourMin;
                if (xiletradeItem.ArmourMax.IsNotEmpty())
                    equipment.Filters.Armour.Max = xiletradeItem.ArmourMax;
            }
            if (xiletradeItem.ChkEnergy)
            {
                if (xiletradeItem.EnergyMin.IsNotEmpty())
                    equipment.Filters.EnergyShield.Min = xiletradeItem.EnergyMin;
                if (xiletradeItem.EnergyMax.IsNotEmpty())
                    equipment.Filters.EnergyShield.Max = xiletradeItem.EnergyMax;
            }
            if (xiletradeItem.ChkEvasion)
            {
                if (xiletradeItem.EvasionMin.IsNotEmpty())
                    equipment.Filters.Evasion.Min = xiletradeItem.EvasionMin;
                if (xiletradeItem.EvasionMax.IsNotEmpty())
                    equipment.Filters.Evasion.Max = xiletradeItem.EvasionMax;
            }
            if (xiletradeItem.ChkDpsTotal)
            {
                if (xiletradeItem.DpsTotalMin.IsNotEmpty())
                    equipment.Filters.DamagePerSecond.Min = xiletradeItem.DpsTotalMin;
                if (xiletradeItem.DpsTotalMax.IsNotEmpty())
                    equipment.Filters.DamagePerSecond.Max = xiletradeItem.DpsTotalMax;
            }
            if (xiletradeItem.ChkDpsPhys)
            {
                if (xiletradeItem.DpsPhysMin.IsNotEmpty())
                    equipment.Filters.PhysicalDps.Min = xiletradeItem.DpsPhysMin;
                if (xiletradeItem.DpsPhysMax.IsNotEmpty())
                    equipment.Filters.PhysicalDps.Max = xiletradeItem.DpsPhysMax;
            }
            if (xiletradeItem.ChkDpsElem)
            {
                if (xiletradeItem.DpsElemMin.IsNotEmpty())
                    equipment.Filters.ElementalDps.Min = xiletradeItem.DpsElemMin;
                if (xiletradeItem.DpsElemMax.IsNotEmpty())
                    equipment.Filters.ElementalDps.Max = xiletradeItem.DpsElemMax;
            }
            if (xiletradeItem.ChkRuneSockets)
            {
                if (xiletradeItem.RuneSocketsMin.IsNotEmpty())
                    equipment.Filters.RuneSockets.Min = xiletradeItem.RuneSocketsMin;
                if (xiletradeItem.RuneSocketsMax.IsNotEmpty())
                    equipment.Filters.RuneSockets.Max = xiletradeItem.RuneSocketsMax;
            }

            //TODO
            /*
            equipment.Filters.Damage
            equipment.Filters.EmptyRuneSockets
            equipment.Filters.CriticalChance
            equipment.Filters.AttacksPerSecond
            equipment.Filters.Block
            equipment.Filters.Spirit
            */
        }
        return equipment;
    }

    private static Stats[] GetStats(FilterData filterData, XiletradeItem xiletradeItem, bool isWeapon)
    {
        Stats[] stats = [];
        bool errorsFilters = false;
        if (xiletradeItem.ItemFilters.Count > 0)
        {
            stats = new Stats[1];
            stats[0] = new()
            {
                Type = "and",
                Filters = new StatsFilters[xiletradeItem.ItemFilters.Count]
            };

            int idx = 0;
            for (int i = 0; i < xiletradeItem.ItemFilters.Count; i++)
            {
                string input = xiletradeItem.ItemFilters[i].Text;
                string id = xiletradeItem.ItemFilters[i].Id;
                string type = xiletradeItem.ItemFilters[i].Id.Split('.')[0];
                if (input.Trim().Length > 0)
                {
                    string type_name = GetAffixType(type);

                    if (type_name.Length is 0)
                    {
                        continue; // will create a bad request as intended (to detect new type) and not crash the app 
                    }

                    FilterResultEntrie filter = null;

                    var filterResult = filterData.Result.FirstOrDefault(x => x.Label == type_name);
                    type_name = type_name.ToLowerInvariant();
                    input = Regex.Escape(input).Replace("\\+\\#", "[+]?\\#");

                    System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
                    System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

                    // TO TEST WITH POE2
                    // For weapons, the pseudo_adds_ [a-z] + _ damage option is given on attack
                    string pseudo = rm.GetString("General014_Pseudo", cultureEn);
                    if (type_name == pseudo && isWeapon && RegexUtil.AddsDamagePattern().IsMatch(id))
                    {
                        id += "_to_attacks";
                    }
                    filter ??= filterResult.Entries.FirstOrDefault(x => x.ID == id && x.Type == type);

                    stats[0].Filters[idx] = new() { Value = new() };
                    if (filter is not null && filter.ID is not null && filter.ID.Trim().Length > 0)
                    {
                        stats[0].Filters[idx].Disabled = xiletradeItem.ItemFilters[i].Disabled;

                        if (xiletradeItem.ItemFilters[i].Option is not 0
                            && xiletradeItem.ItemFilters[i].Option.IsNotEmpty())
                        {
                            stats[0].Filters[idx].Value.Option = xiletradeItem.ItemFilters[i].Option.ToString();
                        }
                        else
                        {
                            if (xiletradeItem.ItemFilters[i].Min.IsNotEmpty())
                                stats[0].Filters[idx].Value.Min = xiletradeItem.ItemFilters[i].Min;
                            if (xiletradeItem.ItemFilters[i].Max.IsNotEmpty())
                                stats[0].Filters[idx].Value.Max = xiletradeItem.ItemFilters[i].Max;
                        }
                        stats[0].Filters[idx++].Id = filter.ID;
                    }
                    else
                    {
                        errorsFilters = true;
                        xiletradeItem.ItemFilters[i].IsNull = true;

                        // Add anything on null to avoid errors
                        //Query.Stats[0].Filters[idx].Disabled = true;
                        //Query.Stats[0].Filters[idx++].Id = "error_id";
                    }
                }
            }
            if (GetEnglishRarity(xiletradeItem.Rarity) is not "Unique")
            {
                stats = UpdateWithCountAttribute(stats);
            }
        }

        if (errorsFilters)
        {
            int errorCount = 0;
            List<int> errors = new();
            for (int i = 0; i < xiletradeItem.ItemFilters.Count; i++)
            {
                if (xiletradeItem.ItemFilters[i].IsNull)
                {
                    errorCount++;
                    errors.Add(i + 1);
                }
            }
            throw new Exception(string.Format("{0} Mod error(s) detected: \r\n\r\nMod lines : {1}\r\n\r\n", errorCount, errors.ToString()));
        }

        return stats;
    }

    private static string BeforeDayToString(int day)
    {
        if (day < 3) return "1day";
        if (day < 7) return "3days";
        if (day < 14) return "1week";
        return "2weeks";
    }

    private static string GetEnglishRarity(string rarityLang)
    {
        System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
        System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

        return rarityLang == Resources.Resources.General005_Any ? rm.GetString("General005_Any", cultureEn) :
            rarityLang == Resources.Resources.General110_FoilUnique ? rm.GetString("General110_FoilUnique", cultureEn) :
            rarityLang == Resources.Resources.General006_Unique ? rm.GetString("General006_Unique", cultureEn) :
            rarityLang == Resources.Resources.General007_Rare ? rm.GetString("General007_Rare", cultureEn) :
            rarityLang == Resources.Resources.General008_Magic ? rm.GetString("General008_Magic", cultureEn) :
            rarityLang == Resources.Resources.General009_Normal ? rm.GetString("General009_Normal", cultureEn) :
            rarityLang == Resources.Resources.General010_AnyNU ? rm.GetString("General010_AnyNU", cultureEn) : string.Empty;
    }

    private static string GetAffixType(string inputType)
    {
        return inputType is "pseudo" ? Resources.Resources.General014_Pseudo :
            inputType is "explicit" ? Resources.Resources.General015_Explicit :
            inputType is "implicit" ? Resources.Resources.General013_Implicit :
            inputType is "enchant" ? Resources.Resources.General011_Enchant :
            inputType is "rune" ? Resources.Resources.General145_Rune : // change to General132_Rune when translated by GGG.
            inputType is "sanctum" ? Resources.Resources.General111_Sanctum :
            inputType is "desecrated" ? Resources.Resources.General158_Desecrated :
            inputType is "fractured" ? Resources.Resources.General016_Fractured :
            inputType is "skill" ? Resources.Resources.General144_Skill : string.Empty;
    }

    // DOESNT WORK WITH API : BAD REQUEST
    // Can not use "weight" type search without being logged.
    private static Stats[] UpdateWithWeightResistance(Stats[] stats)
    {
        var resStat = stats[0].Filters
                .Where(x => x.Id is Strings.StatPoe2.FireResistance
                or Strings.StatPoe2.ColdResistance
                or Strings.StatPoe2.LightningResistance);
        if (!resStat.Any())
        {
            return stats;
        }

        double total = 0;
        foreach (var res in resStat)
        {
            if (res.Value.Min is not null)
            {
                total += (double)res.Value.Min;
            }
        }
        if (total is 0)
        {
            return stats;
        }

        var previous = stats[0];
        stats = new Stats[2];
        stats[0] = previous;

        var stat = new Stats()
        {
            Type = "weight2",
            Value = new() { Min = total },
            Filters =
            [
                new() { Id = Strings.StatPoe2.FireResistance, Value = null },
                new() { Id = Strings.StatPoe2.ColdResistance, Value = null },
                new() { Id = Strings.StatPoe2.LightningResistance, Value = null }
            ]
        };
        //stat.Filters[0].Value.Weight = 1;
        //stat.Filters[1].Value.Weight = 1;
        //stat.Filters[2].Value.Weight = 1;

        stats[1] = stat;
        return stats;
    }

    private static Stats[] UpdateWithCountAttribute(Stats[] stats)
    {
        var attributes = stats[0].Filters
                .Where(x => x is not null &&
                x.Id is Strings.StatPoe2.Strength
                or Strings.StatPoe2.Dexterity
                or Strings.StatPoe2.Intelligence);
        if (!attributes.Any() || attributes.Count() > 1)
        {
            return stats;
        }

        double total = 0;
        foreach (var at in attributes)
        {
            if (at.Value.Min is not null)
            {
                total += (double)at.Value.Min;
                at.Disabled = true;
            }
        }
        if (total is 0)
        {
            return stats;
        }

        var previous = stats[0];
        stats = new Stats[2];
        stats[0] = previous;

        var stat = new Stats()
        {
            Type = "count",
            Value = new() { Min = 1 },
            Filters =
            [
                new() { Id = Strings.StatPoe2.Strength },
                new() { Id = Strings.StatPoe2.Dexterity },
                new() { Id = Strings.StatPoe2.Intelligence }
            ]
        };
        stat.Filters[0].Value.Min = total;
        stat.Filters[1].Value.Min = total;
        stat.Filters[2].Value.Min = total;

        stats[1] = stat;
        return stats;
    }
}

