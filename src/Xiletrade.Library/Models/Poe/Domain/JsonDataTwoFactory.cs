using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Contract.Two;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class JsonDataTwoFactory
{
    private readonly DataManagerService _dm;

    internal JsonDataTwoFactory(DataManagerService dm)
    {
        _dm = dm;
    }

    /// <summary>
    /// Create a POE2 JSON query for custom search OR search presets.
    /// </summary>
    /// <param name="xItem"></param>
    /// <param name="unid"></param>
    /// <param name="market"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    internal JsonDataTwo Create(XiletradeItem xItem, UniqueUnidentified unid, string market, string search)
    {
        var json = new JsonDataTwo(market);

        if (!string.IsNullOrEmpty(search))
        {
            json.Query.Term = search;
        }
        else if (unid is not null)
        {
            json.Query.Name = unid.Name;
            json.Query.Type = unid.Type;
        }

        // Filters
        json.Query.Filters.Equipment = GetEquipmentFilters(xItem);
        json.Query.Filters.Requirement = GetRequirementFilters(xItem);
        json.Query.Filters.Misc = GetMiscFilters(xItem);
        json.Query.Filters.Type = GetTypeFilters(xItem);
        json.Query.Filters.Trade = GetTradeFilters(xItem, _dm.Config.Options.SearchBeforeDay, useSaleType: true);

        return json;
    }

    /// <summary>
    /// Create a POE2 JSON query for regular item search.
    /// </summary>
    /// <param name="xItem"></param>
    /// <param name="item"></param>
    /// <param name="useSaleType"></param>
    /// <param name="market"></param>
    /// <returns></returns>
    internal JsonDataTwo Create(XiletradeItem xItem, ItemData item, bool useSaleType, string market)
    {
        var json = new JsonDataTwo(market);

        // Name / Type
        var name = xItem.UniqueName.Length > 0 ? xItem.UniqueName : item.NameGateway;
        var type = item.TypeGateway;

        bool simpleMode = xItem.ByType || name.Length is 0
            || (!item.Flag.Unique && !item.Flag.FoilVariant);

        if (!simpleMode)
        {
            json.Query.Name = name;
            json.Query.Type = type;
        }
        else if (!xItem.ByType)
        {
            json.Query.Type = type;
        }

        // Filters
        json.Query.Filters.Trade = GetTradeFilters(xItem, _dm.Config.Options.SearchBeforeDay, useSaleType);
        json.Query.Filters.Equipment = GetEquipmentFilters(xItem);
        json.Query.Filters.Requirement = GetRequirementFilters(xItem);
        json.Query.Filters.Map = GetMapFilters(xItem, item);
        json.Query.Filters.Misc = GetMiscFilters(xItem, item);
        json.Query.Filters.Type = GetTypeFilters(xItem, item);

        // Stats
        json.Query.Stats = GetStatsFilters(_dm.Filter, xItem, item.Flag.Weapon);

        return json;
    }

    private static TypeTwo GetTypeFilters(XiletradeItem xItem)
    {
        var rarityEn = GetEnglishRarity(xItem.Rarity);
        var isRarity = rarityEn.Length > 0 && rarityEn is not Strings.any;

        if (isRarity || xItem.Lvl.Enable || xItem.Quality.Enable)
        {
            return null;
        }

        TypeTwo type = new();

        if (isRarity)
        {
            type.Filters.Rarity = new(rarityEn);
        }

        if (xItem.Lvl.Enable)
        {
            type.Filters.ItemLevel = new() { Min = xItem.Lvl.Min, Max = xItem.Lvl.Max };
        }

        if (xItem.Quality.Enable)
        {
            type.Filters.Quality = new() { Min = xItem.Quality.Min, Max = xItem.Quality.Max };
        }

        return type;
    }

    private static TypeTwo GetTypeFilters(XiletradeItem xItem, ItemData item)
    {
        var rarityEn = GetEnglishRarity(xItem.Rarity);
        var isRarity = rarityEn.Length > 0 && rarityEn is not Strings.any;

        var category = item.Flag.GetItemCategoryApi();
        var isCategory = category.Length > 0;

        if ((isRarity || isCategory || xItem.Quality.Enable 
            || (xItem.Lvl.Enable && !item.Flag.Gems)))
        {
            return null;
        }

        TypeTwo type = new();

        if (isRarity)
        {
            type.Filters.Rarity = new(rarityEn);
        }
        if (isCategory)
        {
            type.Filters.Category = new(category);
        }
        if (xItem.Quality.Enable)
        {
            type.Filters.Quality = new() { Min = xItem.Quality.Min, Max = xItem.Quality.Max };
        }
        if (xItem.Lvl.Enable && !item.Flag.Gems)
        {
            type.Filters.ItemLevel = new() { Min = xItem.Lvl.Min, Max = xItem.Lvl.Max };
        }

        return type;
    }

    private static MiscTwo GetMiscFilters(XiletradeItem xItem)
    {
        MiscTwo misc = new();

        if (xItem.Corrupted is DefaultOption.True)
            misc.Filters.Corrupted = GetOptionTrue();
        if (xItem.Corrupted is DefaultOption.False)
            misc.Filters.Corrupted = GetOptionFalse();

        if (xItem.TwiceCorrupted is DefaultOption.True)
            misc.Filters.TwiceCorrupted = GetOptionTrue();
        if (xItem.TwiceCorrupted is DefaultOption.False)
            misc.Filters.TwiceCorrupted = GetOptionFalse();

        if (xItem.Identified is DefaultOption.True)
            misc.Filters.Identified = GetOptionTrue();
        if (xItem.Identified is DefaultOption.False)
            misc.Filters.Identified = GetOptionFalse();
        
        if (xItem.Fractured is DefaultOption.True)
            misc.Filters.Fractured = GetOptionTrue();
        if (xItem.Fractured is DefaultOption.False)
            misc.Filters.Fractured = GetOptionFalse();
        
        if (xItem.Mirrored is DefaultOption.True)
            misc.Filters.Mirrored = GetOptionTrue();
        if (xItem.Mirrored is DefaultOption.False)
            misc.Filters.Mirrored = GetOptionFalse();

        if (xItem.Crafted is DefaultOption.True)
            misc.Filters.Crafted = GetOptionTrue();
        if (xItem.Crafted is DefaultOption.False)
            misc.Filters.Crafted = GetOptionFalse();

        if (xItem.Mutated is DefaultOption.True)
            misc.Filters.Mutated = GetOptionTrue();
        if (xItem.Mutated is DefaultOption.False)
            misc.Filters.Mutated = GetOptionFalse();

        if (xItem.Veiled is DefaultOption.True)
            misc.Filters.Veiled = GetOptionTrue();
        if (xItem.Veiled is DefaultOption.False)
            misc.Filters.Veiled = GetOptionFalse();

        if (xItem.Desecrated is DefaultOption.True)
            misc.Filters.Desecrated = GetOptionTrue();
        if (xItem.Desecrated is DefaultOption.False)
            misc.Filters.Desecrated = GetOptionFalse();

        if (xItem.Sanctified is DefaultOption.True)
            misc.Filters.Sanctified = GetOptionTrue();
        if (xItem.Sanctified is DefaultOption.False)
            misc.Filters.Sanctified = GetOptionFalse();

        if (xItem.GemSockets.Enable)
        {
            misc.Filters.GemSockets = new() { Min = xItem.GemSockets.Min, Max = xItem.GemSockets.Max };
        }

        var enable = misc.Filters.Identified is not null || misc.Filters.Corrupted is not null
            || misc.Filters.TwiceCorrupted is not null || misc.Filters.Veiled is not null
            || misc.Filters.Fractured is not null || misc.Filters.Mirrored is not null
            || misc.Filters.Crafted is not null || misc.Filters.Mutated is not null
            || misc.Filters.Desecrated is not null || misc.Filters.Sanctified is not null
            || xItem.GemSockets.Enable;

        return enable ? misc : null;
    }

    private static MiscTwo GetMiscFilters(XiletradeItem xItem, ItemData item)
    {
        var checkCond = xItem.Lvl.Enable && (item.Flag.Gems || item.Flag.Area)
            || xItem.GemSockets.Enable && item.Flag.Gems;
        var checkForm = xItem.Corrupted is not DefaultOption.Any
            || xItem.TwiceCorrupted is not DefaultOption.Any
            || xItem.Identified is not DefaultOption.Any
            || xItem.Fractured is not DefaultOption.Any
            || xItem.Mirrored is not DefaultOption.Any
            || xItem.Crafted is not DefaultOption.Any
            || xItem.Mutated is not DefaultOption.Any
            || xItem.Veiled is not DefaultOption.Any
            || xItem.Desecrated is not DefaultOption.Any
            || xItem.Sanctified is not DefaultOption.Any;

        if (!(checkCond || checkForm))
        {
            return null;
        }

        MiscTwo misc = new();
        if (item.Flag.Gems)
        {
            if (xItem.Lvl.Enable)
            {
                misc.Filters.GemLevel = new() { Min = xItem.Lvl.Min, Max = xItem.Lvl.Max };
            }
            if (xItem.GemSockets.Enable)
            {
                misc.Filters.GemSockets = new() { Min = xItem.GemSockets.Min, Max = xItem.GemSockets.Max };
            }
        }
        if (item.Flag.Area && xItem.Lvl.Enable)
        {
            misc.Filters.AreaLevel = new() { Min = xItem.Lvl.Min, Max = xItem.Lvl.Max };
        }

        if (checkForm)
        {
            if (xItem.Corrupted is DefaultOption.True)
                misc.Filters.Corrupted = GetOptionTrue();
            if (xItem.Corrupted is DefaultOption.False)
                misc.Filters.Corrupted = GetOptionFalse();

            if (xItem.TwiceCorrupted is DefaultOption.True)
                misc.Filters.TwiceCorrupted = GetOptionTrue();
            if (xItem.TwiceCorrupted is DefaultOption.False)
                misc.Filters.TwiceCorrupted = GetOptionFalse();

            if (xItem.Identified is DefaultOption.True)
                misc.Filters.Identified = GetOptionTrue();
            if (xItem.Identified is DefaultOption.False)
                misc.Filters.Identified = GetOptionFalse();

            if (xItem.Fractured is DefaultOption.True)
                misc.Filters.Fractured = GetOptionTrue();
            if (xItem.Fractured is DefaultOption.False)
                misc.Filters.Fractured = GetOptionFalse();

            if (xItem.Mirrored is DefaultOption.True)
                misc.Filters.Mirrored = GetOptionTrue();
            if (xItem.Mirrored is DefaultOption.False)
                misc.Filters.Mirrored = GetOptionFalse();

            if (xItem.Crafted is DefaultOption.True)
                misc.Filters.Crafted = GetOptionTrue();
            if (xItem.Crafted is DefaultOption.False)
                misc.Filters.Crafted = GetOptionFalse();

            if (xItem.Mutated is DefaultOption.True)
                misc.Filters.Mutated = GetOptionTrue();
            if (xItem.Mutated is DefaultOption.False)
                misc.Filters.Mutated = GetOptionFalse();

            if (xItem.Veiled is DefaultOption.True)
                misc.Filters.Veiled = GetOptionTrue();
            if (xItem.Veiled is DefaultOption.False)
                misc.Filters.Veiled = GetOptionFalse();

            if (xItem.Desecrated is DefaultOption.True)
                misc.Filters.Desecrated = GetOptionTrue();
            if (xItem.Desecrated is DefaultOption.False)
                misc.Filters.Desecrated = GetOptionFalse();

            if (xItem.Sanctified is DefaultOption.True)
                misc.Filters.Sanctified = GetOptionTrue();
            if (xItem.Sanctified is DefaultOption.False)
                misc.Filters.Sanctified = GetOptionFalse();
            
            //TODO
            /*
            Query.Filters.Misc.Filters.UnidentifiedTier
            Query.Filters.Misc.Filters.GemSockets
            Query.Filters.Misc.Filters.BaryaSacredWater
            Query.Filters.Misc.Filters.StackSize
            */
        }

        return misc;
    }

    private static TradeTwo GetTradeFilters(XiletradeItem xItem, int searchConfig, bool useSaleType = false)
    {
        TradeTwo trade = new()
        {
            Disabled = searchConfig is 0
        };

        if (searchConfig is not 0)
        {
            trade.Filters.Indexed = new(BeforeDayToString(searchConfig));
        }
        if (useSaleType)
        {
            trade.Filters.SaleType = new("priced");
        }
        if (xItem.PriceMin > 0 && xItem.PriceMin.IsNotEmpty())
        {
            trade.Filters.Price.Min = xItem.PriceMin;
        }
        if (xItem.ExaltOnly && !xItem.ChaosOnly)
        {
            trade.Disabled = false;
            trade.Filters.Price.Option = "exalted";
        }
        if (xItem.ChaosOnly && !xItem.ExaltOnly)
        {
            trade.Disabled = false;
            trade.Filters.Price.Option = "chaos";
        }
        //TODO: Query.Filters.Trade.Filters.Collapse
        return trade.Disabled ? null : trade;
    }

    private static MapTwo GetMapFilters(XiletradeItem xItem, ItemData item)
    {
        if (!item.Flag.Waystones)
        {
            return null;
        }

        MapTwo map = new();

        if (xItem.ItemRarity.Enable)
        {
            map.Filters.Rarity = new()
            {
                Min = xItem.ItemRarity.Min,
                Max = xItem.ItemRarity.Max
            };
        }
        if (xItem.PackSize.Enable)
        {
            map.Filters.PackSize = new()
            {
                Min = xItem.PackSize.Min,
                Max = xItem.PackSize.Max
            };
        }
        if (xItem.MonsterRarity.Enable)
        {
            map.Filters.RareMonsters = new()
            {
                Min = xItem.MonsterRarity.Min,
                Max = xItem.MonsterRarity.Max
            };
        }
        if (xItem.Effectiveness.Enable)
        {
            map.Filters.MagicMonsters = new()
            {
                Min = xItem.Effectiveness.Min,
                Max = xItem.Effectiveness.Max
            }; // will probably be updated by GGG later
        }
        if (xItem.WaystoneDrop.Enable)
        {
            map.Filters.Bonus = new()
            {
                Min = xItem.WaystoneDrop.Min,
                Max = xItem.WaystoneDrop.Max
            }; // will probably be updated by GGG later
        }
        if (xItem.Revives.Enable)
        {
            map.Filters.Revives = new()
            {
                Min = xItem.Revives.Min,
                Max = xItem.Revives.Max
            };
        }

        return map;
    }

    private static Requirement GetRequirementFilters(XiletradeItem xItem)
    {
        if (xItem.ReqLevel.Enable)
        {
            Requirement requirement = new();
            requirement.Filters.Level = new() { Min = xItem.ReqLevel.Min, Max = xItem.ReqLevel.Max };
            return requirement;
        }
        return null;
    }

    private static Equipment GetEquipmentFilters(XiletradeItem xItem)
    {
        if (!(xItem.Armour.Enable || xItem.Energy.Enable || xItem.Evasion.Enable
                || xItem.DpsTotal.Enable || xItem.DpsPhys.Enable || xItem.DpsElem.Enable
                || xItem.RuneSockets.Enable || xItem.Ward.Enable))
        {
            return null;
        }
        
        Equipment equipment = new();

        if (xItem.Armour.Enable)
        {
            equipment.Filters.Armour = new() { Min = xItem.Armour.Min, Max = xItem.Armour.Max };
        }
        if (xItem.Energy.Enable)
        {
            equipment.Filters.EnergyShield = new() { Min = xItem.Energy.Min, Max = xItem.Energy.Max };
        }
        if (xItem.Evasion.Enable)
        {
            equipment.Filters.Evasion = new() { Min = xItem.Evasion.Min, Max = xItem.Evasion.Max };
        }
        if (xItem.Ward.Enable)
        {
            equipment.Filters.RunicWard = new() { Min = xItem.Ward.Min, Max = xItem.Ward.Max };
        }
        if (xItem.DpsTotal.Enable)
        {
            equipment.Filters.DamagePerSecond = new() { Min = xItem.DpsTotal.Min, Max = xItem.DpsTotal.Max };
        }
        if (xItem.DpsPhys.Enable)
        {
            equipment.Filters.PhysicalDps = new() { Min = xItem.DpsPhys.Min, Max = xItem.DpsPhys.Max };
        }
        if (xItem.DpsElem.Enable)
        {
            equipment.Filters.ElementalDps = new() { Min = xItem.DpsElem.Min, Max = xItem.DpsElem.Max };
        }
        if (xItem.RuneSockets.Enable)
        {
            equipment.Filters.RuneSockets = new() { Min = xItem.RuneSockets.Min, Max = xItem.RuneSockets.Max };
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

        return equipment;
    }

    private static Stats[] GetStatsFilters(FilterData filterData, XiletradeItem xItem, bool isWeapon)
    {
        if (xItem.ItemFilters is null || xItem.ItemFilters.Count is 0)
        {
            return null;
        }
        
        bool errorsFilters = false;
        /*
        var stats = new Stats[1];
        stats[0] = new()
        {
            Type = "and",
            Filters = new StatsFilters[xItem.ItemFilters.Count]
        };
        */
        Stats[] stats =
        [
            new()
            {
                Type = "and",
                Filters = new StatsFilters[xItem.ItemFilters.Count]
            }
        ];

        int idx = 0;
        for (int i = 0; i < xItem.ItemFilters.Count; i++)
        {
            string input = xItem.ItemFilters[i].Text;
            string id = xItem.ItemFilters[i].Id;
            string type = xItem.ItemFilters[i].Type;
            if (input.Trim().Length > 0)
            {
                string type_name = GetAffixType(type);

                if (type_name.Length is 0)
                {
                    continue; // will create a bad request as intended (to detect new type) and not crash the app 
                }

                FilterResultEntrie filter = null;

                var filterResult = filterData.GetFilterResultWithLabel(type_name);
                type_name = type_name.ToLowerInvariant();
                input = Regex.Escape(input).Replace("\\+\\#", "[+]?\\#");

                // TO TEST WITH POE2
                // For weapons, the pseudo_adds_ [a-z] + _ damage option is given on attack
                var pseudo = Resources.Resources.ResourceManager
                    .GetEnglish(nameof(Resources.Resources.General014_Pseudo));
                if (type_name == pseudo && isWeapon && RegexUtil.AddsDamagePattern().IsMatch(id))
                {
                    id += "_to_attacks";
                }
                filter ??= filterResult.FindEntryByIdAndType(id, type);

                stats[0].Filters[idx] = new() { Value = new() };
                if (filter is not null && filter.ID is not null && filter.ID.Trim().Length > 0)
                {
                    stats[0].Filters[idx].Disabled = xItem.ItemFilters[i].Disabled;

                    if (xItem.ItemFilters[i].Option is not 0
                        && xItem.ItemFilters[i].Option.IsNotEmpty())
                    {
                        stats[0].Filters[idx].Value.Option = xItem.ItemFilters[i].Option.ToString();
                    }
                    else
                    {
                        if (xItem.ItemFilters[i].Min.IsNotEmpty())
                            stats[0].Filters[idx].Value.Min = xItem.ItemFilters[i].Min;
                        if (xItem.ItemFilters[i].Max.IsNotEmpty())
                            stats[0].Filters[idx].Value.Max = xItem.ItemFilters[i].Max;
                    }
                    stats[0].Filters[idx++].Id = filter.ID;
                }
                else
                {
                    errorsFilters = true;
                    xItem.ItemFilters[i].IsNull = true;

                    // Add anything on null to avoid errors
                    //Query.Stats[0].Filters[idx].Disabled = true;
                    //Query.Stats[0].Filters[idx++].Id = "error_id";
                }
            }
        }

        if (errorsFilters)
        {
            int errorCount = 0;
            List<int> errors = new();
            for (int i = 0; i < xItem.ItemFilters.Count; i++)
            {
                if (xItem.ItemFilters[i].IsNull)
                {
                    errorCount++;
                    errors.Add(i + 1);
                }
            }
            throw new Exception(string.Format("{0} Mod error(s) detected: \r\n\r\nMod lines : {1}\r\n\r\n", errorCount, errors.ToString()));
        }

        return stats;
    }

    // Utility

    private static OptionTxt GetOptionTrue() => new("true");

    private static OptionTxt GetOptionFalse() => new("false");

    private static string BeforeDayToString(int day)
    {
        if (day < 3) return "1day";
        if (day < 7) return "3days";
        if (day < 14) return "1week";
        return "2weeks";
    }

    private static string GetEnglishRarity(string rarityLang)
    {
        var rm = Resources.Resources.ResourceManager;
        var rarity = rarityLang == Resources.Resources.General005_Any ? rm.GetEnglish(nameof(Resources.Resources.General005_Any))
            : rarityLang == Resources.Resources.General110_FoilUnique ? rm.GetEnglish(nameof(Resources.Resources.General110_FoilUnique))
            : rarityLang == Resources.Resources.General006_Unique ? rm.GetEnglish(nameof(Resources.Resources.General006_Unique))
            : rarityLang == Resources.Resources.General007_Rare ? rm.GetEnglish(nameof(Resources.Resources.General007_Rare))
            : rarityLang == Resources.Resources.General008_Magic ? rm.GetEnglish(nameof(Resources.Resources.General008_Magic))
            : rarityLang == Resources.Resources.General009_Normal ? rm.GetEnglish(nameof(Resources.Resources.General009_Normal))
            : rarityLang == Resources.Resources.General010_AnyNU ? rm.GetEnglish(nameof(Resources.Resources.General010_AnyNU)) 
            : string.Empty;

        return rarity is "Any N-U" ? "nonunique"
            : rarity is "Foil Unique" ? "uniquefoil"
            : rarity.ToLowerInvariant();
    }

    private static string GetAffixType(string inputType)
    {
        return inputType is "pseudo" ? Resources.Resources.General014_Pseudo :
            inputType is "explicit" ? Resources.Resources.General015_Explicit :
            inputType is "implicit" ? Resources.Resources.General013_Implicit :
            inputType is "enchant" ? Resources.Resources.General011_Enchant :
            inputType is "augment" ? Resources.Resources.General145_Augment :
            inputType is "sanctum" ? Resources.Resources.General111_Sanctum :
            inputType is "desecrated" ? Resources.Resources.General158_Desecrated :
            inputType is "fractured" ? Resources.Resources.General016_Fractured :
            inputType is "crafted" ? Resources.Resources.General012_Crafted :
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

    /// <summary>
    /// Deprecated
    /// </summary>
    /// <param name="stats"></param>
    /// <returns></returns>
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

