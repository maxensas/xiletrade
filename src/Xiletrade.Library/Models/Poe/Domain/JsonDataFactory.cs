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
    /// <param name="xItem"></param>
    /// <param name="unid"></param>
    /// <param name="market"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    internal JsonData Create(XiletradeItem xItem, UniqueUnidentified unid, string market, string search)
    {
        var json = new JsonData(market);

        if (!string.IsNullOrEmpty(search))
            json.Query.Term = search;
        else if (unid is not null)
        {
            json.Query.Name = unid.Name;
            json.Query.Type = unid.Type;
        }

        json.Query.Filters.Socket = GetSocketFilters(xItem);
        json.Query.Filters.Requirement = GetRequirementFilters(xItem);
        json.Query.Filters.Armour = GetArmourFilters(xItem);
        json.Query.Filters.Weapon = GetWeaponFilters(xItem);
        json.Query.Filters.Misc = GetMiscFilters(xItem);
        json.Query.Filters.Type = GetTypeFilters(xItem);
        json.Query.Filters.Trade = GetTradeFilters(xItem, _dm.Config.Options.SearchBeforeDay, useSaleType: true);

        return json;
    }

    /// <summary>
    /// Create a POE1 JSON query for regular item search.
    /// </summary>
    /// <param name="xItem"></param>
    /// <param name="item"></param>
    /// <param name="useSaleType"></param>
    /// <param name="market"></param>
    /// <returns></returns>
    internal JsonData Create(XiletradeItem xItem, ItemData item, bool useSaleType, string market)
    {
        var json = new JsonData(market);

        // Name / type
        var name = xItem.UniqueName.Length > 0 ? xItem.UniqueName : item.NameGateway;
        var type = item.TypeGateway;
        
        bool simpleMode = xItem.ByType || name.Length is 0
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
        else if (!xItem.ByType)
        {
            json.Query.Type = item.Flag.Transfigured ? GetTransfiguredGem(name, type) : type;
        }

        // Filters
        json.Query.Filters.Armour = GetArmourFilters(xItem);
        json.Query.Filters.Weapon = GetWeaponFilters(xItem);
        json.Query.Filters.Sanctum = GetSanctumFilters(xItem);
        json.Query.Filters.Trade = GetTradeFilters(xItem, _dm.Config.Options.SearchBeforeDay, useSaleType);
        json.Query.Filters.Socket = GetSocketFilters(xItem);
        json.Query.Filters.Misc = GetMiscFilters(xItem, item);
        json.Query.Filters.Map = GetMapFilters(xItem, item);
        json.Query.Filters.Ultimatum = GetUltimatumFilters(xItem);
        json.Query.Filters.Requirement = GetRequirementFilters(xItem);
        json.Query.Filters.Type = GetTypeFilters(xItem, item);
        json.Query.Filters.Heist = GetHeistFilters(xItem);

        // Stats
        bool errorsFilters = false;
        json.Query.Stats = GetStatsFilters(_dm.Filter, xItem, item, json.Query.Filters.Misc, ref errorsFilters);

        if (errorsFilters)
            ThrowItemFilterErrors(xItem);

        return json;
    }

    private static TypeF GetTypeFilters(XiletradeItem xItem)
    {
        var rarityEn = GetEnglishRarity(xItem.Rarity);
        if (rarityEn.Length > 0 && rarityEn is not Strings.any)
        {
            TypeF type = new();
            type.Filters.Rarity = new(rarityEn);
            return type;
        }
        return null;
    }

    private static TypeF GetTypeFilters(XiletradeItem xItem, ItemData item)
    {
        var category = item.Flag.GetItemCategoryApi();
        var isCategory = category.Length > 0;

        var rarityEn = GetEnglishRarity(xItem.Rarity);
        var isRarity = rarityEn.Length > 0 && rarityEn is not Strings.any;

        if (!(isRarity || isCategory))
        {
            return null;
        }

        TypeF type = new();

        if (isCategory)
        {
            type.Filters.Category = new(category);
        }
        if (isRarity)
        {
            type.Filters.Rarity = new(rarityEn);
        }

        return type;
    }

    private static Requirement GetRequirementFilters(XiletradeItem xItem)
    {
        if (!xItem.ReqLevel.Enable)
        {
            return null;
        }

        Requirement requirement = new();
        requirement.Filters.Level = new() { Min = xItem.ReqLevel.Min, Max = xItem.ReqLevel.Max };
        return requirement;
    }

    private static Ultimatum GetUltimatumFilters(XiletradeItem xItem)
    {
        var enable = xItem.RewardType is not null && xItem.Reward is not null
            && xItem.RewardType is Strings.Reward.DoubleCurrency or Strings.Reward.DoubleDivCards
                or Strings.Reward.MirrorRare or Strings.Reward.ExchangeUnique;
        if (!enable)
        {
            return null;
        }

        Ultimatum ultimatum = new();

        ultimatum.Filters.Reward = new(xItem.RewardType);
        if (xItem.RewardType is Strings.Reward.DoubleCurrency or Strings.Reward.DoubleDivCards)
        {
            ultimatum.Filters.Input = new(xItem.Reward);
        }
        if (xItem.RewardType is Strings.Reward.ExchangeUnique)
        {
            ultimatum.Filters.Output = new(xItem.Reward);
        }

        return ultimatum;
    }

    private static Map GetMapFilters(XiletradeItem xItem, ItemData item)
    {
        var enable = item.Flag.Map || item.Flag.Chart || item.Flag.SanctumResearch || item.Flag.Logbook;
        if (!enable)
        {
            return null;
        }

        Map map = new();

        if (xItem.Lvl.Enable)
        {
            if (item.Flag.Map)
            {
                map.Filters.Tier = new() { Min = xItem.Lvl.Min, Max = xItem.Lvl.Max };
            }
            if (item.Flag.SanctumResearch || item.Flag.Logbook || item.Flag.Chart)
            {
                map.Filters.Area = new() { Min = xItem.Lvl.Min, Max = xItem.Lvl.Max };
            }
        }

        if (item.Flag.Map || item.Flag.Chart)
        {
            if (xItem.InfShaper)
            {
                map.Filters.Shaper = GetOptionTrue();
            }
            if (xItem.SynthesisBlight)
            {
                map.Filters.Blight = GetOptionTrue();
            }
            if (xItem.InfElder)
            {
                map.Filters.Elder = GetOptionTrue();
            }
            if (xItem.BlightRavaged)
            {
                map.Filters.BlightRavaged = GetOptionTrue();
            }
            if (xItem.MapIiq.Enable)
            {
                map.Filters.Iiq = new() { Min = xItem.MapIiq.Min, Max = xItem.MapIiq.Max };
            }
            if (xItem.MapIir.Enable)
            {
                map.Filters.Iir = new() { Min = xItem.MapIir.Min, Max = xItem.MapIir.Max };
            }
            if (xItem.MapPack.Enable)
            {
                map.Filters.PackSize = new() { Min = xItem.MapPack.Min, Max = xItem.MapPack.Max };
            }
            if (xItem.GoldFound.Enable)
            {
                map.Filters.Gold = new() { Min = xItem.GoldFound.Min, Max = xItem.GoldFound.Max };
            }
            if (xItem.DeadSulphur.Enable)
            {
                map.Filters.ChartSulphur = new() { Min = xItem.DeadSulphur.Min, Max = xItem.DeadSulphur.Max };
            }
        }
        if (xItem.RewardType is Strings.Reward.FoilUnique) // valdo box
        {
            map.Filters.MapReward = new(xItem.Reward);
        }

        return map;
    }

    private static Misc GetMiscFilters(XiletradeItem xItem)
    {
        Misc misc = new();

        if (xItem.Corrupted is DefaultOption.True)
            misc.Filters.Corrupted = GetOptionTrue();
        if (xItem.Corrupted is DefaultOption.False)
            misc.Filters.Corrupted = GetOptionFalse();

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

        if (xItem.Split is DefaultOption.True)
            misc.Filters.Split = GetOptionTrue();
        if (xItem.Split is DefaultOption.False)
            misc.Filters.Split = GetOptionFalse();

        if (xItem.Crafted is DefaultOption.True)
            misc.Filters.Crafted = GetOptionTrue();
        if (xItem.Crafted is DefaultOption.False)
            misc.Filters.Crafted = GetOptionFalse();

        if (xItem.Mutated is DefaultOption.True)
            misc.Filters.Mutated = GetOptionTrue();
        if (xItem.Mutated is DefaultOption.False)
            misc.Filters.Mutated = GetOptionFalse();

        if (xItem.Lvl.Enable)
        {
            misc.Filters.Ilvl = new() { Min = xItem.Lvl.Min, Max = xItem.Lvl.Max };
        }

        if (xItem.Quality.Enable)
        {
            misc.Filters.Quality = new() { Min = xItem.Quality.Min, Max = xItem.Quality.Max };
        }

        var activeFilter = misc.Filters.Identified is not null || misc.Filters.Corrupted is not null
            || misc.Filters.Fractured is not null || misc.Filters.Mirrored is not null
            || misc.Filters.Crafted is not null || misc.Filters.Mutated is not null
            || misc.Filters.Split is not null;

        var enable = activeFilter || xItem.Lvl.Enable || xItem.Quality.Enable;
        return enable ? misc : null;
    }

    private static Misc GetMiscFilters(XiletradeItem xItem, ItemData item)
    {
        Misc misc = new();

        if (xItem.Quality.Enable)
        {
            misc.Filters.Quality = new() { Min = xItem.Quality.Min, Max = xItem.Quality.Max };
        }
        if (xItem.MemoryStrand.Enable)
        {
            misc.Filters.MemoryStrand = new() { Min = xItem.MemoryStrand.Min, Max = xItem.MemoryStrand.Max };
        }
        if (xItem.Intangibility.Enable)
        {
            misc.Filters.Intangibility = new() { Min = xItem.Intangibility.Min, Max = xItem.Intangibility.Max };
        }
        if (xItem.FacetorExp.Enable)
        {
            misc.Filters.StoredExp = new() { Min = xItem.FacetorExp.Min, Max = xItem.FacetorExp.Max };
        }

        if (!(!xItem.Lvl.Enable || item.Flag.Gems || item.Flag.Map
            || item.Flag.MiscMapItems || item.Flag.SanctumResearch || item.Flag.Logbook))
        {
            if (!item.Flag.SanctumResearch)
            {
                misc.Filters.Ilvl = new() { Min = xItem.Lvl.Min, Max = xItem.Lvl.Max };
            }
        }

        if (xItem.Lvl.Enable && item.Flag.Gems)
        {
            misc.Filters.Gem_level = new() { Min = xItem.Lvl.Min, Max = xItem.Lvl.Max };
        }

        if (xItem.Corrupted is DefaultOption.True)
            misc.Filters.Corrupted = GetOptionTrue();
        if (xItem.Corrupted is DefaultOption.False)
            misc.Filters.Corrupted = GetOptionFalse();

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

        if (xItem.Split is DefaultOption.True)
            misc.Filters.Split = GetOptionTrue();
        if (xItem.Split is DefaultOption.False)
            misc.Filters.Split = GetOptionFalse();

        if (xItem.Crafted is DefaultOption.True)
            misc.Filters.Crafted = GetOptionTrue();
        if (xItem.Crafted is DefaultOption.False)
            misc.Filters.Crafted = GetOptionFalse();

        if (xItem.Mutated is DefaultOption.True)
            misc.Filters.Mutated = GetOptionTrue();
        if (xItem.Mutated is DefaultOption.False)
            misc.Filters.Mutated = GetOptionFalse();

        var activeFilter = misc.Filters.Identified is not null || misc.Filters.Corrupted is not null
            || misc.Filters.Fractured is not null || misc.Filters.Mirrored is not null
            || misc.Filters.Crafted is not null || misc.Filters.Mutated is not null
            || misc.Filters.Split is not null;

        var enable = (activeFilter || xItem.FacetorExp.Enable || xItem.Quality.Enable 
            || xItem.MemoryStrand.Enable || xItem.Intangibility.Enable || !item.Flag.Map && 
            (xItem.Lvl.Enable || xItem.IsInfluenced 
            || xItem.SynthesisBlight || xItem.BlightRavaged)
        );

        return enable ? misc : null;
    }

    private static Trade GetTradeFilters(XiletradeItem xItem, int searchConfig, bool useSaleType)
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

        if (xItem.PriceMin > 0 && xItem.PriceMin.IsNotEmpty())
        {
            trade.Filters.Price = new();
            trade.Filters.Price.Min = xItem.PriceMin;
        }

        if (xItem.ChaosDivOnly)
        {
            trade.Disabled = false;

            if (trade.Filters.Price is null)
            {
                trade.Filters.Price = new();
            }
            trade.Filters.Price.Option = new("chaos_divine");
        }

        return trade.Disabled ? null : trade;
    }

    private static Weapon GetWeaponFilters(XiletradeItem xItem)
    {
        if (!(xItem.DpsTotal.Enable || xItem.DpsPhys.Enable || xItem.DpsElem.Enable))
        {
            return null;
        }
        
        Weapon weapon = new();

        if (xItem.DpsTotal.Enable)
        {
            weapon.Filters.Damage = new() { Min = xItem.DpsTotal.Min, Max = xItem.DpsTotal.Max };
        }
        if (xItem.DpsPhys.Enable)
        {
            weapon.Filters.Pdps = new() { Min = xItem.DpsPhys.Min, Max = xItem.DpsPhys.Max };
        }
        if (xItem.DpsElem.Enable)
        {
            weapon.Filters.Edps = new() { Min = xItem.DpsElem.Min, Max = xItem.DpsElem.Max };
        }

        return weapon;
    }

    private static Armour GetArmourFilters(XiletradeItem xItem)
    {
        if (!(xItem.Armour.Enable || xItem.Energy.Enable || xItem.Evasion.Enable || xItem.Ward.Enable))
        {
            return null;
        }
        
        Armour armour = new();

        if (xItem.Armour.Enable)
        {
            armour.Filters.Armour = new() { Min = xItem.Armour.Min, Max = xItem.Armour.Max };
        }
        if (xItem.Energy.Enable)
        {
            armour.Filters.Energy = new() { Min = xItem.Energy.Min, Max = xItem.Energy.Max };
        }
        if (xItem.Evasion.Enable)
        {
            armour.Filters.Evasion = new() { Min = xItem.Evasion.Min, Max = xItem.Evasion.Max };
        }
        if (xItem.Ward.Enable)
        {
            armour.Filters.Ward = new() { Min = xItem.Ward.Min, Max = xItem.Ward.Max };
        }

        return armour;
    }

    private static Socket GetSocketFilters(XiletradeItem xItem)
    {
        if (!(xItem.Socket.Enable || xItem.Link.Enable || xItem.SocketColors))
        {
            return null;
        }
        
        Socket socket = new();

        if (xItem.Socket.Enable)
        {
            socket.Filters.Sockets = new() { Min = xItem.Socket.Min, Max = xItem.Socket.Max };
            if (xItem.SocketColors)
            {
                socket.Filters.Sockets.Red = xItem.SocketRed;
                socket.Filters.Sockets.Blue = xItem.SocketBlue;
                socket.Filters.Sockets.Green = xItem.SocketGreen;
                socket.Filters.Sockets.White = xItem.SocketWhite;
            }
        }
        if (xItem.Link.Enable)
        {
            socket.Filters.Links = new() { Min = xItem.Link.Min, Max = xItem.Link.Max };
        }

        return socket;
    }

    private static Sanctum GetSanctumFilters(XiletradeItem xItem)
    {
        if (!(xItem.Resolve.Enable || xItem.MaxResolve.Enable || xItem.Inspiration.Enable || xItem.Aureus.Enable))
        {
            return null;
        }
        
        Sanctum sanctum = new();

        if (xItem.Resolve.Enable)
        {
            sanctum.Filters.Resolve = new() { Min = xItem.Resolve.Min, Max = xItem.Resolve.Max };
        }
        if (xItem.MaxResolve.Enable)
        {
            sanctum.Filters.MaxResolve = new() { Min = xItem.MaxResolve.Min, Max = xItem.MaxResolve.Max };
        }
        if (xItem.Inspiration.Enable)
        {
            sanctum.Filters.Inspiration = new() { Min = xItem.Inspiration.Min, Max = xItem.Inspiration.Max };
        }
        if (xItem.Aureus.Enable)
        {
            sanctum.Filters.Aureus = new() { Min = xItem.Aureus.Min, Max = xItem.Aureus.Max };
        }

        return sanctum;
    }

    private static Heist GetHeistFilters(XiletradeItem xItem)
    {
        var objective = xItem.RewardType is not null
            && xItem.RewardType is Strings.Objective.Moderate or Strings.Objective.High
            or Strings.Objective.Precious or Strings.Objective.Priceless;
        var require = xItem.HeistLockpicking.Enable || xItem.HeistDemolition.Enable || xItem.HeistCounterThaumaturgy.Enable
            || xItem.HeistTrapDisarmament.Enable || xItem.HeistAgility.Enable || xItem.HeistEngineering.Enable
            || xItem.HeistBruteForce.Enable || xItem.HeistPerception.Enable || xItem.HeistDeception.Enable;

        if (!(xItem.HeistRevealedWings.Enable || xItem.HeistRevealedEscapeRoutes.Enable 
            || xItem.HeistRevealedRewardRooms.Enable || xItem.HeistTotalWings.Enable
            || xItem.HeistTotalEscapeRoutes.Enable || xItem.HeistTotalRewardRooms.Enable
            || objective || require))
        {
            return null;
        }

        Heist heist = new();

        // heist target
        if (objective)
        {
            heist.Filters.ObjectiveValue = new(xItem.RewardType);
        }

        // requires
        if (xItem.HeistLockpicking.Enable)
        {
            heist.Filters.Lockpicking = new() { Min = xItem.HeistLockpicking.Min, Max = xItem.HeistLockpicking.Max };
        }
        if (xItem.HeistDemolition.Enable)
        {
            heist.Filters.Demolition = new() { Min = xItem.HeistDemolition.Min, Max = xItem.HeistDemolition.Max };
        }
        if (xItem.HeistCounterThaumaturgy.Enable)
        {
            heist.Filters.Thaumaturgy = new() { Min = xItem.HeistCounterThaumaturgy.Min, Max = xItem.HeistCounterThaumaturgy.Max };
        }
        if (xItem.HeistTrapDisarmament.Enable)
        {
            heist.Filters.Disarmament = new() { Min = xItem.HeistTrapDisarmament.Min, Max = xItem.HeistTrapDisarmament.Max };
        }
        if (xItem.HeistAgility.Enable)
        {
            heist.Filters.Agility = new() { Min = xItem.HeistAgility.Min, Max = xItem.HeistAgility.Max };
        }
        if (xItem.HeistEngineering.Enable)
        {
            heist.Filters.Engineering = new() { Min = xItem.HeistEngineering.Min, Max = xItem.HeistEngineering.Max };
        }
        if (xItem.HeistBruteForce.Enable)
        {
            heist.Filters.BruteForce = new() { Min = xItem.HeistBruteForce.Min, Max = xItem.HeistBruteForce.Max };
        }
        if (xItem.HeistPerception.Enable)
        {
            heist.Filters.Perception = new() { Min = xItem.HeistPerception.Min, Max = xItem.HeistPerception.Max };
        }
        if (xItem.HeistDeception.Enable)
        {
            heist.Filters.Deception = new() { Min = xItem.HeistDeception.Min, Max = xItem.HeistDeception.Max };
        }

        // blueprints
        if (xItem.HeistRevealedWings.Enable)
        {
            heist.Filters.Wings = new() { Min = xItem.HeistRevealedWings.Min, Max = xItem.HeistRevealedWings.Max };
        }
        if (xItem.HeistRevealedEscapeRoutes.Enable)
        {
            heist.Filters.EscapeRoutes = new() { Min = xItem.HeistRevealedEscapeRoutes.Min, Max = xItem.HeistRevealedEscapeRoutes.Max };
        }
        if (xItem.HeistRevealedRewardRooms.Enable)
        {
            heist.Filters.RewardRooms = new() { Min = xItem.HeistRevealedRewardRooms.Min, Max = xItem.HeistRevealedRewardRooms.Max };
        }
        if (xItem.HeistTotalWings.Enable)
        {
            heist.Filters.MaxWings = new() { Min = xItem.HeistTotalWings.Min, Max = xItem.HeistTotalWings.Max };
        }
        if (xItem.HeistTotalEscapeRoutes.Enable)
        {
            heist.Filters.MaxEscapeRoutes = new() { Min = xItem.HeistTotalEscapeRoutes.Min, Max = xItem.HeistTotalEscapeRoutes.Max };
        }
        if (xItem.HeistTotalRewardRooms.Enable)
        {
            heist.Filters.MaxRewardRooms = new() { Min = xItem.HeistTotalRewardRooms.Min, Max = xItem.HeistTotalRewardRooms.Max };
        }

        return heist;
    }

    private static Stats[] GetStatsFilters(FilterData filterData, XiletradeItem xItem, ItemData item, Misc miscFilter, ref bool errorsFilters)
    {
        if (xItem.ItemFilters is null || xItem.ItemFilters.Count is 0)
        {
            return null;
        }

        bool isTimeLessJewel = false;
        if (item.Flag.Unique && item.Flag.Jewel)
        {
            var listFilters = xItem.ItemFilters.Where(x => x.Id.StartWith(Strings.Stat.TimelessJewel)).FirstOrDefault();
            if (listFilters is not null)
            {
                isTimeLessJewel = true;
                var value = listFilters.Min;
                xItem.ItemFilters.Clear();
                var filters = filterData.GetEntryStartsWith(Strings.Stat.TimelessJewel);
                foreach (var filter in filters)
                {
                    var itemFilter = new ItemFilter(filterData, filter.ID, value, value);
                    xItem.ItemFilters.Add(itemFilter);
                }
            }
        }

        Stats[] stats =
        [
            new()
            {
                Type = "and",
                Filters = new StatsFilters[xItem.ItemFilters.Count]
            }
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
        for (int i = 0; i < xItem.ItemFilters.Count; i++)
        {
            if ((item.Flag.Rings || item.Flag.Amulets)
                && Strings.Stat.lMagnitudeImplicits.Contains(xItem.ItemFilters[i].Id))
            {
                highValueBase = true;
            }

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
                    stats[0].Filters[idx].Disabled = xItem.ItemFilters[i].Disabled == true;

                    if (xItem.ItemFilters[i].Option != 0 && xItem.ItemFilters[i].Option.IsNotEmpty())
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

    private static void ThrowItemFilterErrors(XiletradeItem xItem)
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
