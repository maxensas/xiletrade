using System;
using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain.Two;

//TODO create service
internal sealed class NinjaDataTwo
{
    internal string League { get; set; }

    internal List<NinjaItemTwo> Items { get; set; } = new();

    internal NinjaDataTwo()
    {
        Items.AddRange(new(Strings.NinjaTwo.Currency), new(Strings.NinjaTwo.Fragments)
            , new(Strings.NinjaTwo.Expedition), new(Strings.NinjaTwo.Essences)
            , new(Strings.NinjaTwo.Talismans), new(Strings.NinjaTwo.Runes)
            , new(Strings.NinjaTwo.LineageSupportGems), new(Strings.NinjaTwo.UncutGems)
            , new(Strings.NinjaTwo.Abyss), new(Strings.NinjaTwo.Delirium)
            , new(Strings.NinjaTwo.Ultimatum), new(Strings.NinjaTwo.Breach)
            , new(Strings.NinjaTwo.Ritual));
    }

    //TODO
    internal string GetUrl()
    {
        // get Strings.ApiNinjaLeagueTwo
        return Strings.ApiNinjaTwo + "leaguetodo" + "&overviewName=" + Items[0].Name;
    }

    internal void CheckLeague(string league)
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
            foreach (var item in Items)
            {
                item.Creation = DateTime.MinValue;
            }
        }
    }
}
