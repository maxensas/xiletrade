﻿using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Parser;

namespace Xiletrade.Library.Models.Serializable;

public sealed class JsonDataTwo
{
    [JsonPropertyName("query")]
    public QueryTwo Query { get; set; } = new();

    [DataMember(Name = "sort")]
    [JsonPropertyName("sort")]
    public Sort Sort { get; set; } = new();

    internal JsonDataTwo(DataManagerService dm, XiletradeItem xiletradeItem, ItemData item, bool useSaleType, string market)
    {
        OptionTxt optTrue = new("true"), optFalse = new("false");

        //Sort
        Sort.Price = "asc";

        //Query
        Query.Status = new(market);
        if (xiletradeItem.ByType || item.Name.Length is 0 ||
            ! item.Flag.Unique && !item.Flag.FoilVariant)
        {
            if ((!xiletradeItem.ByType && !item.Flag.Jewel) || item.Flag.Waystones)
            {
                Query.Type = item.Type;
            }
        }
        else
        {
            Query.Name = item.Name;
            Query.Type = item.Type;
        }

        //Trade
        Query.Filters.Trade.Disabled = dm.Config.Options.SearchBeforeDay is 0;
        if (dm.Config.Options.SearchBeforeDay is not 0)
        {
            Query.Filters.Trade.Filters.Indexed = new(BeforeDayToString(dm.Config.Options.SearchBeforeDay));
        }
        if (useSaleType)
        {
            Query.Filters.Trade.Filters.SaleType = new("priced");
        }
        if (xiletradeItem.PriceMin > 0 && xiletradeItem.PriceMin.IsNotEmpty())
        {
            Query.Filters.Trade.Filters.Price.Min = xiletradeItem.PriceMin;
        }
        if (xiletradeItem.ExaltOnly)
        {
            Query.Filters.Trade.Disabled = false;
            Query.Filters.Trade.Filters.Price.Option = new("exalted");
        }

        //TODO: Query.Filters.Trade.Filters.Collapse

        // Equipment
        if (xiletradeItem.ChkArmour || xiletradeItem.ChkEnergy || xiletradeItem.ChkEvasion
            || xiletradeItem.ChkDpsTotal || xiletradeItem.ChkDpsPhys || xiletradeItem.ChkDpsElem
            || xiletradeItem.ChkRuneSockets)
        {
            Query.Filters.Equipment = new()
            {
                Disabled = false
            };

            if (xiletradeItem.ChkArmour)
            {
                if (xiletradeItem.ArmourMin.IsNotEmpty())
                    Query.Filters.Equipment.Filters.Armour.Min = xiletradeItem.ArmourMin;
                if (xiletradeItem.ArmourMax.IsNotEmpty())
                    Query.Filters.Equipment.Filters.Armour.Max = xiletradeItem.ArmourMax;
            }
            if (xiletradeItem.ChkEnergy)
            {
                if (xiletradeItem.EnergyMin.IsNotEmpty())
                    Query.Filters.Equipment.Filters.EnergyShield.Min = xiletradeItem.EnergyMin;
                if (xiletradeItem.EnergyMax.IsNotEmpty())
                    Query.Filters.Equipment.Filters.EnergyShield.Max = xiletradeItem.EnergyMax;
            }
            if (xiletradeItem.ChkEvasion)
            {
                if (xiletradeItem.EvasionMin.IsNotEmpty())
                    Query.Filters.Equipment.Filters.Evasion.Min = xiletradeItem.EvasionMin;
                if (xiletradeItem.EvasionMax.IsNotEmpty())
                    Query.Filters.Equipment.Filters.Evasion.Max = xiletradeItem.EvasionMax;
            }
            if (xiletradeItem.ChkDpsTotal)
            {
                if (xiletradeItem.DpsTotalMin.IsNotEmpty())
                    Query.Filters.Equipment.Filters.DamagePerSecond.Min = xiletradeItem.DpsTotalMin;
                if (xiletradeItem.DpsTotalMax.IsNotEmpty())
                    Query.Filters.Equipment.Filters.DamagePerSecond.Max = xiletradeItem.DpsTotalMax;
            }
            if (xiletradeItem.ChkDpsPhys)
            {
                if (xiletradeItem.DpsPhysMin.IsNotEmpty())
                    Query.Filters.Equipment.Filters.PhysicalDps.Min = xiletradeItem.DpsPhysMin;
                if (xiletradeItem.DpsPhysMax.IsNotEmpty())
                    Query.Filters.Equipment.Filters.PhysicalDps.Max = xiletradeItem.DpsPhysMax;
            }
            if (xiletradeItem.ChkDpsElem)
            {
                if (xiletradeItem.DpsElemMin.IsNotEmpty())
                    Query.Filters.Equipment.Filters.ElementalDps.Min = xiletradeItem.DpsElemMin;
                if (xiletradeItem.DpsElemMax.IsNotEmpty())
                    Query.Filters.Equipment.Filters.ElementalDps.Max = xiletradeItem.DpsElemMax;
            }
            if (xiletradeItem.ChkRuneSockets)
            {
                if (xiletradeItem.RuneSocketsMin.IsNotEmpty())
                    Query.Filters.Equipment.Filters.RuneSockets.Min = xiletradeItem.RuneSocketsMin;
                if (xiletradeItem.RuneSocketsMax.IsNotEmpty())
                    Query.Filters.Equipment.Filters.RuneSockets.Max = xiletradeItem.RuneSocketsMax;
            }

            //TODO
            /*
            Query.Filters.Equipment.Filters.Damage
            Query.Filters.Equipment.Filters.EmptyRuneSockets
            Query.Filters.Equipment.Filters.CriticalChance
            Query.Filters.Equipment.Filters.AttacksPerSecond
            Query.Filters.Equipment.Filters.Block
            Query.Filters.Equipment.Filters.Spirit
            */
        }

        // Requirement
        if (xiletradeItem.ChkReqLevel)
        {
            Query.Filters.Requirement = new()
            {
                Disabled = false
            };

            if (xiletradeItem.ReqLevelMin.IsNotEmpty())
                Query.Filters.Requirement.Filters.Level.Min = xiletradeItem.ReqLevelMin;
            if (xiletradeItem.ReqLevelMax.IsNotEmpty())
                Query.Filters.Requirement.Filters.Level.Max = xiletradeItem.ReqLevelMax;
        }
        /*
        if () //TODO
        {
            Query.Filters.Requirement.Filters.Strength
            Query.Filters.Requirement.Filters.Dexterity
            Query.Filters.Requirement.Filters.Intelligence

            Query.Filters.Requirement.Disabled = false;
        }
        */

        //Waystones
        if (xiletradeItem.ChkLv && item.Flag.Waystones)
        {
            Query.Filters.Map = new()
            {
                Disabled = false
            };
            
            if (xiletradeItem.LvMin.IsNotEmpty())
                Query.Filters.Map.Filters.Tier.Min = xiletradeItem.LvMin;
            if (xiletradeItem.LvMax.IsNotEmpty())
                Query.Filters.Map.Filters.Tier.Max = xiletradeItem.LvMax;

            //TODO: Query.Filters.Map.Filters.Bonus
        }

        //Misc
        var checkLvl = xiletradeItem.ChkLv && (item.Flag.Gems || item.Flag.Logbook);
        var checkCorrupted = xiletradeItem.Corrupted is not Strings.any;

        if (checkLvl || checkCorrupted)
        {
            if (item.Flag.Gems)
            {
                if (xiletradeItem.LvMin.IsNotEmpty())
                    Query.Filters.Misc.Filters.GemLevel.Min = xiletradeItem.LvMin;
                if (xiletradeItem.LvMax.IsNotEmpty())
                    Query.Filters.Misc.Filters.GemLevel.Max = xiletradeItem.LvMax;
            }
            if (item.Flag.Logbook)
            {
                if (xiletradeItem.LvMin.IsNotEmpty())
                    Query.Filters.Misc.Filters.AreaLevel.Min = xiletradeItem.LvMin;
                if (xiletradeItem.LvMax.IsNotEmpty())
                    Query.Filters.Misc.Filters.AreaLevel.Max = xiletradeItem.LvMax;
            }

            if (xiletradeItem.Corrupted is "true")
                Query.Filters.Misc.Filters.Corrupted = optTrue;
            if (xiletradeItem.Corrupted is "false")
                Query.Filters.Misc.Filters.Corrupted = optFalse;

            //TODO
            /*
            Query.Filters.Misc.Filters.UnidentifiedTier
            Query.Filters.Misc.Filters.GemSockets
            Query.Filters.Misc.Filters.BaryaSacredWater
            Query.Filters.Misc.Filters.StackSize
            Query.Filters.Misc.Filters.Identified
            Query.Filters.Misc.Filters.Mirrored
            */

            Query.Filters.Misc.Disabled = false;
        }

        //Type
        Query.Filters.Type.Disabled = false;

        string rarityEn = GetEnglishRarity(xiletradeItem.Rarity);
        if (rarityEn.Length > 0)
        {
            rarityEn = rarityEn is "Any N-U" ? "nonunique"
                : rarityEn is "Foil Unique" ? "uniquefoil"
                : rarityEn.ToLowerInvariant();
            if (rarityEn is not Strings.any)
            {
                Query.Filters.Type.Filters.Rarity = new(rarityEn);
            }
        }
        var category = item.Flag.GetItemCategoryApi();
        if (category.Length > 0)
        {
            Query.Filters.Type.Filters.Category = new(category);
        }
        if (xiletradeItem.ChkQuality)
        {
            if (xiletradeItem.QualityMin.IsNotEmpty())
                Query.Filters.Type.Filters.Quality.Min = xiletradeItem.QualityMin;
            if (xiletradeItem.QualityMax.IsNotEmpty())
                Query.Filters.Type.Filters.Quality.Max = xiletradeItem.QualityMax;
        }
        var useIlvl = item.Flag.Weapon || item.Flag.ArmourPiece || item.Flag.Amulets 
            || item.Flag.Belts || item.Flag.Rings || item.Flag.Quivers || item.Flag.UncutGem;
        if (xiletradeItem.ChkLv && useIlvl)
        {
            if (xiletradeItem.LvMin.IsNotEmpty())
                Query.Filters.Type.Filters.ItemLevel.Min = xiletradeItem.LvMin;
            if (xiletradeItem.LvMax.IsNotEmpty())
                Query.Filters.Type.Filters.ItemLevel.Max = xiletradeItem.LvMax;
        }

        //Stats
        Query.Stats = GetStats(dm.Filter, xiletradeItem, item.Flag.Weapon);
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

            stats = UpdateWithCountAttribute(stats);
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
        return inputType is "explicit" ? Resources.Resources.General015_Explicit :
            inputType is "implicit" ? Resources.Resources.General013_Implicit :
            inputType is "enchant" ? Resources.Resources.General011_Enchant :
            inputType is "rune" ? Resources.Resources.General145_Rune : // change to General132_Rune when translated by GGG.
            inputType is "sanctum" ? Resources.Resources.General111_Sanctum :
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
                .Where(x => x.Id is Strings.StatPoe2.Strength
                or Strings.StatPoe2.Dexterity
                or Strings.StatPoe2.Intelligence);
        if (!attributes.Any())
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
