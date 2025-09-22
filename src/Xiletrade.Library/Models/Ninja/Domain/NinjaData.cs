using System;
using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal static class NinjaData
{
    internal static string League { get; set; }
    
    internal static List<NinjaCurrency> Currencys { get; set; } = new();
    internal static List<NinjaItem> Items { get; set; } = new();

    internal static object GetItem(string league, string type)
    {
        CheckLeague(league);
        CheckInitLists();

        foreach (var cur in Currencys)
        {
            if (cur.Name == type)
            {
                return cur;
            }
        }
        foreach (var item in Items)
        {
            if (item.Name == type)
            {
                return item;
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
