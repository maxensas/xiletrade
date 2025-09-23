using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xiletrade.Library.Models.Ninja.Domain;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

/// <summary>
/// Service used to manage cache data for poe ninja.
/// </summary>
public sealed class PoeNinjaService
{
    private static IServiceProvider _serviceProvider;

    internal static string League { get; set; }
    internal static List<NinjaCurrency> Currencys { get; set; } = new();
    internal static List<NinjaItem> Items { get; set; } = new();

    public PoeNinjaService(IServiceProvider service)
    {
        _serviceProvider = service;
    }

    public T GetNinjaItem<T>(string league, string type, string url) where T : class, new()
    {
        try
        {
            var cachedItem = GetCachedItem<T>(league, type);
            if (cachedItem is null)
                return default;

            if (cachedItem.CheckValidity())
            {
                var service = _serviceProvider.GetRequiredService<NetService>();
                string sResult = service.SendHTTP(null, url, Client.Ninja).Result;

                if (string.IsNullOrEmpty(sResult))
                    return default;

                cachedItem.DeserializeAndSetJson(sResult);
            }

            return cachedItem.GetJson();
        }
        catch (Exception)
        {
            // unmanaged exception : Xiletrade must remain independent of poe.ninja 

            // TODO : Add info label on the UI to see something is going wrong with ninja
        }
        return default;
    }

    private static ICachedNinjaItem<T> GetCachedItem<T>(string league, string type) where T : class, new()
    {
        CheckLeague(league);
        CheckInitLists();

        foreach (var cur in Currencys)
        {
            if (cur.Name == type)
            {
                return cur as ICachedNinjaItem<T>;
            }
        }
        foreach (var item in Items)
        {
            if (item.Name == type)
            {
                return item as ICachedNinjaItem<T>;
            }
        }
        return null;
    }

    private static void CheckInitLists()
    {
        if (Currencys.Count is 0)
        {
            Currencys.AddRange(new(Strings.NinjaTypeOne.Currency), new(Strings.NinjaTypeOne.Fragment));
        }

        if (Items.Count is 0)
        {
            Items.AddRange(new(Strings.NinjaTypeOne.Oil), new(Strings.NinjaTypeOne.Incubator)
                , new(Strings.NinjaTypeOne.Invitation), new(Strings.NinjaTypeOne.Scarab)
                , new(Strings.NinjaTypeOne.Fossil), new(Strings.NinjaTypeOne.Resonator)
                , new(Strings.NinjaTypeOne.Essence), new(Strings.NinjaTypeOne.DivinationCard)
                , new(Strings.NinjaTypeOne.Prophecy), new(Strings.NinjaTypeOne.SkillGem)
                , new(Strings.NinjaTypeOne.BaseType), new(Strings.NinjaTypeOne.UniqueMap)
                , new(Strings.NinjaTypeOne.Map), new(Strings.NinjaTypeOne.BlightedMap)
                , new(Strings.NinjaTypeOne.BlightRavagedMap), new(Strings.NinjaTypeOne.ScourgedMap)
                , new(Strings.NinjaTypeOne.UniqueJewel), new(Strings.NinjaTypeOne.UniqueFlask)
                , new(Strings.NinjaTypeOne.UniqueWeapon), new(Strings.NinjaTypeOne.UniqueArmour)
                , new(Strings.NinjaTypeOne.UniqueAccessory), new(Strings.NinjaTypeOne.Beast)
                , new(Strings.NinjaTypeOne.DeliriumOrb), new(Strings.NinjaTypeOne.Vial)
                , new(Strings.NinjaTypeOne.Watchstone), new(Strings.NinjaTypeOne.ClusterJewel)
                , new(Strings.NinjaTypeOne.Omen), new(Strings.NinjaTypeOne.Tattoo)
                , new(Strings.NinjaTypeOne.UniqueRelic), new(Strings.NinjaTypeOne.Coffin)
                , new(Strings.NinjaTypeOne.AllflameEmber), new(Strings.NinjaTypeOne.Runegraft)
                , new(Strings.NinjaTypeOne.Memory), new(Strings.NinjaTypeOne.Artifact));
        }
    }

    private static void CheckLeague(string league)
    {
        if (League is null)
        {
            League = league;
            return;
        }
        if (League.Length > 0 && League != league)
        {
            League = league;

            // reset
            foreach (var cur in Currencys)
            {
                cur.Creation = DateTime.MinValue;
            }
            foreach (var item in Items)
            {
                item.Creation = DateTime.MinValue;
            }
        }
    }
}
