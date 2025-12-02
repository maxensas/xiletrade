using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.One;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class JsonDataFactory
{
    private readonly DataManagerService _dm;

    internal JsonDataFactory(DataManagerService dm)
    {
        _dm = dm;
    }

    /// <summary>
    /// Unidentified unique
    /// </summary>
    /// <param name="xiletradeItem"></param>
    /// <param name="unid"></param>
    /// <param name="market"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    internal JsonData Create(XiletradeItem xiletradeItem, UniqueUnidentified unid, string market, string search)
    {
        var json = new JsonData
        {
            Query = new() { Status = new(market) },
            Sort = new() { Price = "asc" }
        };

        if (!string.IsNullOrEmpty(search))
            json.Query.Term = search;
        else if (unid is not null)
        {
            json.Query.Name = unid.Name;
            json.Query.Type = unid.Type;
        }

        json.Query.Filters.Misc = GetMiscFilters(xiletradeItem);
        json.Query.Filters.Type = GetTypeFilters(xiletradeItem);
        json.Query.Filters.Trade = GetTradeFilters(xiletradeItem, _dm.Config.Options.SearchBeforeDay, useSaleType: true);

        return json;
    }

    /// <summary>
    /// Normal JSON
    /// </summary>
    /// <param name="xiletradeItem"></param>
    /// <param name="item"></param>
    /// <param name="useSaleType"></param>
    /// <param name="market"></param>
    /// <returns></returns>
    internal JsonData Create(XiletradeItem xiletradeItem, ItemData item, bool useSaleType, string market)
    {
        var json = new JsonData
        {
            Query = new() { Status = new(market) },
            Sort = new() { Price = "asc" }
        };

        // Name / type
        bool simpleMode = xiletradeItem.ByType || item.Name.Length is 0
            || (!item.Flag.Unique && !item.Flag.FoilVariant);

        if (!simpleMode)
        {
            json.Query.Name = item.Name;
            json.Query.Type = item.Type;
        }
        else if (!xiletradeItem.ByType)
        {
            json.Query.Type = item.Flag.Transfigured
                ? new GemTransfigured(item.Type, item.Inherits)
                : item.Type;
        }

        bool influenced =
            xiletradeItem.InfShaper || xiletradeItem.InfElder || xiletradeItem.InfCrusader
            || xiletradeItem.InfRedeemer || xiletradeItem.InfHunter || xiletradeItem.InfWarlord;

        // Filters
        json.Query.Filters.Armour = GetArmourFilters(xiletradeItem);
        json.Query.Filters.Weapon = GetWeaponFilters(xiletradeItem);
        json.Query.Filters.Sanctum = GetSanctumFilters(xiletradeItem);
        json.Query.Filters.Trade = GetTradeFilters(xiletradeItem, _dm.Config.Options.SearchBeforeDay, useSaleType);
        json.Query.Filters.Socket = GetSocketFilters(xiletradeItem);
        json.Query.Filters.Misc = GetMiscFilters(xiletradeItem, item, influenced);
        json.Query.Filters.Map = GetMapFilters(xiletradeItem, item, influenced);
        json.Query.Filters.Ultimatum = GetUltimatumFilters(xiletradeItem);
        json.Query.Filters.Requirement = GetRequirementFilters(xiletradeItem);
        json.Query.Filters.Type = GetTypeFilters(xiletradeItem, item);

        // Stats
        bool errorsFilters = false;
        json.Query.Stats = GetStatsFilters(_dm.Filter, xiletradeItem, item, json.Query.Filters.Misc, ref errorsFilters);

        if (errorsFilters)
            ThrowItemFilterErrors(xiletradeItem);

        return json;
    }

    private static TypeF GetTypeFilters(XiletradeItem xiletradeItem)
    {
        TypeF type = new();

        // Rarity
        var rarityEn = GetEnglishRarity(xiletradeItem.Rarity);
        if (rarityEn.Length > 0 && rarityEn is not Strings.any)
            type.Filters.Rarity = new(rarityEn);

        return type;
    }

    private static TypeF GetTypeFilters(XiletradeItem xiletradeItem, ItemData item)
    {
        TypeF type = new();

        // Category
        var category = item.Flag.GetItemCategoryApi();
        if (category.Length > 0)
            type.Filters.Category = new(category);

        // Rarity
        var rarityEn = GetEnglishRarity(xiletradeItem.Rarity);
        if (rarityEn.Length > 0 && rarityEn is not Strings.any)
            type.Filters.Rarity = new(rarityEn);

        return type;
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

    private static Ultimatum GetUltimatumFilters(XiletradeItem xiletradeItem)
    {
        Ultimatum ultimatum = new();

        if (xiletradeItem.RewardType is not null && xiletradeItem.Reward is not null)
        {
            if (xiletradeItem.RewardType is Strings.Reward.DoubleCurrency or Strings.Reward.DoubleDivCards or Strings.Reward.MirrorRare or Strings.Reward.ExchangeUnique) // ultimatum
            {
                ultimatum.Disabled = false;
                ultimatum.Filters.Reward = new(xiletradeItem.RewardType);
                if (xiletradeItem.RewardType is Strings.Reward.DoubleCurrency or Strings.Reward.DoubleDivCards)
                {
                    ultimatum.Filters.Input = new(xiletradeItem.Reward);
                }
                if (xiletradeItem.RewardType is Strings.Reward.ExchangeUnique)
                {
                    ultimatum.Filters.Output = new(xiletradeItem.Reward);
                }
            }
        }

        return ultimatum;
    }

    private static Map GetMapFilters(XiletradeItem xiletradeItem, ItemData item, bool influenced)
    {
        Map map = new()
        {
            Disabled =
                !((item.Flag.Map || item.Flag.MiscMapItems || item.Flag.SanctumResearch || item.Flag.Logbook)
                && (xiletradeItem.ChkLv || xiletradeItem.SynthesisBlight
                || xiletradeItem.BlightRavaged || influenced))
        };

        if (xiletradeItem.ChkLv && item.Flag.Map)
        {
            if (xiletradeItem.LvMin.IsNotEmpty())
                map.Filters.Tier.Min = xiletradeItem.LvMin;
            if (xiletradeItem.LvMax.IsNotEmpty())
                map.Filters.Tier.Max = xiletradeItem.LvMax;
        }

        if (xiletradeItem.ChkLv && (item.Flag.MiscMapItems || item.Flag.SanctumResearch || item.Flag.Logbook))
        {
            if (xiletradeItem.LvMin.IsNotEmpty())
                map.Filters.Area.Min = xiletradeItem.LvMin;
            if (xiletradeItem.LvMax.IsNotEmpty())
                map.Filters.Area.Max = xiletradeItem.LvMax;
        }

        if (item.Flag.Map)
        {
            if (xiletradeItem.InfShaper)
            {
                map.Filters.Shaper = GetOptionTrue();
            }
            if (xiletradeItem.SynthesisBlight)
            {
                map.Filters.Blight = GetOptionTrue();
            }
            if (xiletradeItem.InfElder)
            {
                map.Filters.Elder = GetOptionTrue();
            }
            if (xiletradeItem.BlightRavaged)
            {
                map.Filters.BlightRavaged = GetOptionTrue();
            }

            if (xiletradeItem.ChkMapIiq)
            {
                if (xiletradeItem.MapItemQuantityMin.IsNotEmpty())
                    map.Filters.Iiq.Min = xiletradeItem.MapItemQuantityMin;
                if (xiletradeItem.MapItemQuantityMax.IsNotEmpty())
                    map.Filters.Iiq.Max = xiletradeItem.MapItemQuantityMax;
            }
            if (xiletradeItem.ChkMapIir)
            {
                if (xiletradeItem.MapItemRarityMin.IsNotEmpty())
                    map.Filters.Iir.Min = xiletradeItem.MapItemRarityMin;
                if (xiletradeItem.MapItemRarityMax.IsNotEmpty())
                    map.Filters.Iir.Max = xiletradeItem.MapItemRarityMax;
            }
            if (xiletradeItem.ChkMapPack)
            {
                if (xiletradeItem.MapPackSizeMin.IsNotEmpty())
                    map.Filters.PackSize.Min = xiletradeItem.MapPackSizeMin;
                if (xiletradeItem.MapPackSizeMax.IsNotEmpty())
                    map.Filters.PackSize.Max = xiletradeItem.MapPackSizeMax;
            }
        }
        if (xiletradeItem.RewardType is Strings.Reward.FoilUnique) // valdo box
        {
            map.Filters.MapReward = new(xiletradeItem.Reward);
        }

        return map;
    }

    private static Misc GetMiscFilters(XiletradeItem xiletradeItem)
    {
        Misc misc = new();

        // Corrupted / Identified
        if (xiletradeItem.Corrupted is "true")
            misc.Filters.Corrupted = GetOptionTrue();
        if (xiletradeItem.Corrupted is "false")
            misc.Filters.Corrupted = GetOptionFalse();

        if (xiletradeItem.Identified is "true")
            misc.Filters.Identified = GetOptionTrue();
        if (xiletradeItem.Identified is "false")
            misc.Filters.Identified = GetOptionFalse();

        if (misc.Filters.Identified is not null
         || misc.Filters.Corrupted is not null)
        {
            misc.Disabled = false;
        }

        return misc;
    }

    private static Misc GetMiscFilters(XiletradeItem xiletradeItem, ItemData item, bool influenced)
    {
        Misc misc = new();

        if (xiletradeItem.ChkQuality)
        {
            if (xiletradeItem.QualityMin.IsNotEmpty())
                misc.Filters.Quality.Min = xiletradeItem.QualityMin;
            if (xiletradeItem.QualityMax.IsNotEmpty())
                misc.Filters.Quality.Max = xiletradeItem.QualityMax;
        }

        if (xiletradeItem.ChkMemoryStrand)
        {
            if (xiletradeItem.MemoryStrandMin.IsNotEmpty())
                misc.Filters.MemoryStrand.Min = xiletradeItem.MemoryStrandMin;
            if (xiletradeItem.MemoryStrandMax.IsNotEmpty())
                misc.Filters.MemoryStrand.Max = xiletradeItem.MemoryStrandMax;
        }

        if (xiletradeItem.FacetorExpMin.IsNotEmpty())
            misc.Filters.StoredExp.Min = xiletradeItem.FacetorExpMin;
        if (xiletradeItem.FacetorExpMax.IsNotEmpty())
            misc.Filters.StoredExp.Max = xiletradeItem.FacetorExpMax;

        if (!(!xiletradeItem.ChkLv || item.Flag.Gems || item.Flag.Map
            || item.Flag.MiscMapItems || item.Flag.SanctumResearch || item.Flag.Logbook))
        {
            if (!item.Flag.SanctumResearch)
            {
                if (xiletradeItem.LvMin.IsNotEmpty())
                    misc.Filters.Ilvl.Min = xiletradeItem.LvMin;
                if (xiletradeItem.LvMax.IsNotEmpty())
                    misc.Filters.Ilvl.Max = xiletradeItem.LvMax;
            }
        }

        if (xiletradeItem.ChkLv && item.Flag.Gems)
        {
            if (xiletradeItem.LvMin.IsNotEmpty())
                misc.Filters.Gem_level.Min = xiletradeItem.LvMin;
            if (xiletradeItem.LvMax.IsNotEmpty())
                misc.Filters.Gem_level.Max = xiletradeItem.LvMax;
        }

        /*
        misc.Filters.Synthesis.Option = Strings.any;
        misc.Filters.Split.Option = Strings.any;
        misc.Filters.Mirrored.Option = Strings.any;
        */
        //misc.Filters.Synthesis.Option = Inherit != Strings.Inherit.Maps && itemOptions.SynthesisBlight ? "true" : Strings.any;

        //misc.Filters.Corrupted.Option = itemOptions.Corrupt == 1 ? "true" : (itemOptions.Corrupt == 2 ? "false" : "any");

        if (xiletradeItem.Corrupted is "true")
        {
            misc.Filters.Corrupted = GetOptionTrue();
        }
        else if (xiletradeItem.Corrupted is "false")
        {
            misc.Filters.Corrupted = GetOptionFalse();
        }

        if ((item.Flag.Cluster || item.Flag.Jewel) && item.Flag.Unique && item.Flag.Unidentified)
        {
            misc.Filters.Identified = GetOptionFalse();
        }

        if (item.Flag.Cluster && !item.Flag.Fractured && !item.Flag.Corrupted)
        {
            misc.Filters.Fractured = GetOptionFalse();
        }

        misc.Disabled = !(
            xiletradeItem.FacetorExpMin.IsNotEmpty() || xiletradeItem.FacetorExpMax.IsNotEmpty()
            || xiletradeItem.ChkQuality || xiletradeItem.ChkMemoryStrand || !item.Flag.Map && influenced
            || xiletradeItem.Corrupted is not Strings.any || !item.Flag.Map
            && xiletradeItem.ChkLv || !item.Flag.Map
            && (xiletradeItem.SynthesisBlight || xiletradeItem.BlightRavaged)
        );

        return misc;
    }

    private static Trade GetTradeFilters(XiletradeItem xiletradeItem, int searchConfig, bool useSaleType)
    {
        Trade trade = new()
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
        /*
        trade.Filters.Price.Min = 99999;
        trade.Filters.Price.Max = 99999;
        */
        if (xiletradeItem.PriceMin > 0 && xiletradeItem.PriceMin.IsNotEmpty())
        {
            trade.Filters.Price.Min = xiletradeItem.PriceMin;
        }

        if (xiletradeItem.ChaosDivOnly)
        {
            trade.Disabled = false;
            trade.Filters.Price.Option = new("chaos_divine");
        }

        return trade;
    }

    private static Weapon GetWeaponFilters(XiletradeItem xiletradeItem)
    {
        Weapon weapon = new();

        if (xiletradeItem.ChkDpsTotal || xiletradeItem.ChkDpsPhys || xiletradeItem.ChkDpsElem)
        {
            if (xiletradeItem.ChkDpsTotal)
            {
                if (xiletradeItem.DpsTotalMin.IsNotEmpty())
                    weapon.Filters.Damage.Min = xiletradeItem.DpsTotalMin;
                if (xiletradeItem.DpsTotalMax.IsNotEmpty())
                    weapon.Filters.Damage.Max = xiletradeItem.DpsTotalMax;
            }
            if (xiletradeItem.ChkDpsPhys)
            {
                if (xiletradeItem.DpsPhysMin.IsNotEmpty())
                    weapon.Filters.Pdps.Min = xiletradeItem.DpsPhysMin;
                if (xiletradeItem.DpsPhysMax.IsNotEmpty())
                    weapon.Filters.Pdps.Max = xiletradeItem.DpsPhysMax;
            }
            if (xiletradeItem.ChkDpsElem)
            {
                if (xiletradeItem.DpsElemMin.IsNotEmpty())
                    weapon.Filters.Edps.Min = xiletradeItem.DpsElemMin;
                if (xiletradeItem.DpsElemMax.IsNotEmpty())
                    weapon.Filters.Edps.Max = xiletradeItem.DpsElemMax;
            }

            weapon.Disabled = false;
        }

        return weapon;
    }

    private static Armour GetArmourFilters(XiletradeItem xiletradeItem)
    {
        Armour armour = new();

        if (xiletradeItem.ChkArmour || xiletradeItem.ChkEnergy || xiletradeItem.ChkEvasion || xiletradeItem.ChkWard)
        {
            if (xiletradeItem.ArmourMin.IsNotEmpty())
                armour.Filters.Armour.Min = xiletradeItem.ArmourMin;
            if (xiletradeItem.ArmourMax.IsNotEmpty())
                armour.Filters.Armour.Max = xiletradeItem.ArmourMax;
            if (xiletradeItem.EnergyMin.IsNotEmpty())
                armour.Filters.Energy.Min = xiletradeItem.EnergyMin;
            if (xiletradeItem.EnergyMax.IsNotEmpty())
                armour.Filters.Energy.Max = xiletradeItem.EnergyMax;
            if (xiletradeItem.EvasionMin.IsNotEmpty())
                armour.Filters.Evasion.Min = xiletradeItem.EvasionMin;
            if (xiletradeItem.EvasionMax.IsNotEmpty())
                armour.Filters.Evasion.Max = xiletradeItem.EvasionMax;
            if (xiletradeItem.WardMin.IsNotEmpty())
                armour.Filters.Ward.Min = xiletradeItem.WardMin;
            if (xiletradeItem.WardMax.IsNotEmpty())
                armour.Filters.Ward.Max = xiletradeItem.WardMax;

            armour.Disabled = false;
        }

        return armour;
    }

    private static Socket GetSocketFilters(XiletradeItem xiletradeItem)
    {
        Socket socket = new();

        if (xiletradeItem.ChkSocket || xiletradeItem.ChkLink)
        {
            socket.Disabled = false;

            if (xiletradeItem.ChkSocket)
            {
                if (xiletradeItem.SocketMin.IsNotEmpty())
                    socket.Filters.Sockets.Min = xiletradeItem.SocketMin;
                if (xiletradeItem.SocketMax.IsNotEmpty())
                    socket.Filters.Sockets.Max = xiletradeItem.SocketMax;
            }
            if (xiletradeItem.ChkLink)
            {
                if (xiletradeItem.LinkMin.IsNotEmpty())
                    socket.Filters.Links.Min = xiletradeItem.LinkMin;
                if (xiletradeItem.LinkMax.IsNotEmpty())
                    socket.Filters.Links.Max = xiletradeItem.LinkMax;
            }

            if (xiletradeItem.SocketColors)
            {
                socket.Filters.Sockets.Red = xiletradeItem.SocketRed;
                socket.Filters.Sockets.Blue = xiletradeItem.SocketBlue;
                socket.Filters.Sockets.Green = xiletradeItem.SocketGreen;
                socket.Filters.Sockets.White = xiletradeItem.SocketWhite;
            }
        }

        return socket;
    }

    private static Sanctum GetSanctumFilters(XiletradeItem xiletradeItem)
    {
        Sanctum sanctum = new();

        if (xiletradeItem.ChkResolve || xiletradeItem.ChkMaxResolve || xiletradeItem.ChkInspiration || xiletradeItem.ChkAureus)
        {
            if (xiletradeItem.ChkResolve)
            {
                if (xiletradeItem.ResolveMin.IsNotEmpty())
                    sanctum.Filters.Resolve.Min = xiletradeItem.ResolveMin;
                if (xiletradeItem.ResolveMax.IsNotEmpty())
                    sanctum.Filters.Resolve.Max = xiletradeItem.ResolveMax;
            }

            if (xiletradeItem.ChkMaxResolve)
            {
                if (xiletradeItem.MaxResolveMin.IsNotEmpty())
                    sanctum.Filters.MaxResolve.Min = xiletradeItem.MaxResolveMin;
                if (xiletradeItem.MaxResolveMax.IsNotEmpty())
                    sanctum.Filters.MaxResolve.Max = xiletradeItem.MaxResolveMax;
            }

            if (xiletradeItem.ChkInspiration)
            {
                if (xiletradeItem.InspirationMin.IsNotEmpty())
                    sanctum.Filters.Inspiration.Min = xiletradeItem.InspirationMin;
                if (xiletradeItem.InspirationMax.IsNotEmpty())
                    sanctum.Filters.Inspiration.Max = xiletradeItem.InspirationMax;
            }

            if (xiletradeItem.ChkAureus)
            {
                if (xiletradeItem.AureusMin.IsNotEmpty())
                    sanctum.Filters.Aureus.Min = xiletradeItem.AureusMin;
                if (xiletradeItem.AureusMax.IsNotEmpty())
                    sanctum.Filters.Aureus.Max = xiletradeItem.AureusMax;
            }

            sanctum.Disabled = false;
        }

        return sanctum;
    }

    private static Stats[] GetStatsFilters(FilterData filterData, XiletradeItem xiletradeItem, ItemData item, Misc miscFilter, ref bool errorsFilters)
    {
        if (xiletradeItem.ItemFilters.Count is 0)
        {
            return null;
        }

        bool isTimeLessJewel = false;
        if (item.Flag.Unique && item.Flag.Jewel)
        {
            var listFilters = xiletradeItem.ItemFilters.Where(x => x.Id.StartWith(Strings.Stat.TimelessJewel));
            if (listFilters.Any())
            {
                isTimeLessJewel = true;
                var value = listFilters.First().Min;
                xiletradeItem.ItemFilters.Clear();

                var filters = filterData.Result.SelectMany(result => result.Entries)
                    .Where(filter => filter.ID.StartWith(Strings.Stat.TimelessJewel));
                foreach (var filter in filters)
                {
                    var itemFilter = new ItemFilter(filterData, filter.ID, value, value);
                    xiletradeItem.ItemFilters.Add(itemFilter);
                }
            }
        }

        Stats[] stats =
        [
            new()
            {
                Type = "and",
                Filters = new StatsFilters[xiletradeItem.ItemFilters.Count]
            },
        ];
        if (isTimeLessJewel)
        {
            stats[0].Type = "count";
            stats[0].Value = new()
            {
                Min = 1
            };
        }
        var highValueBase = false;
        int idx = 0;
        for (int i = 0; i < xiletradeItem.ItemFilters.Count; i++)
        {
            if ((item.Flag.Rings || item.Flag.Amulets)
                && Strings.Stat.lMagnitudeImplicits.Contains(xiletradeItem.ItemFilters[i].Id))
            {
                highValueBase = true;
            }

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

                // For weapons, the pseudo_adds_ [a-z] + _ damage option is given on attack
                string pseudo = rm.GetString("General014_Pseudo", cultureEn);

                //bool isShako = DataManager.Words.FirstOrDefault(x => x.NameEn is "Forbidden Shako").Name == Modifier.CurrentItem.Name;
                //if (type_name == pseudo && Inherit is Strings.Inherit.Weapons && Regex.IsMatch(id, @"^pseudo.pseudo_adds_[a-z]+_damage$"))
                if (type_name == pseudo && item.Flag.Weapon && RegexUtil.AddsDamagePattern().IsMatch(id))
                {
                    id += "_to_attacks";
                }/*
                            else if (type_name != pseudo && (Inherit is Strings.Inherit.Weapons or Strings.Inherit.Armours) && !isShako)
                            {
                                // Is the equipment only option (specific)
                                Regex rgx = new("^" + input + "$", RegexOptions.IgnoreCase);
                                filter = filterResult.Entries.FirstOrDefault(x => rgx.IsMatch(x.Text) && x.Type == type);
                            }*/

                filter ??= filterResult.Entries.FirstOrDefault(x => x.ID == id && x.Type == type); // && x.Part == null

                stats[0].Filters[idx] = new() { Value = new() };
                //Query.Stats[0].Filters[idx].Value.Option = 99999;

                if (filter is not null && filter.ID is not null && filter.ID.Trim().Length > 0)
                {
                    stats[0].Filters[idx].Disabled = xiletradeItem.ItemFilters[i].Disabled == true;

                    if (xiletradeItem.ItemFilters[i].Option != 0 && xiletradeItem.ItemFilters[i].Option.IsNotEmpty())
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
        if (highValueBase)
        {
            if (!item.Flag.Mirrored)
            {
                miscFilter.Disabled = false;
                miscFilter.Filters.Mirrored = GetOptionFalse();
            }
            if (!item.Flag.Split)
            {
                miscFilter.Disabled = false;
                miscFilter.Filters.Split = GetOptionFalse();
            }
        }

        return stats;
    }

    private static void ThrowItemFilterErrors(XiletradeItem xiletradeItem)
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

        throw new Exception(
            $"{errorCount} Mod error(s) detected:\r\n\r\nMod lines : {errors}\r\n\r\n");
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
        var cultureEn = new System.Globalization.CultureInfo(Strings.Culture[0]);
        var rm = new System.Resources.ResourceManager(typeof(Resources.Resources));

        var returnVal = rarityLang == Resources.Resources.General005_Any ? rm.GetString("General005_Any", cultureEn) :
            rarityLang == Resources.Resources.General110_FoilUnique ? rm.GetString("General110_FoilUnique", cultureEn) :
            rarityLang == Resources.Resources.General006_Unique ? rm.GetString("General006_Unique", cultureEn) :
            rarityLang == Resources.Resources.General007_Rare ? rm.GetString("General007_Rare", cultureEn) :
            rarityLang == Resources.Resources.General008_Magic ? rm.GetString("General008_Magic", cultureEn) :
            rarityLang == Resources.Resources.General009_Normal ? rm.GetString("General009_Normal", cultureEn) :
            rarityLang == Resources.Resources.General010_AnyNU ? rm.GetString("General010_AnyNU", cultureEn) : string.Empty;
        if (returnVal.Length > 0)
        {
            returnVal = returnVal is "Any N-U" ? "nonunique"
                : returnVal is "Foil Unique" ? "uniquefoil"
                : returnVal.ToLowerInvariant();
        }
        return returnVal;
    }

    private static string GetAffixType(string inputType)
    {
        return inputType is "pseudo" ? Resources.Resources.General014_Pseudo :
            inputType is "explicit" ? Resources.Resources.General015_Explicit :
            inputType is "fractured" ? Resources.Resources.General016_Fractured :
            inputType is "crafted" ? Resources.Resources.General012_Crafted :
            inputType is "implicit" ? Resources.Resources.General013_Implicit :
            inputType is "enchant" ? Resources.Resources.General011_Enchant :
            inputType is "monster" ? Resources.Resources.General018_Monster :
            inputType is "veiled" ? Resources.Resources.General019_Veiled :
            inputType is "delve" ? Resources.Resources.General020_Delve :
            inputType is "ultimatum" ? Resources.Resources.General069_Ultimatum :
            inputType is "scourge" ? Resources.Resources.General099_Scourge :
            inputType is "crucible" ? Resources.Resources.General112_Crucible :
            inputType is "necropolis" ? Resources.Resources.General131_Necropolis :
            inputType is "sanctum" ? Resources.Resources.General111_Sanctum : string.Empty;
    }
}
