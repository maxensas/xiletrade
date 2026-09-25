using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
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
    private readonly IMessageAdapterService _message;
    private readonly ILogger<PoeNinjaService> _logger;
    private readonly DataManagerService _dm;
    private readonly INetService _net;

    private bool IsPoe2 => _dm.Config.Options.GameVersion is 1;

    private static string _league;
    private static bool _isPoe2Cache;

    // poe1
    private static List<NinjaItem> ItemsOne { get; set; } = new();
    private static List<NinjaExchange> ExchangeOne { get; set; } = new();

    // poe2
    private static List<NinjaItemTwo> ItemsTwo { get; set; } = new();
    private static List<NinjaExchange> ExchangeTwo { get; set; } = new();

    private NinjaState NinjaState { get; set; }

    // Lazzy initialization of the service.
    public PoeNinjaService(ILogger<PoeNinjaService> logger,
    IMessageAdapterService message, DataManagerService dm, INetService net)
    {
        _logger = logger;
        _message = message;
        _dm = dm;
        _net = net;

        _ = InitLeaguesAsync();

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    internal async Task<T> GetNinjaItem<T>(NinjaInfoBase ninjaInfo) where T : class, new()
    {
        return await GetNinjaItem<T>(ninjaInfo.League, ninjaInfo.Type, ninjaInfo.Url);
    }

    internal async Task<T> GetNinjaItem<T>(string league, string type, string url) where T : class, new()
    {
        try
        {
            var cachedItem = GetCachedItem<T>(IsPoe2, league, type);
            if (cachedItem is null)
                return null;

            if (!cachedItem.IsCacheValid())
            {
                string sResult = await FetchNinjaData(url);

                if (string.IsNullOrEmpty(sResult))
                    return null;

                var json = _dm.Json.Deserialize<T>(sResult);
                cachedItem.SetJson(json);
            }
            return cachedItem.GetJson();
        }
#if DEBUG
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Exception raised : {Message}", ex.Message);
        }
#else
        catch (Exception)
        {
        }
#endif
        return null;
    }

    internal async Task InitLeaguesAsync()
    {
        try
        {
            var result = await _net.SendHTTP(Strings.Api.NinjaLeague, Client.Ninja);
            var ninjaState = _dm.Json.Deserialize<NinjaState>(result);
            NinjaState = ninjaState ?? GenerateCustomState();
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Can not load leagues list from poe.ninja", MessageStatus.Information);
            NinjaState ??= GenerateCustomState();
        }
    }

    internal string GetLeagueUrl(ReadOnlySpan<char> leagueName)
    {
        if (NinjaState is null || NinjaState.Leagues is null)
            return string.Empty;

        foreach (var league in NinjaState.Leagues)
        {
            if (league.Name.AsSpan().SequenceEqual(leagueName))
                return league.Url;
        }
        return string.Empty;
    }

    internal async Task<NinjaDetail> GetCurrencyHistory(NinjaInfoBase infoBase)
    {
        try
        {
            var result = await _net.SendHTTP(infoBase.UrlDetails, Client.Ninja);
            var json = _dm.Json.Deserialize<NinjaDetail>(result);
            if(json.Pairs?.Count > 1)
            {
                json.Pairs.Sort((a, b) => b.VolumePrimaryValue.CompareTo(a.VolumePrimaryValue));
            }
            return json;
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), "Can not load currency history from poe.ninja", MessageStatus.Information);
        }
        return null;
    }

    internal string GetIcon(ReadOnlySpan<char> name)
    {
        if (IsPoe2)
        {
            foreach (var items in ItemsTwo)
            {
                if (items.Json is null || items.Json.Line is null)
                {
                    continue;
                }
                foreach (var line in items.Json.Line)
                {
                    if (line.Name.AsSpan().SequenceEqual(name) && line.Icon.Length > 0)
                    {
                        return line.Icon;
                    }
                }
            }
            return string.Empty;
        }

        foreach (var items in ItemsOne)
        {
            if (items.Json is null || items.Json.Lines is null)
            {
                continue;
            }
            foreach (var line in items.Json.Lines)
            {
                if (line.Name.AsSpan().SequenceEqual(name) && line.Icon.Length > 0)
                {
                    return line.Icon;
                }
            }
        }
        return string.Empty;
    }

    private NinjaState GenerateCustomState()
    {
        var poeLeagueList = _dm.League?.Result;
        if (poeLeagueList is null)
        {
            return null;
        }
        var leagueKind = poeLeagueList[0].Id;
        var ninjaLeagues = new List<NinjaLeagues>
        {
            new() { Name = leagueKind, DisplayName = leagueKind, Url = leagueKind.ToLowerInvariant(), Hardcore = false, Indexed = true },
            new() { Name = $"Hardcore {leagueKind}", DisplayName = $"Hardcore {leagueKind}", Url = $"{leagueKind.ToLowerInvariant()}hc", Hardcore = true, Indexed = false },
            new() { Name = "Standard", DisplayName = "Standard", Url = "standard", Hardcore = false, Indexed = false },
            new() { Name = "Hardcore", DisplayName = "Hardcore", Url = "hardcore", Hardcore = true, Indexed = false }
        };
        if (poeLeagueList.HasEventLeague())
        {
            ninjaLeagues.Add(new() { Name = "Event", DisplayName = "Event", Url = "event", Hardcore = false, Indexed = false });
            ninjaLeagues.Add(new() { Name = "EventHC", DisplayName = "EventHC", Url = "eventhc", Hardcore = true, Indexed = false });
        }
        return new() { Leagues = [.. ninjaLeagues] };
    }

    private static ICachedNinja<T> GetCachedItem<T>(bool isPoe2, string league, string type) where T : class, new()
    {
        CheckInitLeague(isPoe2, league);
        CheckInitNinjaLists();

        foreach (var item in GetItemsFor<T>())
        {
            if (item is ICachedNinja<T> cached && cached.Name == type)
                return cached;
        }

        return null;
    }

    private static IEnumerable GetItemsFor<T>()
    {
        var type = typeof(T);

        if (type == typeof(NinjaItemContract))
            return ItemsOne;

        if (type == typeof(NinjaItemTwoContract))
            return ItemsTwo;

        if (type == typeof(NinjaExchangeContract))
        {
            return _isPoe2Cache ? ExchangeTwo : ExchangeOne;
        }

        return Array.Empty<object>();
    }

    private async Task<string> FetchNinjaData(string url) => await _net.SendHTTP(url, Client.Ninja);

    private static void CheckInitNinjaLists()
    {
        ClearOppositeLists();
        InitLists();
    }

    private static void InitLists()
    {
        if (_isPoe2Cache)
        {
            if (ItemsTwo.Count is 0)
            {
                foreach (var item in Strings.NinjaTypeTwo.ItemNames)
                {
                    ItemsTwo.Add(new(item));
                }
            }

            if (ExchangeTwo.Count is 0)
            {
                foreach (var exchange in Strings.NinjaTypeTwo.ExchangeNames)
                {
                    ExchangeTwo.Add(new(exchange));
                }
            }
            return;
        }

        if (ItemsOne.Count is 0)
        {
            foreach (var item in Strings.NinjaTypeOne.ItemNames)
            {
                ItemsOne.Add(new(item));
            }
        }

        if (ExchangeOne.Count is 0)
        {
            foreach (var exchange in Strings.NinjaTypeOne.ExchangeNames)
            {
                ExchangeOne.Add(new(exchange));
            }
        }
    }

    private static void ClearOppositeLists()
    {
        if (_isPoe2Cache)
        {
            Clear(ItemsOne, ExchangeOne);
            return;
        }
        Clear(ItemsTwo, ExchangeTwo);
    }

    private static void Clear(params IList[] lists)
    {
        foreach (var list in lists)
            list.Clear();
    }

    private static void CheckInitLeague(bool isPoe2, string league)
    {
        if (_league is null)
        {
            SetLeague(isPoe2, league);
            return;
        }

        bool leagueChanged = _league != league;
        bool poeVersionChanged = _isPoe2Cache != isPoe2;
        if (leagueChanged || poeVersionChanged)
        {
            SetLeague(isPoe2, league);
            ResetCachedItems();
        }
    }

    private static void SetLeague(bool isPoe2, string league)
    {
        _league = league;
        _isPoe2Cache = isPoe2;
    }

    private static void ResetCachedItems()
    {
        foreach (var item in GetCurrentCacheItems())
            item.Creation = DateTime.MinValue;
    }

    private static IEnumerable<ICachedNinjaItem> GetCurrentCacheItems()
    {
        if (_isPoe2Cache)
        {
            foreach (var item in ItemsTwo)
                yield return item;

            foreach (var exchange in ExchangeTwo)
                yield return exchange;
        }
        else
        {
            foreach (var item in ItemsOne)
                yield return item;

            foreach (var exchange in ExchangeOne)
                yield return exchange;
        }
    }
}
