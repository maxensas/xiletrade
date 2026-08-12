using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Contract.One;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class JsonDataFactory
{
    private readonly DataManagerService _dm;

    internal JsonDataFactory(DataManagerService dm)
    {
        _dm = dm;
    }

    /// <summary>
    /// Create a POE1 JSON query for custom search OR search presets.
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

        json.Query.Filters.Socket = GetSocketFilters(xiletradeItem);
        json.Query.Filters.Requirement = GetRequirementFilters(xiletradeItem);
        json.Query.Filters.Armour = GetArmourFilters(xiletradeItem);
        json.Query.Filters.Weapon = GetWeaponFilters(xiletradeItem);
        json.Query.Filters.Misc = GetMiscFilters(xiletradeItem);
        json.Query.Filters.Type = GetTypeFilters(xiletradeItem);
        json.Query.Filters.Trade = GetTradeFilters(xiletradeItem, _dm.Config.Options.SearchBeforeDay, useSaleType: true);

        return json;
    }

    /// <summary>
    /// Create a POE1 JSON query for regular item search.
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
        var name = xiletradeItem.UniqueName.Length > 0 ? xiletradeItem.UniqueName : item.NameGateway;
        var type = item.TypeGateway;
        
        bool simpleMode = xiletradeItem.ByType || name.Length is 0
            || (!item.Flag.Unique && !item.Flag.FoilVariant);

        if (!simpleMode)
        {
            json.Query.Name = name;
            json.Query.Type = type;
        }
        else if (item.Flag.Chart)
        {
            json.Query.Type = new OptionTxt(name, GetChartDiscriminator(item.TypeEn));
        }
        else if (!xiletradeItem.ByType)
        {
            json.Query.Type = item.Flag.Transfigured ? GetTransfiguredGem(name, type) : type;
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
        json.Query.Filters.Map = GetMapFilters(xiletradeItem, item);
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
        Requirement requirement = new()
        {
            Disabled = !xiletradeItem.ReqLevel.Enable
        };

        requirement.Filters.Level.Min = xiletradeItem.ReqLevel.Min;
        requirement.Filters.Level.Max = xiletradeItem.ReqLevel.Max;

        return requirement;
    }

    private static Ultimatum GetUltimatumFilters(XiletradeItem xiletradeItem)
    {
        Ultimatum ultimatum = new();

        if (xiletradeItem.RewardType is not null && xiletradeItem.Reward is not null)
        {
            if (xiletradeItem.RewardType is Strings.Reward.DoubleCurrency or Strings.Reward.DoubleDivCards 
                or Strings.Reward.MirrorRare or Strings.Reward.ExchangeUnique) // ultimatum
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

    private static Map GetMapFilters(XiletradeItem xiletradeItem, ItemData item)
    {
        Map map = new()
        {
            Disabled = !(item.Flag.Map || item.Flag.Chart || item.Flag.SanctumResearch || item.Flag.Logbook)
        };

        if (map.Disabled)
        {
            return map;
        }

        if (xiletradeItem.Lvl.Enable)
        {
            if (item.Flag.Map)
            {
                map.Filters.Tier.Min = xiletradeItem.Lvl.Min;
                map.Filters.Tier.Max = xiletradeItem.Lvl.Max;
            }
            if (item.Flag.SanctumResearch || item.Flag.Logbook || item.Flag.Chart)
            {
                map.Filters.Area.Min = xiletradeItem.Lvl.Min;
                map.Filters.Area.Max = xiletradeItem.Lvl.Max;
            }
        }

        if (item.Flag.Map || item.Flag.Chart)
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
            if (xiletradeItem.MapIiq.Enable)
            {
                map.Filters.Iiq.Min = xiletradeItem.MapIiq.Min;
                map.Filters.Iiq.Max = xiletradeItem.MapIiq.Max;
            }
            if (xiletradeItem.MapIir.Enable)
            {
                map.Filters.Iir.Min = xiletradeItem.MapIir.Min;
                map.Filters.Iir.Max = xiletradeItem.MapIir.Max;
            }
            if (xiletradeItem.MapPack.Enable)
            {
                map.Filters.PackSize.Min = xiletradeItem.MapPack.Min;
                map.Filters.PackSize.Max = xiletradeItem.MapPack.Max;
            }
            if (xiletradeItem.GoldFound.Enable)
            {
                map.Filters.Gold.Min = xiletradeItem.GoldFound.Min;
                map.Filters.Gold.Max = xiletradeItem.GoldFound.Max;
            }
            if (xiletradeItem.DeadSulphur.Enable)
            {
                map.Filters.ChartSulphur.Min = xiletradeItem.DeadSulphur.Min;
                map.Filters.ChartSulphur.Max = xiletradeItem.DeadSulphur.Max;
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

        if (xiletradeItem.Corrupted is DefaultOption.True)
            misc.Filters.Corrupted = GetOptionTrue();
        if (xiletradeItem.Corrupted is DefaultOption.False)
            misc.Filters.Corrupted = GetOptionFalse();

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

        if (xiletradeItem.Split is DefaultOption.True)
            misc.Filters.Split = GetOptionTrue();
        if (xiletradeItem.Split is DefaultOption.False)
            misc.Filters.Split = GetOptionFalse();

        if (xiletradeItem.Crafted is DefaultOption.True)
            misc.Filters.Crafted = GetOptionTrue();
        if (xiletradeItem.Crafted is DefaultOption.False)
            misc.Filters.Crafted = GetOptionFalse();

        if (xiletradeItem.Mutated is DefaultOption.True)
            misc.Filters.Mutated = GetOptionTrue();
        if (xiletradeItem.Mutated is DefaultOption.False)
            misc.Filters.Mutated = GetOptionFalse();

        if (xiletradeItem.Lvl.Enable)
        {
            misc.Filters.Ilvl.Min = xiletradeItem.Lvl.Min;
            misc.Filters.Ilvl.Max = xiletradeItem.Lvl.Max;
        }

        if (xiletradeItem.Quality.Enable)
        {
            misc.Filters.Quality.Min = xiletradeItem.Quality.Min;
            misc.Filters.Quality.Max = xiletradeItem.Quality.Max;
        }

        var activeFilter = misc.Filters.Identified is not null || misc.Filters.Corrupted is not null
            || misc.Filters.Fractured is not null || misc.Filters.Mirrored is not null
            || misc.Filters.Crafted is not null || misc.Filters.Mutated is not null
            || misc.Filters.Split is not null;

        if (activeFilter || xiletradeItem.Lvl.Enable || xiletradeItem.Quality.Enable)
        {
            misc.Disabled = false;
        }

        return misc;
    }

    private static Misc GetMiscFilters(XiletradeItem xiletradeItem, ItemData item, bool influenced)
    {
        Misc misc = new();

        if (xiletradeItem.Quality.Enable)
        {
            misc.Filters.Quality.Min = xiletradeItem.Quality.Min;
            misc.Filters.Quality.Max = xiletradeItem.Quality.Max;
        }
        if (xiletradeItem.MemoryStrand.Enable)
        {
            misc.Filters.MemoryStrand.Min = xiletradeItem.MemoryStrand.Min;
            misc.Filters.MemoryStrand.Max = xiletradeItem.MemoryStrand.Max;
        }
        if (xiletradeItem.FacetorExp.Enable)
        {
            misc.Filters.StoredExp.Min = xiletradeItem.FacetorExp.Min;
            misc.Filters.StoredExp.Max = xiletradeItem.FacetorExp.Max;
        }

        if (!(!xiletradeItem.Lvl.Enable || item.Flag.Gems || item.Flag.Map
            || item.Flag.MiscMapItems || item.Flag.SanctumResearch || item.Flag.Logbook))
        {
            if (!item.Flag.SanctumResearch)
            {
                misc.Filters.Ilvl.Min = xiletradeItem.Lvl.Min;
                misc.Filters.Ilvl.Max = xiletradeItem.Lvl.Max;
            }
        }

        if (xiletradeItem.Lvl.Enable && item.Flag.Gems)
        {
            misc.Filters.Gem_level.Min = xiletradeItem.Lvl.Min;
            misc.Filters.Gem_level.Max = xiletradeItem.Lvl.Max;
        }

        if (xiletradeItem.Corrupted is DefaultOption.True)
            misc.Filters.Corrupted = GetOptionTrue();
        if (xiletradeItem.Corrupted is DefaultOption.False)
            misc.Filters.Corrupted = GetOptionFalse();

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

        if (xiletradeItem.Split is DefaultOption.True)
            misc.Filters.Split = GetOptionTrue();
        if (xiletradeItem.Split is DefaultOption.False)
            misc.Filters.Split = GetOptionFalse();

        if (xiletradeItem.Crafted is DefaultOption.True)
            misc.Filters.Crafted = GetOptionTrue();
        if (xiletradeItem.Crafted is DefaultOption.False)
            misc.Filters.Crafted = GetOptionFalse();

        if (xiletradeItem.Mutated is DefaultOption.True)
            misc.Filters.Mutated = GetOptionTrue();
        if (xiletradeItem.Mutated is DefaultOption.False)
            misc.Filters.Mutated = GetOptionFalse();

        var activeFilter = misc.Filters.Identified is not null || misc.Filters.Corrupted is not null
            || misc.Filters.Fractured is not null || misc.Filters.Mirrored is not null
            || misc.Filters.Crafted is not null || misc.Filters.Mutated is not null
            || misc.Filters.Split is not null;

        misc.Disabled = !(activeFilter || xiletradeItem.FacetorExp.Enable || xiletradeItem.Quality.Enable 
            || xiletradeItem.MemoryStrand.Enable || !item.Flag.Map && 
            (xiletradeItem.Lvl.Enable || influenced || xiletradeItem.SynthesisBlight || xiletradeItem.BlightRavaged)
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
        Weapon weapon = new()
        {
            Disabled = !(xiletradeItem.DpsTotal.Enable || xiletradeItem.DpsPhys.Enable 
                || xiletradeItem.DpsElem.Enable)
        };

        if (xiletradeItem.DpsTotal.Enable)
        {
            weapon.Filters.Damage.Min = xiletradeItem.DpsTotal.Min;
            weapon.Filters.Damage.Max = xiletradeItem.DpsTotal.Max;
        }
        if (xiletradeItem.DpsPhys.Enable)
        {
            weapon.Filters.Pdps.Min = xiletradeItem.DpsPhys.Min;
            weapon.Filters.Pdps.Max = xiletradeItem.DpsPhys.Max;
        }
        if (xiletradeItem.DpsElem.Enable)
        {
            weapon.Filters.Edps.Min = xiletradeItem.DpsElem.Min;
            weapon.Filters.Edps.Max = xiletradeItem.DpsElem.Max;
        }

        return weapon;
    }

    private static Armour GetArmourFilters(XiletradeItem xiletradeItem)
    {
        Armour armour = new()
        {
            Disabled = !(xiletradeItem.Armour.Enable || xiletradeItem.Energy.Enable
                || xiletradeItem.Evasion.Enable || xiletradeItem.Ward.Enable)
        };

        if (xiletradeItem.Armour.Enable)
        {
            armour.Filters.Armour.Min = xiletradeItem.Armour.Min;
            armour.Filters.Armour.Max = xiletradeItem.Armour.Max;
        }
        if (xiletradeItem.Energy.Enable)
        {
            armour.Filters.Energy.Min = xiletradeItem.Energy.Min;
            armour.Filters.Energy.Max = xiletradeItem.Energy.Max;
        }
        if (xiletradeItem.Evasion.Enable)
        {
            armour.Filters.Evasion.Min = xiletradeItem.Evasion.Min;
            armour.Filters.Evasion.Max = xiletradeItem.Evasion.Max;
        }
        if (xiletradeItem.Ward.Enable)
        {
            armour.Filters.Ward.Min = xiletradeItem.Ward.Min;
            armour.Filters.Ward.Max = xiletradeItem.Ward.Max;
        }

        return armour;
    }

    private static Socket GetSocketFilters(XiletradeItem xiletradeItem)
    {
        Socket socket = new()
        {
            Disabled = !(xiletradeItem.Socket.Enable || xiletradeItem.Link.Enable || xiletradeItem.SocketColors)
        };

        if (xiletradeItem.Socket.Enable)
        {
            socket.Filters.Sockets.Min = xiletradeItem.Socket.Min;
            socket.Filters.Sockets.Max = xiletradeItem.Socket.Max;
        }
        if (xiletradeItem.Link.Enable)
        {
            socket.Filters.Links.Min = xiletradeItem.Link.Min;
            socket.Filters.Links.Max = xiletradeItem.Link.Max;
        }
        if (xiletradeItem.SocketColors)
        {
            socket.Filters.Sockets.Red = xiletradeItem.SocketRed;
            socket.Filters.Sockets.Blue = xiletradeItem.SocketBlue;
            socket.Filters.Sockets.Green = xiletradeItem.SocketGreen;
            socket.Filters.Sockets.White = xiletradeItem.SocketWhite;
        }

        return socket;
    }

    private static Sanctum GetSanctumFilters(XiletradeItem xiletradeItem)
    {
        Sanctum sanctum = new()
        {
            Disabled = !(xiletradeItem.Resolve.Enable || xiletradeItem.MaxResolve.Enable
                || xiletradeItem.Inspiration.Enable || xiletradeItem.Aureus.Enable)
        };

        if (xiletradeItem.Resolve.Enable)
        {
            sanctum.Filters.Resolve.Min = xiletradeItem.Resolve.Min;
            sanctum.Filters.Resolve.Max = xiletradeItem.Resolve.Max;
        }
        if (xiletradeItem.MaxResolve.Enable)
        {
            sanctum.Filters.MaxResolve.Min = xiletradeItem.MaxResolve.Min;
            sanctum.Filters.MaxResolve.Max = xiletradeItem.MaxResolve.Max;
        }
        if (xiletradeItem.Inspiration.Enable)
        {
            sanctum.Filters.Inspiration.Min = xiletradeItem.Inspiration.Min;
            sanctum.Filters.Inspiration.Max = xiletradeItem.Inspiration.Max;
        }
        if (xiletradeItem.Aureus.Enable)
        {
            sanctum.Filters.Aureus.Min = xiletradeItem.Aureus.Min;
            sanctum.Filters.Aureus.Max = xiletradeItem.Aureus.Max;
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
            var listFilters = xiletradeItem.ItemFilters.Where(x => x.Id.StartWith(Strings.Stat.TimelessJewel)).FirstOrDefault();
            if (listFilters is not null)
            {
                isTimeLessJewel = true;
                var value = listFilters.Min;
                xiletradeItem.ItemFilters.Clear();
                var filters = filterData.GetEntryStartsWith(Strings.Stat.TimelessJewel);
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

                // For weapons, the pseudo_adds_ [a-z] + _ damage option is given on attack
                var pseudo = Resources.Resources.ResourceManager
                    .GetEnglish(nameof(Resources.Resources.General014_Pseudo));

                if (type_name == pseudo && item.Flag.Weapon && RegexUtil.AddsDamagePattern().IsMatch(id))
                {
                    id += "_to_attacks";
                }

                filter ??= filterResult.FindEntryByIdAndType(id, type);

                stats[0].Filters[idx] = new() { Value = new() };

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
        var rm = Resources.Resources.ResourceManager;
        var returnVal = rarityLang == Resources.Resources.General005_Any ? rm.GetEnglish(nameof(Resources.Resources.General005_Any)) 
            : rarityLang == Resources.Resources.General110_FoilUnique ? rm.GetEnglish(nameof(Resources.Resources.General110_FoilUnique))
            : rarityLang == Resources.Resources.General006_Unique ? rm.GetEnglish(nameof(Resources.Resources.General006_Unique))
            : rarityLang == Resources.Resources.General007_Rare ? rm.GetEnglish(nameof(Resources.Resources.General007_Rare))
            : rarityLang == Resources.Resources.General008_Magic ? rm.GetEnglish(nameof(Resources.Resources.General008_Magic))
            : rarityLang == Resources.Resources.General009_Normal ? rm.GetEnglish(nameof(Resources.Resources.General009_Normal))
            : rarityLang == Resources.Resources.General010_AnyNU ? rm.GetEnglish(nameof(Resources.Resources.General010_AnyNU)) 
            : string.Empty;
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
            inputType is "sanctum" ? Resources.Resources.General111_Sanctum : 
            inputType is "imbued" ? Resources.Resources.General197_ImbuedFilter : string.Empty;
    }

    private GemTransfigured GetTransfiguredGem(ReadOnlySpan<char> vaalGemName, string type)
    {
        var alt = string.Empty;
        var findGem = _dm.Gems.FindGemByName(type);
        bool isVaal = vaalGemName.Length > 0;
        if (findGem is not null)
        {
            if (!isVaal && findGem.Type != findGem.Name) // transfigured normal gem
            {
                type = findGem.Type;
                alt = findGem.Disc;
            }
            if (isVaal && findGem.Type == findGem.Name)
            {
                var findGem2 = _dm.Gems.FindGemByName(vaalGemName);
                if (findGem2 is not null) // transfigured vaal gem
                {
                    alt = findGem2.Disc;
                }
            }
        }
        return new(type, alt);
    }

    private static string GetChartDiscriminator(ReadOnlySpan<char> type)
    {
        int lastSpace = type.LastIndexOf(' ');

        if (lastSpace < 0)
            return type.ToString().ToLowerInvariant();

        ReadOnlySpan<char> lastWord = type[(lastSpace + 1)..];
        ReadOnlySpan<char> remaining = type[..lastSpace];

        return string.Concat(lastWord.ToString().ToLowerInvariant(), "_", 
            remaining.ToString().Replace(' ', '_').ToLowerInvariant());
    }
}
