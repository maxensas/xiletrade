using System.Linq;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain.Two;

internal sealed record NinjaInfoTwo : NinjaInfoBase
{
    internal string Id { get; private set; }

    internal NinjaInfoTwo(DataManagerService dm, PoeNinjaService ninja, string league, ItemData item) : base(dm, ninja)
    {
        Id = item.Id;
        League = league;
        Type = GetType(item);
        Url = Strings.ApiNinjaTwo + League + "&overviewName=" + Type;
        Link = GetLink();
        VerifiedLink = League.Length > 0 && Type.Length > 0;
    }

    private static string GetType(ItemData item)
    {
        if (item.IdCurrency.Length > 0)
        {
            return item.IdCurrency is Strings.CurrencyTypePoe2.Currency ? Strings.NinjaTypeTwo.Currency
            : item.IdCurrency is Strings.CurrencyTypePoe2.UncutGems ? Strings.NinjaTypeTwo.UncutGems
            : item.IdCurrency is Strings.CurrencyTypePoe2.Runes ? Strings.NinjaTypeTwo.Runes
            : item.IdCurrency is Strings.CurrencyTypePoe2.Fragments ? Strings.NinjaTypeTwo.Fragments
            : item.IdCurrency is Strings.CurrencyTypePoe2.Expedition ? Strings.NinjaTypeTwo.Expedition
            : item.IdCurrency is Strings.CurrencyTypePoe2.Essences ? Strings.NinjaTypeTwo.Essences
            : item.IdCurrency is Strings.CurrencyTypePoe2.Talismans ? Strings.NinjaTypeTwo.Talismans
            : item.IdCurrency is Strings.CurrencyTypePoe2.Abyss ? Strings.NinjaTypeTwo.Abyss // Abyssal Bones
            : item.IdCurrency is Strings.CurrencyTypePoe2.Delirium ? Strings.NinjaTypeTwo.Delirium // Distilled Emotions
            : item.IdCurrency is Strings.CurrencyTypePoe2.Ultimatum ? Strings.NinjaTypeTwo.Ultimatum // Soul Cores
            : item.IdCurrency is Strings.CurrencyTypePoe2.Breach ? Strings.NinjaTypeTwo.Breach // Catalysts
            : item.IdCurrency is Strings.CurrencyTypePoe2.Ritual ? Strings.NinjaTypeTwo.Ritual // Omens
            : string.Empty;
        }
        return item.Flag.SupportGems ? Strings.NinjaTypeTwo.LineageSupportGems : string.Empty;
    }

    private string GetLink()
    {
        var leagueKind = _dm.League.Result[0].Id.ToLowerInvariant();
        var ninjaLeague = "standard/";

        var leagueSelect = _dm.League.Result.FirstOrDefault(x => x.Text == League);
        if (leagueSelect is not null)
        {
            var league = _ninja.NinjaState.Leagues.Where(x => x.Name == leagueSelect.Text).FirstOrDefault();
            if (league is not null)
            {
                leagueKind = league.Url;
                ninjaLeague = league.Url + "/";
            }
        }

        return Strings.UrlPoeNinja + ninjaLeague + GetWebCategory();
    }

    private string GetWebCategory()
    {
        return Type is Strings.NinjaTypeTwo.Currency ? "currency"
            : Type is Strings.NinjaTypeTwo.UncutGems ? "uncut-gems"
            : Type is Strings.NinjaTypeTwo.Runes ? "runes"
            : Type is Strings.NinjaTypeTwo.Fragments ? "fragments"
            : Type is Strings.NinjaTypeTwo.Expedition ? "expedition"
            : Type is Strings.NinjaTypeTwo.Essences ? "essences"
            : Type is Strings.NinjaTypeTwo.Talismans ? "talismans"
            : Type is Strings.NinjaTypeTwo.Abyss ? "abyssal-bones"
            : Type is Strings.NinjaTypeTwo.Delirium ? "distilled-emotions"
            : Type is Strings.NinjaTypeTwo.Ultimatum ? "soul-cores"
            : Type is Strings.NinjaTypeTwo.Breach ? "breach-catalyst"
            : Type is Strings.NinjaTypeTwo.Ritual ? "omens"
            : Type is Strings.NinjaTypeTwo.LineageSupportGems ? "lineage-support-gems"
            : string.Empty;
    }
}
