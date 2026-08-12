using System.Collections.Generic;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain;

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

    internal XiletradeOption Socket { get; set; } = new();
    internal XiletradeOption Link { get; set; } = new();
    internal XiletradeOption Quality { get; set; } = new();
    internal XiletradeOption Lvl { get; set; } = new();
    internal XiletradeOption DpsTotal { get; set; } = new();
    internal XiletradeOption DpsPhys { get; set; } = new();
    internal XiletradeOption DpsElem { get; set; } = new();
    internal XiletradeOption Armour { get; set; } = new();
    internal XiletradeOption Energy { get; set; } = new();
    internal XiletradeOption Evasion { get; set; } = new();
    internal XiletradeOption Ward { get; set; } = new();
    internal XiletradeOption MapIiq { get; set; } = new();
    internal XiletradeOption MapIir { get; set; } = new();
    internal XiletradeOption MapPack { get; set; } = new();
    internal XiletradeOption MapScarab { get; set; } = new();
    internal XiletradeOption MapCurrency { get; set; } = new();
    internal XiletradeOption MapDivCard { get; set; } = new();
    internal XiletradeOption Resolve { get; set; } = new();
    internal XiletradeOption MaxResolve { get; set; } = new();
    internal XiletradeOption Inspiration { get; set; } = new();
    internal XiletradeOption Aureus { get; set; } = new();
    internal XiletradeOption RuneSockets { get; set; } = new();
    internal XiletradeOption GemSockets { get; set; } = new();
    internal XiletradeOption ReqLevel { get; set; } = new();
    internal XiletradeOption MemoryStrand { get; set; } = new();
    internal XiletradeOption ItemRarity { get; set; } = new();
    internal XiletradeOption MonsterRarity { get; set; } = new();
    internal XiletradeOption Effectiveness { get; set; } = new();
    internal XiletradeOption PackSize { get; set; } = new();
    internal XiletradeOption WaystoneDrop { get; set; } = new();
    internal XiletradeOption Revives { get; set; } = new();
    internal XiletradeOption GoldFound { get; set; } = new();
    internal XiletradeOption DeadSulphur { get; set; } = new();
    internal XiletradeOption FacetorExp { get; set; } = new();

    internal DefaultOption Corrupted { get; set; }
    internal DefaultOption TwiceCorrupted { get; set; }
    internal DefaultOption Identified { get; set; }
    internal DefaultOption Fractured { get; set; }
    internal DefaultOption Mirrored { get; set; }
    internal DefaultOption Split { get; set; }
    internal DefaultOption Crafted { get; set; }
    internal DefaultOption Mutated { get; set; }
    internal DefaultOption Veiled { get; set; } // Unrevealed
    internal DefaultOption Desecrated { get; set; }
    internal DefaultOption Sanctified { get; set; }

    internal string RewardType { get; set; }
    internal string Reward { get; set; }
    internal string Rarity { get; set; }
    internal string UniqueName { get; set; }

    internal double SocketRed { get; set; } = ModFilter.EMPTYFIELD;
    internal double SocketGreen { get; set; } = ModFilter.EMPTYFIELD;
    internal double SocketBlue { get; set; } = ModFilter.EMPTYFIELD;
    internal double SocketWhite { get; set; } = ModFilter.EMPTYFIELD;
    internal double PriceMin { get; set; } = ModFilter.EMPTYFIELD;

    internal List<ItemFilter> ItemFilters { get; set; } = new();
}
