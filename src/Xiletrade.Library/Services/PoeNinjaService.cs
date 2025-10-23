using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Models.Ninja.Contract.Two;
using Xiletrade.Library.Models.Ninja.Domain;
using Xiletrade.Library.Models.Ninja.Domain.Two;
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

    private static readonly Dictionary<Type, Func<IEnumerable>> _itemProviders = new()
    {
        { typeof(NinjaCurrencyContract), () => Currencys },
        { typeof(NinjaItemContract), () => Items },
        { typeof(NinjaItemTwoContract), () => ItemsTwo }
    };

    internal static string League { get; set; }
    internal static bool IsPoe2Cache { get; set; }

    // poe1
    internal static List<NinjaCurrency> Currencys { get; set; } = new();
    internal static List<NinjaItem> Items { get; set; } = new();

    // poe2
    internal static List<NinjaItemTwo> ItemsTwo { get; set; } = new();

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
        catch (Exception)
        {
            // unmanaged exception : Xiletrade must remain independent of poe.ninja 
            // TODO : Add info label on the UI to see something is going wrong with ninja
        }
        return null;
    }

    private static ICachedNinjaItem<T> GetCachedItem<T>(string league, string type) where T : class, new()
    {
        CheckInitLeague(league);
        CheckInitNinjaLists();

        if (_itemProviders.TryGetValue(typeof(T), out var provider))
            return provider().Cast<ICachedNinjaItem<T>>().FirstOrDefault(x => x.Name == type);

        return null;
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
        if (ItemsTwo.Count > 0)
            return;

        ItemsTwo.AddRange(
            new(Strings.NinjaTypeTwo.Currency), new(Strings.NinjaTypeTwo.Fragments),
            new(Strings.NinjaTypeTwo.Expedition), new(Strings.NinjaTypeTwo.Essences),
            new(Strings.NinjaTypeTwo.Talismans), new(Strings.NinjaTypeTwo.Runes),
            new(Strings.NinjaTypeTwo.LineageSupportGems), new(Strings.NinjaTypeTwo.UncutGems),
            new(Strings.NinjaTypeTwo.Abyss), new(Strings.NinjaTypeTwo.Delirium),
            new(Strings.NinjaTypeTwo.Ultimatum), new(Strings.NinjaTypeTwo.Breach),
            new(Strings.NinjaTypeTwo.Ritual)
        );
    }

    private static void InitPoe1Lists()
    {
        if (Currencys.Count is 0)
        {
            Currencys.AddRange(
                new(Strings.NinjaTypeOne.Currency), new(Strings.NinjaTypeOne.Fragment)
            );
        }

        if (Items.Count is 0)
        {
            Items.AddRange(
                new(Strings.NinjaTypeOne.Oil), new(Strings.NinjaTypeOne.Incubator),
                new(Strings.NinjaTypeOne.Invitation), new(Strings.NinjaTypeOne.Scarab),
                new(Strings.NinjaTypeOne.Fossil), new(Strings.NinjaTypeOne.Resonator),
                new(Strings.NinjaTypeOne.Essence), new(Strings.NinjaTypeOne.DivinationCard),
                new(Strings.NinjaTypeOne.Prophecy), new(Strings.NinjaTypeOne.SkillGem),
                new(Strings.NinjaTypeOne.BaseType), new(Strings.NinjaTypeOne.UniqueMap),
                new(Strings.NinjaTypeOne.Map), new(Strings.NinjaTypeOne.BlightedMap),
                new(Strings.NinjaTypeOne.BlightRavagedMap), new(Strings.NinjaTypeOne.ScourgedMap),
                new(Strings.NinjaTypeOne.UniqueJewel), new(Strings.NinjaTypeOne.UniqueFlask),
                new(Strings.NinjaTypeOne.UniqueWeapon), new(Strings.NinjaTypeOne.UniqueArmour),
                new(Strings.NinjaTypeOne.UniqueAccessory), new(Strings.NinjaTypeOne.Beast),
                new(Strings.NinjaTypeOne.DeliriumOrb), new(Strings.NinjaTypeOne.Vial),
                new(Strings.NinjaTypeOne.Watchstone), new(Strings.NinjaTypeOne.ClusterJewel),
                new(Strings.NinjaTypeOne.Omen), new(Strings.NinjaTypeOne.Tattoo),
                new(Strings.NinjaTypeOne.UniqueRelic), new(Strings.NinjaTypeOne.Coffin),
                new(Strings.NinjaTypeOne.AllflameEmber), new(Strings.NinjaTypeOne.Runegraft),
                new(Strings.NinjaTypeOne.Memory), new(Strings.NinjaTypeOne.Artifact)
            );
        }
    }

    private static void ClearPoe1List()
    {
        Currencys.Clear();
        Items.Clear();
    }

    private static void ClearPoe2List() => ItemsTwo.Clear();

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
            foreach (var item in ItemsTwo)
                item.Creation = DateTime.MinValue;
            return;
        }

        foreach (var cur in Currencys)
            cur.Creation = DateTime.MinValue;

        foreach (var item in Items)
            item.Creation = DateTime.MinValue;
    }
}
