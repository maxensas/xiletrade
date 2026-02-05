using System;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class TotalStats
{
    //private readonly bool _isPoe2;
    
    internal double Resistance { get; private set; } = 0;
    internal double Life { get; private set; } = 0;
    internal double EnergyShield { get; private set; } = 0;

    internal TotalStats(bool isPoe2)
    {
        //_isPoe2 = isPoe2;
    }

    internal void Fill(FilterData filterEn, ModFilter modFilter, Lang lang, ReadOnlySpan<char> currentValue)
    {
        string modLowerEnglish = modFilter.Entrie.Text.ToLowerInvariant();
        if (lang is not Lang.English)
        {
            modLowerEnglish = filterEn.GetFilterDataEntry(modFilter.Entrie.ID)?.Text.ToLowerInvariant() ?? modLowerEnglish;
        }

        double totResist = CalculateTotalResist(modLowerEnglish, currentValue, includeChaos: true);
        if (totResist is not 0)
        {
            Resistance = Resistance > 0 ? Resistance + totResist : totResist;
        }
        double totLife = CalculateTotalLife(modLowerEnglish, currentValue);
        if (totLife is not 0)
        {
            Life = Life > 0 ? Life + totLife : totLife;
        }
        double totEs = CalculateGlobalEs(modLowerEnglish, currentValue);
        if (totEs is not 0)
        {
            EnergyShield = EnergyShield > 0 ? EnergyShield + totEs : totEs;
        }
    }

    internal static bool IsTotalStat(ReadOnlySpan<char> modEnLow, Stat stat)
    {
        bool cond = false;
        foreach (var words in stat is Stat.Life ? Strings.lTotalStatLifeUnwanted :
            stat is Stat.Es ? Strings.lTotalStatEsUnwanted : Strings.lTotalStatResistUnwanted)
        {
            cond = cond || modEnLow.Contain(words);
        }

        cond = (stat is Stat.Life ? modEnLow.Contain(Strings.Words.ToMaxLife)
            || modEnLow.Contain(Strings.Words.ToStrength) :
            stat is Stat.Es ? modEnLow.Contain(Strings.Words.ToMaxEs) :
            modEnLow.Contain(Strings.Words.Resistance)) && !cond;

        return cond;
    }

    //private
    private static int CalculateTotalResist(ReadOnlySpan<char> modEnLow, ReadOnlySpan<char> currentValue, bool includeChaos)
    {
        int returnVal = 0;
        if (!IsTotalStat(modEnLow, Stat.Resist))
        {
            return returnVal;
        }
        Span<char> currentReplaced = stackalloc char[currentValue.Length];
        for (int i = 0; i < currentValue.Length; i++)
        {
            currentReplaced[i] = currentValue[i] == '.' ? ',' : currentValue[i];
        }

        if (double.TryParse(currentReplaced, out double currentVal))
        {
            if (modEnLow.Contain(Strings.Words.ToAllResist))
            {
                return Convert.ToInt32(currentVal) * 3;
            }
            if (modEnLow.Contain(Strings.Words.Fire))
            {
                returnVal = Convert.ToInt32(currentVal);
            }
            if (modEnLow.Contain(Strings.Words.Cold))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
            if (modEnLow.Contain(Strings.Words.Lightning))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
            if (includeChaos && modEnLow.Contain(Strings.Words.Chaos))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
        }
        return returnVal;
    }

    private static double CalculateTotalLife(ReadOnlySpan<char> modEnLow, ReadOnlySpan<char> currentValue)
    {
        if (!IsTotalStat(modEnLow, Stat.Life))
        {
            return 0;
        }
        Span<char> currentReplaced = stackalloc char[currentValue.Length];
        for (int i = 0; i < currentValue.Length; i++)
        {
            currentReplaced[i] = currentValue[i] == '.' ? ',' : currentValue[i];
        }
        if (double.TryParse(currentReplaced, out double currentVal))
        {
            var cond = modEnLow.Contain(Strings.Words.ToStrength);
            return cond ? Math.Truncate(currentVal / 2) : currentVal;
        }
        return 0;
    }

    private static int CalculateGlobalEs(ReadOnlySpan<char> modEnLow, ReadOnlySpan<char> currentValue)
    {
        if (IsTotalStat(modEnLow, Stat.Es) && int.TryParse(currentValue, out int currentVal))
        {
            return currentVal;
        }
        return 0;
    }
}
