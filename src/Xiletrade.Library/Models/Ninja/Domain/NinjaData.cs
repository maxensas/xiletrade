using System;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal static class NinjaData
{
    internal static string League { get; set; }
    internal static NinjaCurrency Currency { get; private set; } = new();
    internal static NinjaCurrency Fragment { get; private set; } = new();
    internal static NinjaItem Oil { get; private set; } = new();
    internal static NinjaItem Incubator { get; private set; } = new();
    internal static NinjaItem Invitation { get; private set; } = new();
    internal static NinjaItem Scarab { get; private set; } = new();
    internal static NinjaItem Fossil { get; private set; } = new();
    internal static NinjaItem Resonator { get; private set; } = new();
    internal static NinjaItem Essence { get; private set; } = new();
    internal static NinjaItem DivinationCard { get; private set; } = new();
    internal static NinjaItem Prophecy { get; private set; } = new();
    internal static NinjaItem SkillGem { get; private set; } = new();
    internal static NinjaItem BaseType { get; private set; } = new();
    internal static NinjaItem UniqueMap { get; private set; } = new();
    internal static NinjaItem BlightedMap { get; private set; } = new();
    internal static NinjaItem BlightRavagedMap { get; private set; } = new();
    internal static NinjaItem ScourgedMap { get; private set; } = new();
    internal static NinjaItem Map { get; private set; } = new();
    internal static NinjaItem UniqueJewel { get; private set; } = new();
    internal static NinjaItem UniqueFlask { get; private set; } = new();
    internal static NinjaItem UniqueWeapon { get; private set; } = new();
    internal static NinjaItem UniqueArmour { get; private set; } = new();
    internal static NinjaItem UniqueAccessory { get; private set; } = new();
    internal static NinjaItem Beast { get; private set; } = new();
    internal static NinjaItem DeliriumOrb { get; private set; } = new();
    internal static NinjaItem Vial { get; private set; } = new();
    internal static NinjaItem Watchstone { get; private set; } = new();
    internal static NinjaItem ClusterJewel { get; private set; } = new();
    internal static NinjaItem Omen { get; private set; } = new();
    internal static NinjaItem Tattoo { get; private set; } = new();
    internal static NinjaItem UniqueRelic { get; private set; } = new();
    internal static NinjaItem Coffin { get; private set; } = new();
    internal static NinjaItem AllflameEmber { get; private set; } = new();
    internal static NinjaItem KalguuranRune { get; private set; } = new();
    internal static NinjaItem Memory { get; private set; } = new();
    internal static NinjaItem Artifact { get; private set; } = new();
    internal static NinjaItem AllFlameEmber { get; private set; } = new();

    internal static void CheckLeague(string league)
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
            Currency.Creation = Fragment.Creation = Oil.Creation = Incubator.Creation =
                Invitation.Creation = Scarab.Creation = Fossil.Creation = Resonator.Creation =
                Essence.Creation = DivinationCard.Creation = Prophecy.Creation = SkillGem.Creation =
                BaseType.Creation = UniqueMap.Creation = Map.Creation = UniqueJewel.Creation =
                UniqueFlask.Creation = UniqueWeapon.Creation = UniqueArmour.Creation =
                UniqueAccessory.Creation = Beast.Creation = DeliriumOrb.Creation = Vial.Creation =
                Watchstone.Creation = ClusterJewel.Creation = Omen.Creation = Tattoo.Creation =
                UniqueRelic.Creation = Coffin.Creation = AllflameEmber.Creation = KalguuranRune.Creation =
                Memory.Creation = Artifact.Creation = AllflameEmber.Creation = DateTime.MinValue;
        }
    }
}
