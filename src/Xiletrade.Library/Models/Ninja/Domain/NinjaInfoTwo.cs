using System.Linq;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed record NinjaInfoTwo : NinjaInfoBase
{
    internal string Id { get; private set; }

    internal NinjaInfoTwo(DataManagerService dm, PoeNinjaService ninja, string league, ItemData item) : base(dm, ninja)
    {
        Id = item.Id;
        League = league;
        Type = GetType(item);
        var urlSuffix = League.Replace(" ", "+") + "&type=" + Type;
        Url = Strings.ApiNinjaItem + urlSuffix;
        Link = GetLink() + "/" + Normalize(item.NameEn) + "-" + Normalize(item.TypeEn);
        VerifiedLink = League.Length > 0 && Type.Length > 0;
    }

    private static string GetType(ItemData item)
    {
        return  item.Flag.Weapon ? Strings.NinjaTypeTwo.UniqueWeapons 
            : item.Flag.ArmourPiece ? Strings.NinjaTypeTwo.UniqueArmours
            : item.Flag.Tablet ? Strings.NinjaTypeTwo.UniqueMaps
            : item.Flag.Charm ? Strings.NinjaTypeTwo.UniqueCharms
            : item.Flag.Jewellery ? Strings.NinjaTypeTwo.UniqueAccessories
            : item.Flag.Flask ? Strings.NinjaTypeTwo.UniqueFlasks
            : item.Flag.Jewel ? Strings.NinjaTypeTwo.UniqueJewels
            : item.Flag.SanctumRelic ? Strings.NinjaTypeTwo.UniqueSanctumRelics
            : string.Empty;
    }

    private string GetLink()
    {
        var ninjaLeague = "standard/";

        var leagueSelect = _dm.League.Result.FirstOrDefault(x => x.Id == League);
        if (leagueSelect is not null)
        {
            var league = _ninja.NinjaState.Leagues.Where(x => x.Name == leagueSelect.Id).FirstOrDefault();
            if (league is not null)
            {
                ninjaLeague = league.Url + "/";
            }
        }

        return Strings.UrlPoeNinja + ninjaLeague + GetWebCategory();
    }

    private string GetWebCategory()
    {
        return Type is Strings.NinjaTypeTwo.UniqueWeapons ? "unique-weapons"
            : Type is Strings.NinjaTypeTwo.UniqueArmours ? "unique-armours"
            : Type is Strings.NinjaTypeTwo.UniqueMaps ? "unique-maps"
            : Type is Strings.NinjaTypeTwo.UniqueCharms ? "unique-charms"
            : Type is Strings.NinjaTypeTwo.UniqueAccessories ? "unique-accessories"
            : Type is Strings.NinjaTypeTwo.UniqueFlasks ? "unique-flasks"
            : Type is Strings.NinjaTypeTwo.UniqueJewels ? "unique-jewels"
            : Type is Strings.NinjaTypeTwo.UniqueSanctumRelics ? "unique-relics"
            : string.Empty;
    }
}
