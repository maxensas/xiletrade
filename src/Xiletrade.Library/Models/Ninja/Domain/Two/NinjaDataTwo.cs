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
        Items.AddRange(new(Strings.NinjaTypeTwo.Currency), new(Strings.NinjaTypeTwo.Fragments)
            , new(Strings.NinjaTypeTwo.Expedition), new(Strings.NinjaTypeTwo.Essences)
            , new(Strings.NinjaTypeTwo.Talismans), new(Strings.NinjaTypeTwo.Runes)
            , new(Strings.NinjaTypeTwo.LineageSupportGems), new(Strings.NinjaTypeTwo.UncutGems)
            , new(Strings.NinjaTypeTwo.Abyss), new(Strings.NinjaTypeTwo.Delirium)
            , new(Strings.NinjaTypeTwo.Ultimatum), new(Strings.NinjaTypeTwo.Breach)
            , new(Strings.NinjaTypeTwo.Ritual));
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
