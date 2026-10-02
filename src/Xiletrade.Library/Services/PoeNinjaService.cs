using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

    // poe1
    private readonly List<NinjaItem> _itemsOne = new();
    private readonly List<NinjaExchange> _exchangeOne = new();

    // poe2
    private readonly List<NinjaItemTwo> _itemsTwo = new();
    private readonly List<NinjaExchange> _exchangeTwo = new();

    private string _league;
    private bool _isPoe2Cache;
    private bool IsPoe2 => _dm.Config.Options.GameVersion is 1;

    private readonly Lock _initLock = new();
    private volatile NinjaState _ninjaState;

    /// <summary>
    /// Initialized upon the first read (blocking)
    /// </summary>   
    private NinjaState NinjaState
    {
        get
        {
            var current = _ninjaState;
            if (current is not null)
                return current;

            Exception error = null;

            lock (_initLock)
            {
                if (_ninjaState is null)
                {
                    (current, error) = Task.Run(LoadStateAsync).GetAwaiter().GetResult();
                    _ninjaState = current;
                }
                current = _ninjaState!;
            }

            if (error is not null)
                ShowInitError(error);

            return current;
        }
        set => _ninjaState = value;
    }

    public PoeNinjaService(ILogger<PoeNinjaService> logger, DataManagerService dm,
        IMessageAdapterService message, INetService net)
    {
        _logger = logger;
        _dm = dm;
        _message = message;
        _net = net;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    private void ShowInitError(Exception ex) =>
        _message.Show(ex.GetFormated(), Resources.Resources.Error030_XNinjaLeague, MessageStatus.Information);

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
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Exception raised : {Message}", ex.Message);
        }
        return null;
    }

    private async Task<(NinjaState State, Exception Error)> LoadStateAsync()
    {
        try
        {
            var result = await _net.SendHTTP(Strings.Api.NinjaLeague, Client.Ninja);
            var ninjaState = _dm.Json.Deserialize<NinjaState>(result);
            return (ninjaState ?? GenerateCustomState(), null);
        }
        catch (Exception ex)
        {
            return (GenerateCustomState(), ex);
        }
    }

    internal async Task InitLeaguesAsync()
    {
        var (state, error) = await LoadStateAsync();
        NinjaState = state;
        if (error is not null)
            ShowInitError(error);
    }

    internal string GetLeagueUrl(ReadOnlySpan<char> leagueName)
    {
        var state = NinjaState;
        if (state is null || state.Leagues is null)
            return string.Empty;

        foreach (var league in state.Leagues)
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
            _message.Show(ex.GetFormated(), Resources.Resources.Error029_XCurrencyHistory, MessageStatus.Information);
        }
        return null;
    }

    internal string GetIcon(ReadOnlySpan<char> name)
    {
        if (IsPoe2)
        {
            foreach (var items in _itemsTwo)
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

        foreach (var items in _itemsOne)
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

    private ICachedNinja<T> GetCachedItem<T>(bool isPoe2, string league, string type) where T : class, new()
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

    private IEnumerable GetItemsFor<T>()
    {
        var type = typeof(T);

        if (type == typeof(NinjaItemContract))
            return _itemsOne;

        if (type == typeof(NinjaItemTwoContract))
            return _itemsTwo;

        if (type == typeof(NinjaExchangeContract))
        {
            return _isPoe2Cache ? _exchangeTwo : _exchangeOne;
        }

        return Array.Empty<object>();
    }

    private async Task<string> FetchNinjaData(string url) => await _net.SendHTTP(url, Client.Ninja);

    private void CheckInitNinjaLists()
    {
        ClearOppositeLists();
        InitLists();
    }

    private void InitLists()
    {
        if (_isPoe2Cache)
        {
            if (_itemsTwo.Count is 0)
            {
                foreach (var item in Strings.NinjaTypeTwo.ItemNames)
                {
                    _itemsTwo.Add(new(item));
                }
            }

            if (_exchangeTwo.Count is 0)
            {
                foreach (var exchange in Strings.NinjaTypeTwo.ExchangeNames)
                {
                    _exchangeTwo.Add(new(exchange));
                }
            }
            return;
        }

        if (_itemsOne.Count is 0)
        {
            foreach (var item in Strings.NinjaTypeOne.ItemNames)
            {
                _itemsOne.Add(new(item));
            }
        }

        if (_exchangeOne.Count is 0)
        {
            foreach (var exchange in Strings.NinjaTypeOne.ExchangeNames)
            {
                _exchangeOne.Add(new(exchange));
            }
        }
    }

    private void ClearOppositeLists()
    {
        if (_isPoe2Cache)
        {
            Clear(_itemsOne, _exchangeOne);
            return;
        }
        Clear(_itemsTwo, _exchangeTwo);
    }

    private static void Clear(params IList[] lists)
    {
        foreach (var list in lists)
            list.Clear();
    }

    private void CheckInitLeague(bool isPoe2, string league)
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

    private void SetLeague(bool isPoe2, string league)
    {
        _league = league;
        _isPoe2Cache = isPoe2;
    }

    private void ResetCachedItems()
    {
        foreach (var item in GetCurrentCacheItems())
            item.Creation = DateTime.MinValue;
    }

    private IEnumerable<ICachedNinjaItem> GetCurrentCacheItems()
    {
        if (_isPoe2Cache)
        {
            foreach (var item in _itemsTwo)
                yield return item;

            foreach (var exchange in _exchangeTwo)
                yield return exchange;
        }
        else
        {
            foreach (var item in _itemsOne)
                yield return item;

            foreach (var exchange in _exchangeOne)
                yield return exchange;
        }
    }
}
