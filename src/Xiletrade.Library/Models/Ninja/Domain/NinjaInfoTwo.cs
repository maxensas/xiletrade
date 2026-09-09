using System;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Ninja.Contract;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed record NinjaInfoTwo : NinjaInfoBase
{
    internal NinjaInfoTwo(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        var item = _vm.Item;
        League = _vm.Form.League[_vm.Form.LeagueIndex];
        Type = GetType(item);
        var urlSuffix = League.Replace(" ", "+") + "&type=" + Type;
        Url = Strings.ApiNinjaItem + urlSuffix;
        Link = GetLink(item) + "/" + Normalize(item.NameEn) + "-" + Normalize(item.TypeEn);
        VerifiedLink = League.Length > 0 && Type.Length > 0;
    }

    private string GetLink(ItemData item)
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

        return Strings.UrlPoeNinja + ninjaLeague + GetType(item, webCategory : true);
    }

    private static string GetType(ItemData item, bool webCategory = false)
    {
        return item.Flag.Weapon.IsWeapon ? webCategory ? "unique-weapons" : Strings.NinjaTypeTwo.UniqueWeapons
            : item.Flag.Armour.IsArmour ? webCategory ? "unique-armours" : Strings.NinjaTypeTwo.UniqueArmours
            : item.Flag.Tablet ? webCategory ? "unique-tablets" : Strings.NinjaTypeTwo.UniqueTablets
            : item.Flag.Slot.Charm ? webCategory ? "unique-charms" : Strings.NinjaTypeTwo.UniqueCharms
            : item.Flag.Jewellery.IsJewellery ? webCategory ? "unique-accessories" : Strings.NinjaTypeTwo.UniqueAccessories
            : item.Flag.Slot.Flask ? webCategory ? "unique-flasks" : Strings.NinjaTypeTwo.UniqueFlasks
            : item.Flag.Jewel.IsJewel ? webCategory ? "unique-jewels" : Strings.NinjaTypeTwo.UniqueJewels
            : item.Flag.SanctumRelic ? webCategory ? "unique-relics" : Strings.NinjaTypeTwo.UniqueSanctumRelics
            : string.Empty;
    }

    internal override async Task<NinjaValue> GetNinjaValueAsync()
    {
        if (string.IsNullOrEmpty(_vm.Item.NameEn))
        {
            return null;
        }
        var jsonItem = await _ninja.GetNinjaItem<NinjaItemTwoContract>(this);
        if (jsonItem is null)
        {
            return null;
        }

        var line = jsonItem.Line.FirstOrDefault(_vm.Form.UnidentifiedUnique
            ? x => x.Name == _vm.Form.Unique[_vm.Form.UniqueIndex].Name
            : x => x.ItemId == $"{_vm.Item.NameEn} {_vm.Item.TypeEn}");
        line ??= jsonItem.Line.FirstOrDefault(x => x.Name == _vm.Item.NameEn);
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

        return new()
        {
            Id = line.ItemId,
            Name = line.Name,
            ChaosPrice = chaosPrice,
            ExaltPrice = exaltedPrice,
            DivinePrice = divinePrice
        };
    }
}
