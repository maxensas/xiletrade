using System.Collections.Generic;

namespace Xiletrade.Library.Models;

internal sealed class XiletradeItem
{
    internal bool SynthesisBlight { get; set; }
    internal bool BlightRavaged { get; set; }
    internal bool InfShaper { get; set; }
    internal bool InfElder { get; set; }
    internal bool InfCrusader { get; set; }
    internal bool InfRedeemer { get; set; }
    internal bool InfHunter { get; set; }
    internal bool InfWarlord { get; set; }
    internal bool ChaosDivOnly { get; set; }
    internal bool ExaltOnly { get; set; }
    internal bool ChaosOnly { get; set; }
    internal bool ByType { get; set; }
    internal bool SocketColors { get; set; }
    internal bool ChkSocket { get; set; }
    internal bool ChkLink { get; set; }
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
    internal bool ChkReqLevel { get; set; }
    internal bool ChkMemoryStrand { get; set; }

    internal string Corrupted { get; set; }
    internal string RewardType { get; set; }
    internal string Reward { get; set; }
    internal string Rarity { get; set; }

    internal double ArmourMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double ArmourMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double EnergyMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double EnergyMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double EvasionMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double EvasionMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double WardMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double WardMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double DpsTotalMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double DpsTotalMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double DpsPhysMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double DpsPhysMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double DpsElemMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double DpsElemMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double SocketRed { get; set; } = ModFilter.EMPTYFIELD;
    internal double SocketGreen { get; set; } = ModFilter.EMPTYFIELD;
    internal double SocketBlue { get; set; } = ModFilter.EMPTYFIELD;
    internal double SocketWhite { get; set; } = ModFilter.EMPTYFIELD;
    internal double SocketMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double SocketMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double LinkMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double LinkMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double QualityMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double QualityMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double LvMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double LvMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double PriceMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double FacetorExpMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double FacetorExpMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double ResolveMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double ResolveMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double MaxResolveMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double MaxResolveMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double InspirationMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double InspirationMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double AureusMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double AureusMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapItemQuantityMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapItemQuantityMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapItemRarityMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapItemRarityMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapPackSizeMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapPackSizeMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapMoreScarabMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapMoreScarabMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapMoreCurrencyMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapMoreCurrencyMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapMoreDivCardMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double MapMoreDivCardMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double RuneSocketsMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double RuneSocketsMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double ReqLevelMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double ReqLevelMax { get; set; } = ModFilter.EMPTYFIELD;
    internal double MemoryStrandMin { get; set; } = ModFilter.EMPTYFIELD;
    internal double MemoryStrandMax { get; set; } = ModFilter.EMPTYFIELD;

    internal List<ItemFilter> ItemFilters { get; set; } = new();
}
