using System;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Models.Ninja.Contract.Exchange;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed record class NinjaInfoExchange : NinjaInfoBase
{
    internal string Id { get; private set; }

    internal NinjaInfoExchange(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        var item = _vm.Item;
        Id = item.Id;
        League = _vm.Form.League[_vm.Form.LeagueIndex];
        Type = GetType(item);
        var urlSuffix = League.Replace(" ", "+") + "&type=" + Type;
        Url = Strings.ApiNinjaExchangeOverview + urlSuffix;
        UrlDetails = Strings.ApiNinjaExchangeDetails + urlSuffix + "&id=" + Normalize(item.TypeEn);
        Link = GetLink() + "/" + Normalize(item.TypeEn);
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
            : item.IdCurrency is Strings.CurrencyTypePoe1.Keepers ? Strings.NinjaTypeOne.Currency
            : item.IdCurrency is Strings.CurrencyTypePoe1.DjinnCoins ? Strings.NinjaTypeOne.DjinnCoin
            : item.IdCurrency is Strings.CurrencyTypePoe1.Fragments ?
            item.Id.Contain("scarab") ? Strings.NinjaTypeOne.Scarab :
            item.Id.Contain("astrolabe") ? Strings.NinjaTypeOne.Astrolabe : Strings.NinjaTypeOne.Fragment
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
            : item.IdCurrency is Strings.CurrencyTypePoe1.Ducats ? Strings.NinjaTypeOne.Ducat
            : item.IdCurrency is Strings.CurrencyTypePoe1.EnshroudingCrystals ? Strings.NinjaTypeOne.EnshroudingCrystal
            : string.Empty;
        }
        return string.Empty;
    }

    private string GetLink()
    {
        var leagueKind = _dm.League.Result[0].Id.ToLowerInvariant();
        var ninjaLeague = "standard/";

        var leagueSelect = _dm.League.Result.FirstOrDefault(x => x.Id == League);
        if (leagueSelect is not null)
        {
            var leagueUrl = _ninja.GetLeagueUrl(leagueSelect.Id);
            if (leagueUrl.Length > 0)
            {
                leagueKind = leagueUrl;
                ninjaLeague = leagueUrl + "/";
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
            : Type is Strings.NinjaTypeOne.DjinnCoin ? "djinn-coins"
            : Type is Strings.NinjaTypeOne.Astrolabe ? "astrolabes"
            : Type is Strings.NinjaTypeOne.Ducat ? "ducats"
            : Type is Strings.NinjaTypeOne.EnshroudingCrystal ? "enshrouding-crystals"
            : string.Empty;
    }

    internal override async Task<NinjaValue> GetNinjaValueAsync()
    {
        var jsonItem = await _ninja.GetNinjaItem<NinjaExchangeContract>(this);
        if (jsonItem is null)
        {
            return null;
        }
        var line = jsonItem.Line.FirstOrDefault(x => x.Id == Id);
        if (line is null)
        {
            return null;
        }

        var divinePrice = jsonItem.Core.Primary is "divine" ? line.PrimaryValue : 0;
        var isDivinePrimary = divinePrice > 0;
        var chaosPrice = jsonItem.Core.Primary is "chaos" ? line.PrimaryValue : 0;
        var isChaosPrimary = chaosPrice > 0;

        if (isDivinePrimary)
        {
            chaosPrice = divinePrice * jsonItem.Core.Rates.Chaos.Value;
        }
        if (isChaosPrimary)
        {
            divinePrice = chaosPrice * jsonItem.Core.Rates.Divine.Value;
        }

        var jsonDetail = await _ninja.GetCurrencyHistory(this);
        if (jsonDetail is not null)
        {
            return new()
            {
                Id = line.Id,
                Name = jsonItem.Items.FirstOrDefault(x => x.Id == line.Id)?.Name,
                ChaosPrice = chaosPrice,
                DivinePrice = divinePrice,
                Detail = jsonDetail
            };
        }

        return null;
    }
}
