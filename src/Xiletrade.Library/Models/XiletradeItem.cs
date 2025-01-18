using System.Collections.Generic;

namespace Xiletrade.Library.Models;

internal sealed class XiletradeItem
{
    internal bool SynthesisBlight { get; set; }
    internal bool BlightRavaged { get; set; }
    internal bool Scourged { get; set; }
    internal bool InfShaper { get; set; }
    internal bool InfElder { get; set; }
    internal bool InfCrusader { get; set; }
    internal bool InfRedeemer { get; set; }
    internal bool InfHunter { get; set; }
    internal bool InfWarlord { get; set; }
    internal bool ChaosDivOnly { get; set; }
    internal bool ExaltOnly { get; set; }
    internal bool ByType { get; set; }
    internal bool SocketColors { get; set; }
    internal bool ChkSocket { get; set; }
    internal bool ChkQuality { get; set; }
    internal bool ChkLv { get; set; }
    internal bool ChkDpsTotal { get; set; }
    internal bool ChkDpsPhys { get; set; }
    internal bool ChkDpsElem { get; set; }
    internal bool ChkArmour { get; set; }
    internal bool ChkEnergy { get; set; }
    internal bool ChkEvasion { get; set; }
    internal bool ChkWard { get; set; }
    internal bool ChkMapIiq { get; set; }
    internal bool ChkMapIir { get; set; }
    internal bool ChkMapPack { get; set; }
    internal bool ChkMapScarab { get; set; }
    internal bool ChkMapCurrency { get; set; }
    internal bool ChkMapDivCard { get; set; }
    internal bool ChkResolve { get; set; }
    internal bool ChkMaxResolve { get; set; }
    internal bool ChkInspiration { get; set; }
    internal bool ChkAureus { get; set; }
    internal bool ChkRuneSockets { get; set; }

    internal string Corrupted { get; set; }
    internal string AlternateQuality { get; set; }
    internal string RewardType { get; set; }
    internal string Reward { get; set; }
    internal string Rarity { get; set; }

    internal double ArmourMin { get; set; }
    internal double ArmourMax { get; set; }
    internal double EnergyMin { get; set; }
    internal double EnergyMax { get; set; }
    internal double EvasionMin { get; set; }
    internal double EvasionMax { get; set; }
    internal double WardMin { get; set; }
    internal double WardMax { get; set; }
    internal double DpsTotalMin { get; set; }
    internal double DpsTotalMax { get; set; }
    internal double DpsPhysMin { get; set; }
    internal double DpsPhysMax { get; set; }
    internal double DpsElemMin { get; set; }
    internal double DpsElemMax { get; set; }
    internal double SocketRed { get; set; }
    internal double SocketGreen { get; set; }
    internal double SocketBlue { get; set; }
    internal double SocketWhite { get; set; }
    internal double SocketMin { get; set; }
    internal double SocketMax { get; set; }
    internal double LinkMin { get; set; }
    internal double LinkMax { get; set; }
    internal double QualityMin { get; set; }
    internal double QualityMax { get; set; }
    internal double LvMin { get; set; }
    internal double LvMax { get; set; }
    internal double PriceMin { get; set; }
    internal double FacetorExpMin { get; set; }
    internal double FacetorExpMax { get; set; }
    internal double ResolveMin { get; set; }
    internal double ResolveMax { get; set; }
    internal double MaxResolveMin { get; set; }
    internal double MaxResolveMax { get; set; }
    internal double InspirationMin { get; set; }
    internal double InspirationMax { get; set; }
    internal double AureusMin { get; set; }
    internal double AureusMax { get; set; }
    internal double MapItemQuantityMin { get; set; }
    internal double MapItemQuantityMax { get; set; }
    internal double MapItemRarityMin { get; set; }
    internal double MapItemRarityMax { get; set; }
    internal double MapPackSizeMin { get; set; }
    internal double MapPackSizeMax { get; set; }
    internal double MapMoreScarabMin { get; set; }
    internal double MapMoreScarabMax { get; set; }
    internal double MapMoreCurrencyMin { get; set; }
    internal double MapMoreCurrencyMax { get; set; }
    internal double MapMoreDivCardMin { get; set; }
    internal double MapMoreDivCardMax { get; set; }
    internal double RuneSocketsMin { get; set; }
    internal double RuneSocketsMax { get; set; }

    internal List<ItemFilter> ItemFilters { get; set; } = new();
}
