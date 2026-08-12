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
    /// <param name="xiletradeItem"></param>
    /// <param name="unid"></param>
    /// <param name="market"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    internal JsonDataTwo Create(XiletradeItem xiletradeItem, UniqueUnidentified unid, string market, string search)
    {
        var json = new JsonDataTwo 
        {
            Query = new() { Status = new(market) },
            Sort = new() { Price = "asc" }
        };

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
        json.Query.Filters.Equipment = GetEquipmentFilters(xiletradeItem);
        json.Query.Filters.Requirement = GetRequirementFilters(xiletradeItem);
        json.Query.Filters.Misc = GetMiscFilters(xiletradeItem);
        json.Query.Filters.Type = GetTypeFilters(xiletradeItem);
        json.Query.Filters.Trade = GetTradeFilters(xiletradeItem, _dm.Config.Options.SearchBeforeDay, useSaleType: true);

        return json;
    }

    /// <summary>
    /// Create a POE2 JSON query for regular item search.
    /// </summary>
    /// <param name="xiletradeItem"></param>
    /// <param name="item"></param>
    /// <param name="useSaleType"></param>
    /// <param name="market"></param>
    /// <returns></returns>
    internal JsonDataTwo Create(XiletradeItem xiletradeItem, ItemData item, bool useSaleType, string market)
    {
        var json = new JsonDataTwo
        {
            Query = new() { Status = new(market) },
            Sort = new() { Price = "asc" }
        };

        // Name / Type
        var name = xiletradeItem.UniqueName.Length > 0 ? xiletradeItem.UniqueName : item.NameGateway;
        var type = item.TypeGateway;

        bool simpleMode = xiletradeItem.ByType || name.Length is 0
            || (!item.Flag.Unique && !item.Flag.FoilVariant);

        if (!simpleMode)
        {
            json.Query.Name = name;
            json.Query.Type = type;
        }
        else if (!xiletradeItem.ByType)
        {
            json.Query.Type = type;
        }

        // Filters
        json.Query.Filters.Trade = GetTradeFilters(xiletradeItem, _dm.Config.Options.SearchBeforeDay, useSaleType);
        json.Query.Filters.Equipment = GetEquipmentFilters(xiletradeItem);
        json.Query.Filters.Requirement = GetRequirementFilters(xiletradeItem);
        json.Query.Filters.Map = GetMapFilters(xiletradeItem, item);
        json.Query.Filters.Misc = GetMiscFilters(xiletradeItem, item);
        json.Query.Filters.Type = GetTypeFilters(xiletradeItem, item);

        // Stats
        json.Query.Stats = GetStatsFilters(_dm.Filter, xiletradeItem, item.Flag.Weapon);

        return json;
    }

    private static TypeTwo GetTypeFilters(XiletradeItem xiletradeItem)
    {
        TypeTwo type = new();

        string rarityEn = GetEnglishRarity(xiletradeItem.Rarity);
        if (rarityEn.Length > 0 && rarityEn is not Strings.any)
        {
            type.Filters.Rarity = new(rarityEn);
            type.Disabled = false;
        }

        if (xiletradeItem.Lvl.Enable)
        {
            type.Filters.ItemLevel.Min = xiletradeItem.Lvl.Min;
            type.Filters.ItemLevel.Max = xiletradeItem.Lvl.Max;
        }

        if (xiletradeItem.Quality.Enable)
        {
            type.Filters.Quality.Min = xiletradeItem.Quality.Min;
            type.Filters.Quality.Max = xiletradeItem.Quality.Max;
        }

        return type;
    }

    private static TypeTwo GetTypeFilters(XiletradeItem xiletradeItem, ItemData item)
    {
        TypeTwo type = new() { Disabled = false };

        string rarityEn = GetEnglishRarity(xiletradeItem.Rarity);
        if (rarityEn.Length > 0 && rarityEn is not Strings.any)
        {
            type.Filters.Rarity = new(rarityEn);
        }
        var category = item.Flag.GetItemCategoryApi();
        if (category.Length > 0)
        {
            type.Filters.Category = new(category);
        }
        if (xiletradeItem.Quality.Enable)
        {
            type.Filters.Quality.Min = xiletradeItem.Quality.Min;
            type.Filters.Quality.Max = xiletradeItem.Quality.Max;
        }
        if (xiletradeItem.Lvl.Enable && !item.Flag.Gems)
        {
            type.Filters.ItemLevel.Min = xiletradeItem.Lvl.Min;
            type.Filters.ItemLevel.Max = xiletradeItem.Lvl.Max;
        }

        return type;
    }

    private static MiscTwo GetMiscFilters(XiletradeItem xiletradeItem)
    {
        MiscTwo misc = new();

        if (xiletradeItem.Corrupted is DefaultOption.True)
            misc.Filters.Corrupted = GetOptionTrue();
        if (xiletradeItem.Corrupted is DefaultOption.False)
            misc.Filters.Corrupted = GetOptionFalse();

        if (xiletradeItem.TwiceCorrupted is DefaultOption.True)
            misc.Filters.TwiceCorrupted = GetOptionTrue();
        if (xiletradeItem.TwiceCorrupted is DefaultOption.False)
            misc.Filters.TwiceCorrupted = GetOptionFalse();

        if (xiletradeItem.Identified is DefaultOption.True)
            misc.Filters.Identified = GetOptionTrue();
        if (xiletradeItem.Identified is DefaultOption.False)
            misc.Filters.Identified = GetOptionFalse();
        
        if (xiletradeItem.Fractured is DefaultOption.True)
            misc.Filters.Fractured = GetOptionTrue();
        if (xiletradeItem.Fractured is DefaultOption.False)
            misc.Filters.Fractured = GetOptionFalse();
        
        if (xiletradeItem.Mirrored is DefaultOption.True)
            misc.Filters.Mirrored = GetOptionTrue();
        if (xiletradeItem.Mirrored is DefaultOption.False)
            misc.Filters.Mirrored = GetOptionFalse();

        if (xiletradeItem.Crafted is DefaultOption.True)
            misc.Filters.Crafted = GetOptionTrue();
        if (xiletradeItem.Crafted is DefaultOption.False)
            misc.Filters.Crafted = GetOptionFalse();

        if (xiletradeItem.Mutated is DefaultOption.True)
            misc.Filters.Mutated = GetOptionTrue();
        if (xiletradeItem.Mutated is DefaultOption.False)
            misc.Filters.Mutated = GetOptionFalse();

        if (xiletradeItem.Veiled is DefaultOption.True)
            misc.Filters.Veiled = GetOptionTrue();
        if (xiletradeItem.Veiled is DefaultOption.False)
            misc.Filters.Veiled = GetOptionFalse();

        if (xiletradeItem.Desecrated is DefaultOption.True)
            misc.Filters.Desecrated = GetOptionTrue();
        if (xiletradeItem.Desecrated is DefaultOption.False)
            misc.Filters.Desecrated = GetOptionFalse();

        if (xiletradeItem.Sanctified is DefaultOption.True)
            misc.Filters.Sanctified = GetOptionTrue();
        if (xiletradeItem.Sanctified is DefaultOption.False)
            misc.Filters.Sanctified = GetOptionFalse();

        if (xiletradeItem.GemSockets.Enable)
        {
            misc.Filters.GemSockets.Min = xiletradeItem.GemSockets.Min;
            misc.Filters.GemSockets.Max = xiletradeItem.GemSockets.Max;
        }

        if (misc.Filters.Identified is not null || misc.Filters.Corrupted is not null
            || misc.Filters.TwiceCorrupted is not null || misc.Filters.Veiled is not null
            || misc.Filters.Fractured is not null || misc.Filters.Mirrored is not null
            || misc.Filters.Crafted is not null || misc.Filters.Mutated is not null
            || misc.Filters.Desecrated is not null || misc.Filters.Sanctified is not null
            || xiletradeItem.GemSockets.Enable)
            misc.Disabled = false;

        return misc;
    }

    private static MiscTwo GetMiscFilters(XiletradeItem xiletradeItem, ItemData item)
    {
        MiscTwo misc = new();

        var checkCond = xiletradeItem.Lvl.Enable && (item.Flag.Gems || item.Flag.Area)
            || xiletradeItem.GemSockets.Enable && item.Flag.Gems;
        var checkForm = xiletradeItem.Corrupted is not DefaultOption.Any
            || xiletradeItem.TwiceCorrupted is not DefaultOption.Any
            || xiletradeItem.Identified is not DefaultOption.Any
            || xiletradeItem.Fractured is not DefaultOption.Any
            || xiletradeItem.Mirrored is not DefaultOption.Any
            || xiletradeItem.Crafted is not DefaultOption.Any
            || xiletradeItem.Mutated is not DefaultOption.Any
            || xiletradeItem.Veiled is not DefaultOption.Any
            || xiletradeItem.Desecrated is not DefaultOption.Any
            || xiletradeItem.Sanctified is not DefaultOption.Any;

        misc.Disabled = !(checkCond || checkForm);

        if (item.Flag.Gems)
        {
            if (xiletradeItem.Lvl.Enable)
            {
                misc.Filters.GemLevel.Min = xiletradeItem.Lvl.Min;
                misc.Filters.GemLevel.Max = xiletradeItem.Lvl.Max;
            }

            if (xiletradeItem.GemSockets.Enable)
            {
                misc.Filters.GemSockets.Min = xiletradeItem.GemSockets.Min;
                misc.Filters.GemSockets.Max = xiletradeItem.GemSockets.Max;
            }
        }
        if (item.Flag.Area && xiletradeItem.Lvl.Enable)
        {
            misc.Filters.AreaLevel.Min = xiletradeItem.Lvl.Min;
            misc.Filters.AreaLevel.Max = xiletradeItem.Lvl.Max;
        }

        if (checkForm)
        {
            if (xiletradeItem.Corrupted is DefaultOption.True)
                misc.Filters.Corrupted = GetOptionTrue();
            if (xiletradeItem.Corrupted is DefaultOption.False)
                misc.Filters.Corrupted = GetOptionFalse();

            if (xiletradeItem.TwiceCorrupted is DefaultOption.True)
                misc.Filters.TwiceCorrupted = GetOptionTrue();
            if (xiletradeItem.TwiceCorrupted is DefaultOption.False)
                misc.Filters.TwiceCorrupted = GetOptionFalse();

            if (xiletradeItem.Identified is DefaultOption.True)
                misc.Filters.Identified = GetOptionTrue();
            if (xiletradeItem.Identified is DefaultOption.False)
                misc.Filters.Identified = GetOptionFalse();

            if (xiletradeItem.Fractured is DefaultOption.True)
                misc.Filters.Fractured = GetOptionTrue();
            if (xiletradeItem.Fractured is DefaultOption.False)
                misc.Filters.Fractured = GetOptionFalse();

            if (xiletradeItem.Mirrored is DefaultOption.True)
                misc.Filters.Mirrored = GetOptionTrue();
            if (xiletradeItem.Mirrored is DefaultOption.False)
                misc.Filters.Mirrored = GetOptionFalse();

            if (xiletradeItem.Crafted is DefaultOption.True)
                misc.Filters.Crafted = GetOptionTrue();
            if (xiletradeItem.Crafted is DefaultOption.False)
                misc.Filters.Crafted = GetOptionFalse();

            if (xiletradeItem.Mutated is DefaultOption.True)
                misc.Filters.Mutated = GetOptionTrue();
            if (xiletradeItem.Mutated is DefaultOption.False)
                misc.Filters.Mutated = GetOptionFalse();

            if (xiletradeItem.Veiled is DefaultOption.True)
                misc.Filters.Veiled = GetOptionTrue();
            if (xiletradeItem.Veiled is DefaultOption.False)
                misc.Filters.Veiled = GetOptionFalse();

            if (xiletradeItem.Desecrated is DefaultOption.True)
                misc.Filters.Desecrated = GetOptionTrue();
            if (xiletradeItem.Desecrated is DefaultOption.False)
                misc.Filters.Desecrated = GetOptionFalse();

            if (xiletradeItem.Sanctified is DefaultOption.True)
                misc.Filters.Sanctified = GetOptionTrue();
            if (xiletradeItem.Sanctified is DefaultOption.False)
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

    private static TradeTwo GetTradeFilters(XiletradeItem xiletradeItem, int searchConfig, bool useSaleType = false)
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
        MapTwo map = new()
        {
            Disabled = !item.Flag.Waystones
        };

        if (map.Disabled)
        {
            return map;
        }

        if (xiletradeItem.ItemRarity.Enable)
        {
            map.Filters.Rarity = new()
            {
                Min = xiletradeItem.ItemRarity.Min,
                Max = xiletradeItem.ItemRarity.Max
            };
        }
        if (xiletradeItem.PackSize.Enable)
        {
            map.Filters.PackSize = new()
            {
                Min = xiletradeItem.PackSize.Min,
                Max = xiletradeItem.PackSize.Max
            };
        }
        if (xiletradeItem.MonsterRarity.Enable)
        {
            map.Filters.RareMonsters = new()
            {
                Min = xiletradeItem.MonsterRarity.Min,
                Max = xiletradeItem.MonsterRarity.Max
            };
        }
        if (xiletradeItem.Effectiveness.Enable)
        {
            map.Filters.MagicMonsters = new()
            {
                Min = xiletradeItem.Effectiveness.Min,
                Max = xiletradeItem.Effectiveness.Max
            }; // will probably be updated by GGG later
        }
        if (xiletradeItem.WaystoneDrop.Enable)
        {
            map.Filters.Bonus = new()
            {
                Min = xiletradeItem.WaystoneDrop.Min,
                Max = xiletradeItem.WaystoneDrop.Max
            }; // will probably be updated by GGG later
        }
        if (xiletradeItem.Revives.Enable)
        {
            map.Filters.Revives = new()
            {
                Min = xiletradeItem.Revives.Min,
                Max = xiletradeItem.Revives.Max
            };
        }

        return map;
    }

    private static Requirement GetRequirementFilters(XiletradeItem xiletradeItem)
    {
        Requirement requirement = new()
        {
            Disabled = !xiletradeItem.ReqLevel.Enable
        };

        requirement.Filters.Level.Min = xiletradeItem.ReqLevel.Min;
        requirement.Filters.Level.Max = xiletradeItem.ReqLevel.Max;

        return requirement;
    }

    private static Equipment GetEquipmentFilters(XiletradeItem xiletradeItem)
    {
        Equipment equipment = new()
        {
            Disabled = !(xiletradeItem.Armour.Enable || xiletradeItem.Energy.Enable || xiletradeItem.Evasion.Enable
                || xiletradeItem.DpsTotal.Enable || xiletradeItem.DpsPhys.Enable || xiletradeItem.DpsElem.Enable
                || xiletradeItem.RuneSockets.Enable || xiletradeItem.Ward.Enable)
        };

        if (equipment.Disabled)
        {
            return equipment;
        }

        if (xiletradeItem.Armour.Enable)
        {
            equipment.Filters.Armour.Min = xiletradeItem.Armour.Min;
            equipment.Filters.Armour.Max = xiletradeItem.Armour.Max;
        }
        if (xiletradeItem.Energy.Enable)
        {
            equipment.Filters.EnergyShield.Min = xiletradeItem.Energy.Min;
            equipment.Filters.EnergyShield.Max = xiletradeItem.Energy.Max;
        }
        if (xiletradeItem.Evasion.Enable)
        {
            equipment.Filters.Evasion.Min = xiletradeItem.Evasion.Min;
            equipment.Filters.Evasion.Max = xiletradeItem.Evasion.Max;
        }
        if (xiletradeItem.Ward.Enable)
        {
            equipment.Filters.RunicWard.Min = xiletradeItem.Ward.Min;
            equipment.Filters.RunicWard.Max = xiletradeItem.Ward.Max;
        }
        if (xiletradeItem.DpsTotal.Enable)
        {
            equipment.Filters.DamagePerSecond.Min = xiletradeItem.DpsTotal.Min;
            equipment.Filters.DamagePerSecond.Max = xiletradeItem.DpsTotal.Max;
        }
        if (xiletradeItem.DpsPhys.Enable)
        {
            equipment.Filters.PhysicalDps.Min = xiletradeItem.DpsPhys.Min;
            equipment.Filters.PhysicalDps.Max = xiletradeItem.DpsPhys.Max;
        }
        if (xiletradeItem.DpsElem.Enable)
        {
            equipment.Filters.ElementalDps.Min = xiletradeItem.DpsElem.Min;
            equipment.Filters.ElementalDps.Max = xiletradeItem.DpsElem.Max;
        }
        if (xiletradeItem.RuneSockets.Enable)
        {
            equipment.Filters.RuneSockets.Min = xiletradeItem.RuneSockets.Min;
            equipment.Filters.RuneSockets.Max = xiletradeItem.RuneSockets.Max;
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

    private static Stats[] GetStatsFilters(FilterData filterData, XiletradeItem xiletradeItem, bool isWeapon)
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
                string type = xiletradeItem.ItemFilters[i].Type;
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
            /*
            if (GetEnglishRarity(xiletradeItem.Rarity) is not "unique")
            {
                stats = UpdateWithCountAttribute(stats);
            }
            */
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

