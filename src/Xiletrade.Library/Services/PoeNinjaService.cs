using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Models.Ninja.Contract.Exchange;
using Xiletrade.Library.Models.Ninja.Contract.Exchange.Detail;
using Xiletrade.Library.Models.Ninja.Domain;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

/// <summary>
/// Service used to manage cache data for poe ninja.
/// </summary>
/// <remarks>
/// One unique service for poe 1 and 2.
/// </remarks>
public sealed class PoeNinjaService
{
    private static IServiceProvider _serviceProvider;
    private static bool IsPoe2 => _serviceProvider.GetRequiredService<DataManagerService>()
        .Config.Options.GameVersion is 1;

    internal static string League { get; set; }
    internal static bool IsPoe2Cache { get; set; }

    // poe1
    internal static List<NinjaItem> ItemsOne { get; set; } = new();
    internal static List<NinjaExchange> ExchangeOne { get; set; } = new();

    // poe2
    internal static List<NinjaExchange> ExchangeTwo { get; set; } = new();

    internal NinjaState NinjaState { get; private set; }

    public PoeNinjaService(IServiceProvider service)
    {
        _serviceProvider = service;
    }

    internal async Task<T> GetNinjaItem<T>(NinjaInfo ninjaInfo) where T : class, new()
    {
        return await GetNinjaItem<T>(ninjaInfo.League, ninjaInfo.Type, ninjaInfo.Url);
    }

    internal async Task<T> GetNinjaItem<T>(NinjaInfoTwo ninjaInfoTwo) where T : class, new()
    {
        return await GetNinjaItem<T>(ninjaInfoTwo.League, ninjaInfoTwo.Type, ninjaInfoTwo.Url);
    }

    internal async Task<T> GetNinjaItem<T>(NinjaInfoExchange ninjaInfoExchange) where T : class, new()
    {
        return await GetNinjaItem<T>(ninjaInfoExchange.League, ninjaInfoExchange.Type, ninjaInfoExchange.Url);
    }

