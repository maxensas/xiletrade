using System.Linq;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed record NinjaInfoTwo : NinjaInfoBase
{
    internal NinjaInfoTwo(DataManagerService dm, PoeNinjaService ninja, string league, ItemData item) : base(dm, ninja)
    {
        League = league;
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
        return item.Flag.Weapon ? webCategory ? "unique-weapons" : Strings.NinjaTypeTwo.UniqueWeapons
            : item.Flag.ArmourPiece ? webCategory ? "unique-armours" : Strings.NinjaTypeTwo.UniqueArmours
            : item.Flag.Tablet ? webCategory ? "unique-tablets" : Strings.NinjaTypeTwo.UniqueTablets
            : item.Flag.Charm ? webCategory ? "unique-charms" : Strings.NinjaTypeTwo.UniqueCharms
            : item.Flag.Jewellery ? webCategory ? "unique-accessories" : Strings.NinjaTypeTwo.UniqueAccessories
            : item.Flag.Flask ? webCategory ? "unique-flasks" : Strings.NinjaTypeTwo.UniqueFlasks
            : item.Flag.Jewel ? webCategory ? "unique-jewels" : Strings.NinjaTypeTwo.UniqueJewels
            : item.Flag.SanctumRelic ? webCategory ? "unique-relics" : Strings.NinjaTypeTwo.UniqueSanctumRelics
            : string.Empty;
    }
}
