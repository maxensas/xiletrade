using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Xiletrade.Library.Models.Poe.Domain.Parser;

namespace Xiletrade.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[RankColumn]
public class SpanBenchmark
{
    private static readonly string _infoDescPoe2 = "Item Class: Quarterstaves\r\nRarity: Rare\r\nFate Call\r\nAegis Quarterstaff\r\n--------\r\nQuality: +24% (augmented)\r\nPhysical Damage: 416-681 (augmented)\r\nLightning Damage: 19-342 (lightning)\r\nCritical Hit Chance: 14.80% (augmented)\r\nAttacks per Second: 1.74 (augmented)\r\n--------\r\nRequires: Level 79, 127 Dex, 50 Int\r\n--------\r\nSockets: S S S \r\n--------\r\nItem Level: 82\r\n--------\r\n18% increased Physical Damage (rune)\r\n50% increased Attack Damage against Rare or Unique Enemies (rune)\r\nGain 5% of Damage as Extra Damage of all Elements (rune)\r\n--------\r\n{ Implicit Modifier }\r\n+15(12-18)% to Block chance\r\n--------\r\n{ Fractured Prefix Modifier \"Vapourising\" (Tier: 1) — Damage, Elemental, Lightning, Attack }\r\nAdds 19(1-19) to 342(310-358) Lightning Damage\r\n{ Prefix Modifier \"Merciless\" (Tier: 1) — Damage, Physical, Attack }\r\n179(170-179)% increased Physical Damage\r\n{ Desecrated Prefix Modifier \"Flaring\" (Tier: 1) — Damage, Physical, Attack }\r\nAdds 55(37-55) to 88(63-94) Physical Damage\r\n{ Suffix Modifier \"of Unmaking\" (Tier: 1) — Attack, Critical }\r\n+4.8(4.41-5)% to Critical Hit Chance\r\n{ Suffix Modifier \"of Infamy\" (Tier: 2) — Attack, Speed }\r\n24(23-25)% increased Attack Speed\r\n{ Suffix Modifier \"of the Essence\" — Attack }\r\n+5 to Level of all Attack Skills\r\n--------\r\nFractured Item\r\n";
    private static readonly string _infoDescPoe1 = "Item Class: Gloves\r\nRarity: Rare\r\nArmageddon Talons\r\nHydrascale Gauntlets\r\n--------\r\nQuality: +20% (augmented)\r\nArmour: 222 (augmented)\r\nEvasion Rating: 222 (augmented)\r\n--------\r\nRequirements:\r\nLevel: 72\r\nStr: 45\r\nDex: 98\r\nInt: 98\r\n--------\r\nSockets: W-W-B-G \r\n--------\r\nItem Level: 86\r\n--------\r\n{ Eater of Worlds Implicit Modifier (Perfect) — Damage, Physical, Elemental, Cold }\r\n35% of Physical Damage Converted to Cold Damage\r\n{ Searing Exarch Implicit Modifier (Greater) }\r\n20% chance to Unnerve Enemies for 4 seconds on Hit\r\n(Unnerved enemies take 10% increased Spell Damage)\r\n--------\r\n{ Prefix Modifier \"Athlete's\" (Tier: 1) — Life }\r\n+127(115-129) to maximum Life\r\n{ Prefix Modifier \"Gladiator's\" (Tier: 4) — Defences, Armour, Evasion }\r\n57(56-67)% increased Armour and Evasion\r\n{ Master Crafted Prefix Modifier \"Upgraded\" — Damage, Physical, Elemental, Cold }\r\n25(20-25)% of Physical Damage Converted to Cold Damage\r\n{ Suffix Modifier \"of Puhuarte\" — Damage, Elemental, Cold, Resistance }\r\n+48(46-48)% to Cold Resistance\r\n48(30-50)% increased Damage with Hits against Chilled Enemies\r\n{ Suffix Modifier \"of Nullification\" (Tier: 1) }\r\n+13(13-14)% chance to Suppress Spell Damage\r\n(40% of Damage from Suppressed Hits and Ailments they inflict is prevented)\r\n{ Suffix Modifier \"of Exile\" (Tier: 2) — Chaos, Resistance }\r\n+30(26-30)% to Chaos Resistance\r\nSearing Exarch Item\r\nEater of Worlds Item\r\n";

    public SpanBenchmark()
    {

    }

    //| Method                     | Mean     | Error     | StdDev    | Rank | Gen0   | Gen1   | Allocated |
    //|--------------------------- |---------:|----------:|----------:|-----:|-------:|-------:|----------:|
    //| XiletradePoe2Span          | 2.484 us | 0.0372 us | 0.0330 us |    1 | 0.1640 |      - |   2.71 KB |
    //| XiletradePoe1Span          | 2.757 us | 0.0400 us | 0.0334 us |    2 | 0.1755 |      - |   2.91 KB |
    //|                            |          |           |           |      |        |        |           |
    //| XiletradePoe2Stringbuilder | 4.011 us | 0.0365 us | 0.0342 us |    1 | 0.9537 | 0.0076 |  15.64 KB |
    //| XiletradePoe1Stringbuilder | 4.185 us | 0.0624 us | 0.0584 us |    2 | 1.0376 | 0.0153 |  16.99 KB |

    [BenchmarkCategory("Xiletrade.Stringbuilder")]
    [Benchmark]
    public void XiletradePoe1Stringbuilder()
    {
        var infoDesc = new InfoDescription(_infoDescPoe1, useSb: true);
    }

    [BenchmarkCategory("Xiletrade.Span")]
    [Benchmark]
    public void XiletradePoe1Span()
    {
        // does not trigger ParseBracketMod()
        var infoDesc = new InfoDescription(_infoDescPoe1);
    }

    [BenchmarkCategory("Xiletrade.Stringbuilder")]
    [Benchmark]
    public void XiletradePoe2Stringbuilder()
    {
        var infoDesc = new InfoDescription(_infoDescPoe2, useSb: true);
    }

    [BenchmarkCategory("Xiletrade.Span")]
    [Benchmark]
    public void XiletradePoe2Span()
    {
        // does not trigger ParseBracketMod()
        var infoDesc = new InfoDescription(_infoDescPoe2);
    }
}