    internal async Task<T> GetNinjaItem<T>(string league, string type, string url) where T : class, new()
    {
        try
        {
            var cachedItem = GetCachedItem<T>(league, type);
            if (cachedItem is null)
                return null;

            if (!cachedItem.IsCacheValid())
            {
                string sResult = await FetchNinjaData(url);

                if (string.IsNullOrEmpty(sResult))
                    return null;

                var dm = _serviceProvider.GetRequiredService<DataManagerService>();
                var json = dm.Json.Deserialize<T>(sResult);
                cachedItem.SetJson(json);
            }
            return cachedItem.GetJson();
        }
#if DEBUG
        catch (Exception ex)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<PoeNinjaService>>();
            logger.LogInformation("Exception raised : {Message}", ex.Message);
        }
#else
        catch (Exception)
        {
        }
#endif
        return null;
    }

    internal async Task LoadStateAsync()
    {
        try
        {
            var net = _serviceProvider.GetRequiredService<NetService>();
            var result = await net.SendHTTP(Strings.ApiNinjaLeague, Client.Ninja);
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();
            var ninjaState = dm.Json.Deserialize<NinjaState>(result);
            NinjaState = ninjaState ?? GenerateCustomState();
        }
        catch (Exception ex)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Can not load leagues list from poe.ninja", MessageStatus.Information);
            NinjaState ??= GenerateCustomState();
        }
    }

    internal async Task<NinjaDetail> GetCurrencyHistory(NinjaInfoBase infoBase)
    {
        try
        {
            var net = _serviceProvider.GetRequiredService<NetService>();
            var result = await net.SendHTTP(infoBase.UrlDetails, Client.Ninja);
            var dm = _serviceProvider.GetRequiredService<DataManagerService>();
            return dm.Json.Deserialize<NinjaDetail>(result);
        }
        catch (Exception ex)
        {
            var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
            ms.Show(ex.GetFormated(), "Can not load currency history from poe.ninja", MessageStatus.Information);
        }
        return null;
    }

    private NinjaState GenerateCustomState()
    {
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        if (dm.League is null || dm.League.Result is null)
        {
            return null;
        }
        string leagueKind = dm.League.Result[0].Id;
        var eventLeague = dm.League.Result.FirstOrDefault(x => x.Text.Contain('(')
            && x.Text.Contain(')') && x.Text.Contain("00")) is not null;
        NinjaState state = new()
        {
            Leagues = [
                new() { Name = leagueKind, DisplayName = leagueKind, Url = leagueKind.ToLowerInvariant(), Hardcore = false, Indexed = true },
                new() { Name = "Hardcore " + leagueKind, DisplayName = "Hardcore " + leagueKind, Url = leagueKind.ToLowerInvariant() + "hc", Hardcore = true, Indexed = false },
                new() { Name = "Standard", DisplayName = "Standard", Url = "standard", Hardcore = false, Indexed = false },
                new() { Name = "Hardcore", DisplayName = "Hardcore", Url = "hardcore", Hardcore = true, Indexed = false }
            ]
        };
        if (eventLeague)
        {
            state = new()
            {
                Leagues = [..state.Leagues,
                    new() { Name = "Event", DisplayName = "Event", Url = "event", Hardcore = false, Indexed = false },
                    new() { Name = "EventHC", DisplayName = "EventHC", Url = "eventhc", Hardcore = true, Indexed = false }
                ]
            };
        }
        return state;
    }

    private static ICachedNinjaItem<T> GetCachedItem<T>(string league, string type) where T : class, new()
    {
        CheckInitLeague(league);
        CheckInitNinjaLists();

        return GetItemsFor<T>().Cast<ICachedNinjaItem<T>>().FirstOrDefault(x => x.Name == type);
    }

    private static IEnumerable GetItemsFor<T>()
    {
        var type = typeof(T);

        if (type == typeof(NinjaItemContract))
            return ItemsOne;

        if (type == typeof(NinjaExchangeContract))
        {
            return IsPoe2 ? ExchangeTwo : ExchangeOne;
        }

        return Enumerable.Empty<object>();
    }

    private static async Task<string> FetchNinjaData(string url)
        => await _serviceProvider.GetRequiredService<NetService>().SendHTTP(url, Client.Ninja);

    private static void CheckInitNinjaLists()
    {
        if (IsPoe2Cache)
        {
            InitPoe2Lists();
            ClearPoe1List();
            return;
        }
        InitPoe1Lists();
        ClearPoe2List();
    }

    private static void InitPoe2Lists()
    {
        if (ExchangeTwo.Count > 0)
            return;

        ExchangeTwo.AddRange(
            new(Strings.NinjaTypeTwo.Currency), new(Strings.NinjaTypeTwo.Fragments),
            new(Strings.NinjaTypeTwo.Abyss), new(Strings.NinjaTypeTwo.UncutGems),
            new(Strings.NinjaTypeTwo.LineageSupportGems), new(Strings.NinjaTypeTwo.Essences),
            new(Strings.NinjaTypeTwo.Ultimatum), //new(Strings.NinjaTypeTwo.Talismans),
            new(Strings.NinjaTypeTwo.Idols), new(Strings.NinjaTypeTwo.Runes), 
            new(Strings.NinjaTypeTwo.Ritual), new(Strings.NinjaTypeTwo.Expedition), 
            new(Strings.NinjaTypeTwo.Delirium), new(Strings.NinjaTypeTwo.Breach)
        );
    }

    private static void InitPoe1Lists()
    {
        if (ItemsOne.Count is 0)
        {
            ItemsOne.AddRange(
                new(Strings.NinjaTypeOne.Incubator), new(Strings.NinjaTypeOne.Wombgift),
                new(Strings.NinjaTypeOne.UniqueWeapon), new(Strings.NinjaTypeOne.UniqueArmour), 
                new(Strings.NinjaTypeOne.UniqueAccessory),new(Strings.NinjaTypeOne.UniqueFlask), 
                new(Strings.NinjaTypeOne.UniqueJewel), new(Strings.NinjaTypeOne.ForbiddenJewel), 
                new(Strings.NinjaTypeOne.UniqueTincture), new(Strings.NinjaTypeOne.UniqueRelic), 
                new(Strings.NinjaTypeOne.SkillGem), new(Strings.NinjaTypeOne.ClusterJewel), 
                new(Strings.NinjaTypeOne.Map), new(Strings.NinjaTypeOne.BlightedMap), 
                new(Strings.NinjaTypeOne.BlightRavagedMap), new(Strings.NinjaTypeOne.UniqueMap), 
                new(Strings.NinjaTypeOne.DeliriumOrb), new(Strings.NinjaTypeOne.Invitation), 
                new(Strings.NinjaTypeOne.IncursionTemple), new(Strings.NinjaTypeOne.BaseType), 
                new(Strings.NinjaTypeOne.Beast), new(Strings.NinjaTypeOne.Vial)
            );
        }

        if (ExchangeOne.Count is 0)
        {
            ExchangeOne.AddRange(
                new(Strings.NinjaTypeOne.Currency), new(Strings.NinjaTypeOne.Fragment),
                new(Strings.NinjaTypeOne.Oil), new(Strings.NinjaTypeOne.Incubator),
                new(Strings.NinjaTypeOne.Scarab), new(Strings.NinjaTypeOne.DeliriumOrb), 
                new(Strings.NinjaTypeOne.Fossil), new(Strings.NinjaTypeOne.Resonator),
                new(Strings.NinjaTypeOne.Essence), new(Strings.NinjaTypeOne.DivinationCard),
                new(Strings.NinjaTypeOne.Omen), new(Strings.NinjaTypeOne.Tattoo),
                new(Strings.NinjaTypeOne.AllflameEmber), new(Strings.NinjaTypeOne.Runegraft),
                new(Strings.NinjaTypeOne.Artifact)
            );
        }
    }

    private static void ClearPoe1List()
    {
        ItemsOne.Clear();
        ExchangeOne.Clear();
    }

    private static void ClearPoe2List() => ExchangeTwo.Clear();

    private static void CheckInitLeague(string league)
    {
        if (League is null)
        {
            SetLeague(league);
            return;
        }

        bool leagueChanged = League != league;
        bool poeVersionChanged = IsPoe2Cache != IsPoe2;
        if (leagueChanged || poeVersionChanged)
        {
            SetLeague(league);
            ResetCachedItems();
        }
    }

    private static void SetLeague(string league)
    {
        League = league;
        IsPoe2Cache = IsPoe2;
    }

    private static void ResetCachedItems()
    {
        if (IsPoe2Cache)
        {
            foreach (var item in ExchangeTwo)
                item.Creation = DateTime.MinValue;
            return;
        }
        foreach (var item in ItemsOne)
            item.Creation = DateTime.MinValue;

        foreach (var item in ExchangeOne)
            item.Creation = DateTime.MinValue;
    }
}
