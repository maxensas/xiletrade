﻿namespace Xiletrade.Library.Models;

internal sealed class NinjaInfo
{
    internal string League { get; private set; }
    internal string Rarity { get; private set; }
    internal string LvlMin { get; private set; }
    internal string QualMin { get; private set; }
    internal int AltIdx { get; private set; }
    internal bool SynthBlight { get; private set; }
    internal bool Ravaged { get; private set; }
    internal bool ScourgedMap { get; private set; }
    internal string Influences { get; private set; }

    internal NinjaInfo(string league, string rarity, string lvlMin, string qualMin
        , int altIdx, bool synthBlight, bool ravaged, bool scourgedMap, string influences)
    {
        League = league;
        Rarity = rarity;
        LvlMin = lvlMin;
        QualMin = qualMin;
        AltIdx = altIdx;
        SynthBlight = synthBlight;
        Ravaged = ravaged;
        ScourgedMap = scourgedMap;
        Influences = influences;
    }
}
