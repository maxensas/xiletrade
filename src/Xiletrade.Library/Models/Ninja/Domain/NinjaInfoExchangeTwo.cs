using System;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Models.Ninja.Contract.Exchange;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed record NinjaInfoExchangeTwo : NinjaInfoBase
{
    internal string Id { get; private set; }

    internal NinjaInfoExchangeTwo(IServiceProvider serviceProvider) : base(serviceProvider)
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
        if (item.IdCurrency.Length is 0)
        {
            return string.Empty;
        }
        
        return item.IdCurrency is Strings.CurrencyTypePoe2.Currency ? Strings.NinjaTypeTwo.Currency
                : item.IdCurrency is Strings.CurrencyTypePoe2.Vaal ?
                    item.Id.EndWith("thesis") || item.Id.Contain("soul-core") ? Strings.NinjaTypeTwo.SoulCores 
                    : Strings.NinjaTypeTwo.Currency
                : item.IdCurrency is Strings.CurrencyTypePoe2.UncutGems ? Strings.NinjaTypeTwo.UncutGems
                : item.IdCurrency is Strings.CurrencyTypePoe2.Runes ? Strings.NinjaTypeTwo.Runes
                : item.IdCurrency is Strings.CurrencyTypePoe2.Fragments ? Strings.NinjaTypeTwo.Fragments
                : item.IdCurrency is Strings.CurrencyTypePoe2.Expedition ? Strings.NinjaTypeTwo.Expedition
                : item.IdCurrency is Strings.CurrencyTypePoe2.Essences ? Strings.NinjaTypeTwo.Essences
                : item.IdCurrency is Strings.CurrencyTypePoe2.Talismans ? Strings.NinjaTypeTwo.Talismans
                //: item.IdCurrency is Strings.CurrencyTypePoe2.Idol ? Strings.NinjaTypeTwo.Idols
                : item.IdCurrency is Strings.CurrencyTypePoe2.Abyss ? Strings.NinjaTypeTwo.Abyss // Abyssal Bones
                : item.IdCurrency is Strings.CurrencyTypePoe2.Delirium ?
                    item.Id.Contain("simulacrum") ? Strings.NinjaTypeTwo.Fragments
                    : Strings.NinjaTypeTwo.Delirium // Distilled Emotions
                : item.IdCurrency is Strings.CurrencyTypePoe2.Ultimatum ? Strings.NinjaTypeTwo.SoulCores // Soul Cores
                : item.IdCurrency is Strings.CurrencyTypePoe2.Breach ? Strings.NinjaTypeTwo.Breach // Catalysts
                : item.IdCurrency is Strings.CurrencyTypePoe2.Ritual ?
                    item.Id.Contain("idol") ? Strings.NinjaTypeTwo.Idols 
                    : item.Id.Equal("call-of-the-shadows") ? Strings.NinjaTypeTwo.Fragments
                    : item.Id.Equal("raven-touched-shard") ? Strings.NinjaTypeTwo.SoulCores
                    : Strings.NinjaTypeTwo.Ritual // Omens
                : item.IdCurrency is Strings.CurrencyTypePoe2.LineageSupportGems ? Strings.NinjaTypeTwo.LineageSupportGems
                : item.IdCurrency is Strings.CurrencyTypePoe2.Verisium ? Strings.NinjaTypeTwo.Verisium
                : string.Empty;
    }

    private string GetLink()
    {
        var ninjaLeague = "standard/";

        var leagueSelect = _dm.League.Result.FirstOrDefault(x => x.Id == League);
        if (leagueSelect is not null)
        {
            var leagueUrl = _ninja.GetLeagueUrl(leagueSelect.Id);
            if (leagueUrl.Length > 0)
            {
                ninjaLeague = leagueUrl + "/";
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
            : Type is Strings.NinjaTypeTwo.SoulCores ? "soul-cores"
            : Type is Strings.NinjaTypeTwo.Breach ? "breach-catalyst"
            : Type is Strings.NinjaTypeTwo.Ritual ? "omens"
            : Type is Strings.NinjaTypeTwo.LineageSupportGems ? "lineage-support-gems"
            : Type is Strings.NinjaTypeTwo.Idols ? "idols"
            : Type is Strings.NinjaTypeTwo.Verisium ? "verisium"
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
        var exaltedPrice = jsonItem.Core.Primary is "exalted" ? line.PrimaryValue : 0;
        var isExaltedPrimary = exaltedPrice > 0;
        if (isDivinePrimary)
        {
            chaosPrice = divinePrice * jsonItem.Core.Rates.Chaos.Value;
            exaltedPrice = divinePrice * jsonItem.Core.Rates.Exalted.Value;
        }
        if (isChaosPrimary)
        {
            divinePrice = chaosPrice * jsonItem.Core.Rates.Divine.Value;
            exaltedPrice = chaosPrice * jsonItem.Core.Rates.Exalted.Value;
        }
        if (isExaltedPrimary)
        {
            divinePrice = exaltedPrice * jsonItem.Core.Rates.Divine.Value;
            chaosPrice = exaltedPrice * jsonItem.Core.Rates.Chaos.Value;
        }

        var jsonDetail = await _ninja.GetCurrencyHistory(this);
        if (jsonDetail is not null)
        {
            return new()
            {
                Id = line.Id,
                Name = jsonItem.Items.FirstOrDefault(x => x.Id == line.Id)?.Name,
                ChaosPrice = chaosPrice,
                ExaltPrice = exaltedPrice,
                DivinePrice = divinePrice,
                Detail = jsonDetail
            };
        }

        return null;
    }
}
