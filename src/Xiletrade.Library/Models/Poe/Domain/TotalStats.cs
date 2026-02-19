using System;
using System.Linq;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class TotalStats
{
    internal double CurrentResistance { get; private set; }
    internal double CurrentLife { get; private set; }
    internal double CurrentEnergyShield { get; private set; }
    internal double CurrentAttribute { get; private set; }

    internal double TierResistance { get; private set; }
    internal double TierLife { get; private set; }
    internal double TierEnergyShield { get; private set; }
    internal double TierAttribute { get; private set; }

    // not pretty
    internal void Fill(FilterData filterEn, ModFilter modFilter, Lang lang, 
        ReadOnlySpan<char> currentValue, double tierValue)
    {
        string modEnglish = modFilter.Entrie.Text;
        if (lang is not Lang.English)
        {
            modEnglish = filterEn.GetFilterDataEntry(modFilter.Entrie.ID)?.Text ?? modEnglish;
        }

        double totResist = CalculateTotalResist(modEnglish, currentValue);
        if (totResist is not 0)
        {
            CurrentResistance = CurrentResistance > 0 ? CurrentResistance + totResist : totResist;
            if (tierValue.IsNotEmpty())
            {
                bool isAll = modEnglish.Contains(Strings.Words.ToAllResist, StringComparison.OrdinalIgnoreCase);
                tierValue = isAll ? tierValue * 3 : tierValue;
                TierResistance = TierResistance > 0 ? TierResistance + tierValue : tierValue;
            }
        }
        double totLife = CalculateTotalLife(modEnglish, currentValue);
        if (totLife is not 0)
        {
            CurrentLife = CurrentLife > 0 ? CurrentLife + totLife : totLife;
            if (tierValue.IsNotEmpty())
            {
                TierLife = TierLife > 0 ? TierLife + tierValue : tierValue;
            }
        }
        double totEs = CalculateGlobalEs(modEnglish, currentValue);
        if (totEs is not 0)
        {
            CurrentEnergyShield = CurrentEnergyShield > 0 ? CurrentEnergyShield + totEs : totEs;
            if (tierValue.IsNotEmpty())
            {
                TierEnergyShield = TierEnergyShield > 0 ? TierEnergyShield + tierValue : tierValue;
            }
        }
        double totAttr = CalculateAttribute(modEnglish, currentValue);
        if (totAttr is not 0)
        {
            CurrentAttribute = CurrentAttribute > 0 ? CurrentAttribute + totAttr : totAttr;
            if (tierValue.IsNotEmpty())
            {
                TierAttribute = TierAttribute > 0 ? TierAttribute + tierValue : tierValue;
            }
        }
    }

    internal static bool IsTotalStat(ReadOnlySpan<char> modEn, Stat stat)
    {
        bool cond = false;
        foreach (var words in stat is Stat.Life ? Strings.lTotalStatLifeUnwanted :
            stat is Stat.Es ? Strings.lTotalStatEsUnwanted : Strings.lTotalStatResistUnwanted)
        {
            cond = cond || modEn.Contains(words, StringComparison.OrdinalIgnoreCase);
        }

        cond = (stat is Stat.Life ? modEn.Contains(Strings.Words.ToMaxLife, StringComparison.OrdinalIgnoreCase)
            || modEn.Contains(Strings.Words.ToStrength, StringComparison.OrdinalIgnoreCase) :
            stat is Stat.Es ? modEn.Contains(Strings.Words.ToMaxEs, StringComparison.OrdinalIgnoreCase) :
            modEn.Contains(Strings.Words.Resistance, StringComparison.OrdinalIgnoreCase)
            && !modEn.Contains(Strings.Words.Chaos, StringComparison.OrdinalIgnoreCase)) && !cond;

        return cond;
    }

    internal static bool IsAttribute(ReadOnlySpan<char> mod)
    {
        foreach (ReadOnlySpan<char> val in Strings.StatPoe2.dicAttributes.Values)
        {
            if (val.SequenceEqual(mod))
            {
                return true;
            }
        }
        return false;
    }

    internal static bool IsAllAttribute(ReadOnlySpan<char> mod)
        => Strings.StatPoe2.dicAttributes.Last().Value.AsSpan().SequenceEqual(mod);

    //private
    private static int CalculateTotalResist(ReadOnlySpan<char> modEn, ReadOnlySpan<char> currentValue)
    {
        int returnVal = 0;
        if (!IsTotalStat(modEn, Stat.Resist))
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
            if (modEn.Contains(Strings.Words.ToAllResist, StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToInt32(currentVal) * 3;
            }
            if (modEn.Contains(Strings.Words.Fire, StringComparison.OrdinalIgnoreCase))
            {
                returnVal = Convert.ToInt32(currentVal);
            }
            if (modEn.Contains(Strings.Words.Cold, StringComparison.OrdinalIgnoreCase))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
            if (modEn.Contains(Strings.Words.Lightning, StringComparison.OrdinalIgnoreCase))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
        }
        return returnVal;
    }

    private static double CalculateTotalLife(ReadOnlySpan<char> modEn, ReadOnlySpan<char> currentValue)
    {
        if (!IsTotalStat(modEn, Stat.Life))
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
            var cond = modEn.Contains(Strings.Words.ToStrength, StringComparison.OrdinalIgnoreCase);
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

    private static int CalculateAttribute(ReadOnlySpan<char> modEnLow, ReadOnlySpan<char> currentValue)
    {
        if (IsAttribute(modEnLow) 
            && int.TryParse(currentValue, out int currentVal))
        {
            return IsAllAttribute(modEnLow) ? currentVal * 3 : currentVal;
        }
        return 0;
    }
}
