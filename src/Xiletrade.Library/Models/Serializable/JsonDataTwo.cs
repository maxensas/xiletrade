using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using System.Linq;
using System.Text.RegularExpressions;

namespace Xiletrade.Library.Models.Serializable;

public sealed class JsonDataTwo
{
    [JsonPropertyName("query")]
    public QueryTwo Query { get; set; } = new();

    [DataMember(Name = "sort")]
    [JsonPropertyName("sort")]
    public Sort Sort { get; set; } = new();

    internal JsonDataTwo(XiletradeItem xiletradeItem, ItemBaseName currentItem, bool useSaleType, string market)
    {
        OptionTxt optTrue = new("true"), optFalse = new("false");
        string Inherit = currentItem.Inherits.Length > 0 ? currentItem.Inherits[0] : string.Empty;
        string Inherit2 = currentItem.Inherits.Length > 1 ? currentItem.Inherits[1] : string.Empty;

        //Sort
        Sort.Price = "asc";

        //Query
        Query.Status = new(market);
        if (xiletradeItem.ByType || currentItem.Name.Length is 0 ||
            xiletradeItem.Rarity != Resources.Resources.General006_Unique
            && xiletradeItem.Rarity != Resources.Resources.General110_FoilUnique)
        {
            if ((!xiletradeItem.ByType && Inherit is not Strings.Inherit.Jewels)
                || Inherit is Strings.Inherit.Waystones)
            {
                Query.Type = currentItem.Type;
            }
        }
        else
        {
            Query.Name = currentItem.Name;
            Query.Type = currentItem.Type;
        }

        //Trade
        Query.Filters.Trade.Disabled = DataManager.Config.Options.SearchBeforeDay is 0;
        if (DataManager.Config.Options.SearchBeforeDay is not 0)
        {
            Query.Filters.Trade.Filters.Indexed = new(BeforeDayToString(DataManager.Config.Options.SearchBeforeDay));
        }
        if (useSaleType)
        {
            Query.Filters.Trade.Filters.SaleType = new("priced");
        }
        if (xiletradeItem.PriceMin > 0 && xiletradeItem.PriceMin.IsNotEmpty())
        {
            Query.Filters.Trade.Filters.Price.Min = xiletradeItem.PriceMin;
        }
        //TODO: Query.Filters.Trade.Filters.Collapse

        // Equipment
        if (xiletradeItem.ChkArmour || xiletradeItem.ChkEnergy || xiletradeItem.ChkEvasion ||
            xiletradeItem.ChkDpsTotal || xiletradeItem.ChkDpsPhys || xiletradeItem.ChkDpsElem)
        {
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

            //TODO
            /*
            Query.Filters.Equipment.Filters.Damage
            Query.Filters.Equipment.Filters.EmptyRuneSockets
            Query.Filters.Equipment.Filters.CriticalChance
            Query.Filters.Equipment.Filters.AttacksPerSecond
            Query.Filters.Equipment.Filters.Block
            Query.Filters.Equipment.Filters.Spirit
            */

            Query.Filters.Equipment.Disabled = false;
        }

        // Requirement
        /*
        if () //TODO
        {
            Query.Filters.Requirement.Filters.Level
            Query.Filters.Requirement.Filters.Strength
            Query.Filters.Requirement.Filters.Dexterity
            Query.Filters.Requirement.Filters.Intelligence

            Query.Filters.Requirement.Disabled = false;
        }
        */

        //Waystones
        if (xiletradeItem.ChkLv && Inherit is Strings.Inherit.Maps)
        {
            if (xiletradeItem.LvMin.IsNotEmpty())
                Query.Filters.Map.Filters.Tier.Min = xiletradeItem.LvMin;
            if (xiletradeItem.LvMax.IsNotEmpty())
                Query.Filters.Map.Filters.Tier.Max = xiletradeItem.LvMax;

            //TODO: Query.Filters.Map.Filters.Bonus

            Query.Filters.Map.Disabled = false;
        }

        //Misc
        var isGem = Inherit is Strings.Inherit.Gems;
        var isArea = Inherit is Strings.Inherit.Sanctum or Strings.Inherit.Expedition;// || Inherit2 is "Area";
        var checkLvl = xiletradeItem.ChkLv && (isGem || isArea);
        var checkCorrupted = xiletradeItem.Corrupted is not Strings.any;

        if (checkLvl || checkCorrupted)
        {
            if (isGem)
            {
                if (xiletradeItem.LvMin.IsNotEmpty())
                    Query.Filters.Misc.Filters.GemLevel.Min = xiletradeItem.LvMin;
                if (xiletradeItem.LvMax.IsNotEmpty())
                    Query.Filters.Misc.Filters.GemLevel.Max = xiletradeItem.LvMax;
            }
            if (isArea)
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
        if (Strings.dicInherit.TryGetValue(Inherit, out string option))
        {
            if (xiletradeItem.ByType && Inherit is Strings.Inherit.Weapons or Strings.Inherit.Armours)
            {
                string[] lInherit = currentItem.Inherits;

                if (lInherit.Length > 2)
                {
                    string gearType = lInherit[Inherit is Strings.Inherit.Armours ? 1 : 2].ToLowerInvariant();

                    if (Inherit is Strings.Inherit.Weapons)
                    {
                        gearType = gearType.Replace("hand", string.Empty);
                        gearType = gearType.Remove(gearType.Length - 1);
                        if (gearType is "stave" && lInherit.Length is 4)
                        {
                            gearType = "staff";
                        }
                        else if (gearType is "onethrustingsword")
                        {
                            gearType = "onesword";
                        }
                    }
                    else if (Inherit is Strings.Inherit.Armours && gearType is "shields" or "helmets" or "bodyarmours")
                    {
                        gearType = gearType is "bodyarmours" ? "chest" : gearType.Remove(gearType.Length - 1);
                    }
                    option += "." + gearType;
                }
            }
            Query.Filters.Type.Filters.Category = new(option); // Item category
        }
        if (xiletradeItem.ChkQuality)
        {
            if (xiletradeItem.QualityMin.IsNotEmpty())
                Query.Filters.Type.Filters.Quality.Min = xiletradeItem.QualityMin;
            if (xiletradeItem.QualityMax.IsNotEmpty())
                Query.Filters.Type.Filters.Quality.Max = xiletradeItem.QualityMax;
        }

        var useIlvl = Inherit is Strings.Inherit.Armours or Strings.Inherit.Amulets or Strings.Inherit.Belts
            or Strings.Inherit.Belts or Strings.Inherit.Rings or Strings.Inherit.Quivers or Strings.Inherit.Weapons;
        if (xiletradeItem.ChkLv && useIlvl)
        {
            if (xiletradeItem.LvMin.IsNotEmpty())
                Query.Filters.Type.Filters.ItemLevel.Min = xiletradeItem.LvMin;
            if (xiletradeItem.LvMax.IsNotEmpty())
                Query.Filters.Type.Filters.ItemLevel.Max = xiletradeItem.LvMax;
        }

        //Stats
        Query.Stats = GetStats(xiletradeItem, Inherit);
    }

    private static Stats[] GetStats(XiletradeItem xiletradeItem, string Inherit)
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

                    var filterResult = DataManager.Filter.Result.FirstOrDefault(x => x.Label == type_name);
                    type_name = type_name.ToLowerInvariant();
                    input = Regex.Escape(input).Replace("\\+\\#", "[+]?\\#");

                    System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
                    System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

                    // TO TEST WITH POE2
                    // For weapons, the pseudo_adds_ [a-z] + _ damage option is given on attack
                    string pseudo = rm.GetString("General014_Pseudo", cultureEn);
                    if (type_name == pseudo && Inherit is Strings.Inherit.Weapons && RegexUtil.AddsDamagePattern().IsMatch(id))
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
}
