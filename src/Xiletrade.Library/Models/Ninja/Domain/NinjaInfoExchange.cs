using System.Linq;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed record class NinjaInfoExchange : NinjaInfoBase
{
    internal string Id { get; private set; }

    internal NinjaInfoExchange(DataManagerService dm, PoeNinjaService ninja, string league, ItemData item) : base(dm, ninja)
    {
        Id = item.Id;
        League = league;
        Type = GetType(item);
        Url = Strings.ApiNinjaExchangeOverview + League + "&type=" + Type;
        UrlDetails = Strings.ApiNinjaExchangeDetails + League + "&type=" + Type + "&id=" + Id;
        Link = GetLink();
        VerifiedLink = League.Length > 0 && Type.Length > 0;
    }

    private static string GetType(ItemData item)
    {
        if (item.IdCurrency.Length > 0)
        {
            return item.IdCurrency is Strings.CurrencyTypePoe1.Currency ? Strings.NinjaTypeOne.Currency
            : item.IdCurrency is Strings.CurrencyTypePoe1.EldritchCurrency ? Strings.NinjaTypeOne.Currency
            : item.IdCurrency is Strings.CurrencyTypePoe1.TaintedCurrency ? Strings.NinjaTypeOne.Currency
            : item.IdCurrency is Strings.CurrencyTypePoe1.Catalysts ? Strings.NinjaTypeOne.Currency
            : item.IdCurrency is Strings.CurrencyTypePoe1.Exotic ? Strings.NinjaTypeOne.Currency
            : item.IdCurrency is Strings.CurrencyTypePoe1.Fragments ?
            item.Id.Contain("scarab") ? Strings.NinjaTypeOne.Scarab : Strings.NinjaTypeOne.Fragment
            : item.IdCurrency is Strings.CurrencyTypePoe1.Runegrafts ? Strings.NinjaTypeOne.Runegraft
            : item.IdCurrency is Strings.CurrencyTypePoe1.AllflameEmbers ? Strings.NinjaTypeOne.AllflameEmber
            : item.IdCurrency is Strings.CurrencyTypePoe1.Ancestor ?
            item.Id.Contain("omen") ? Strings.NinjaTypeOne.Omen : Strings.NinjaTypeOne.Tattoo
            : item.IdCurrency is Strings.CurrencyTypePoe1.Cards ? Strings.NinjaTypeOne.DivinationCard
            : item.IdCurrency is Strings.CurrencyTypePoe1.Expedition ? Strings.NinjaTypeOne.Artifact
            : item.IdCurrency is Strings.CurrencyTypePoe1.Oils ? Strings.NinjaTypeOne.Oil
            : item.IdCurrency is Strings.CurrencyTypePoe1.Incubators ? Strings.NinjaTypeOne.Incubator
            : item.IdCurrency is Strings.CurrencyTypePoe1.DeliriumOrbs ? Strings.NinjaTypeOne.DeliriumOrb
            : item.IdCurrency is Strings.CurrencyTypePoe1.Delve ? 
            item.Id.Contain("fossil") ? Strings.NinjaTypeOne.Fossil : Strings.NinjaTypeOne.Resonator
            : item.IdCurrency is Strings.CurrencyTypePoe1.Essences ? Strings.NinjaTypeOne.Essence
            
            : string.Empty;
        }
        return string.Empty;
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
        return Type is Strings.NinjaTypeOne.Currency ? "currency"
            : Type is Strings.NinjaTypeOne.Fragment ? "fragments"
            : Type is Strings.NinjaTypeOne.Oil ? "oils"
            : Type is Strings.NinjaTypeOne.Incubator ? "incubators"
            : Type is Strings.NinjaTypeOne.Scarab ? "scarabs"
            : Type is Strings.NinjaTypeOne.Fossil ? "fossils"
            : Type is Strings.NinjaTypeOne.Resonator ? "resonators"
            : Type is Strings.NinjaTypeOne.Essence ? "essences"
            : Type is Strings.NinjaTypeOne.DivinationCard ? "divination-cards"
            : Type is Strings.NinjaTypeOne.DeliriumOrb ? "delirium-orbs"
            : Type is Strings.NinjaTypeOne.Omen ? "omens"
            : Type is Strings.NinjaTypeOne.Tattoo ? "tattoos"
            : Type is Strings.NinjaTypeOne.AllflameEmber ? "allflame-ember"
            : Type is Strings.NinjaTypeOne.Runegraft ? "kalguuran-runes"
            : Type is Strings.NinjaTypeOne.Artifact ? "artifact"
            : string.Empty;
    }
}
