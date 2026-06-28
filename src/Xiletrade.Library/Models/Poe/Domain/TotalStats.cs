using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed record TotalStats
{
    private readonly double _currentResistance;
    private readonly double _currentLife;
    private readonly double _currentEnergyShield;
    private readonly double _currentAttribute;

    private readonly double _tierResistance;
    private readonly double _tierLife;
    private readonly double _tierEnergyShield;
    private readonly double _tierAttribute;

    // Can extend here
    internal Dictionary<StatPanel, (double current, double tier)> Map => new()
    {
        { StatPanel.TotalLife, (_currentLife, _tierLife) },
        { StatPanel.TotalElemResistance, (_currentResistance, _tierResistance) },
        { StatPanel.TotalGlobalEs, (_currentEnergyShield, _tierEnergyShield) },
        { StatPanel.TotalAttribute, (_currentAttribute, _tierAttribute) }
    };

    internal string GetResistance(bool preferTier) => preferTier && _tierResistance > 0 ?
            _tierResistance.ToStr() : _currentResistance.ToStr();

    internal string GetLife(bool preferTier) => preferTier && _tierLife > 0 ?
            _tierLife.ToStr() : _currentLife.ToStr();

    internal string GetEnergyShield(bool preferTier) => preferTier && _tierEnergyShield > 0 ?
            _tierEnergyShield.ToStr() : _currentEnergyShield.ToStr();

    internal string GetAttribute(bool preferTier) => preferTier && _tierAttribute > 0 ?
            _tierAttribute.ToStr() : _currentAttribute.ToStr();

    internal bool Resistance => _currentResistance > 0;
    internal bool Life => _currentLife > 0;
    internal bool EnergyShield => _currentEnergyShield > 0;
    internal bool Attribute => _currentAttribute > 0;

    internal double TotalPhysicalIncrease { get; }

    internal TotalStats(DataManagerService dm, ItemFlag flag, List<ModLine> modLineList, Lang lang)
    {
        if (!flag.Parseable || flag.Unique || flag.Jewel)
        {
            return;
        }
        foreach (var modLine in modLineList)
        {
            var tierValue = modLine.TierMin;
            string modEnglish = modLine.ItemFilter.Text;

            if (lang is not Lang.English)
            {
                modEnglish = dm.FilterEn.GetFilterDataEntry(modLine.ItemFilter.Id)?.Text ?? modEnglish;
            }

            double totResist = CalculateTotalResist(modEnglish, modLine.Current);
            if (totResist is not 0)
            {
                _currentResistance = _currentResistance > 0 ? _currentResistance + totResist : totResist;
                if (tierValue.IsNotEmpty())
                {
                    bool isAll = modEnglish.Contains(Strings.Words.ToAllResist, StringComparison.OrdinalIgnoreCase);
                    tierValue = isAll ? tierValue * 3 : tierValue;
                    _tierResistance = _tierResistance > 0 ? _tierResistance + tierValue : tierValue;
                }
            }
            double totLife = CalculateTotalLife(modEnglish, modLine.Current);
            if (totLife is not 0)
            {
                _currentLife = _currentLife > 0 ? _currentLife + totLife : totLife;
                if (tierValue.IsNotEmpty())
                {
                    _tierLife = _tierLife > 0 ? _tierLife + tierValue : tierValue;
                }
            }
            double totEs = CalculateGlobalEs(modEnglish, modLine.Current);
            if (totEs is not 0)
            {
                _currentEnergyShield = _currentEnergyShield > 0 ? _currentEnergyShield + totEs : totEs;
                if (tierValue.IsNotEmpty())
                {
                    _tierEnergyShield = _tierEnergyShield > 0 ? _tierEnergyShield + tierValue : tierValue;
                }
            }
            double totAttr = CalculateAttribute(modEnglish, modLine.Current);
            if (totAttr is not 0)
            {
                _currentAttribute = _currentAttribute > 0 ? _currentAttribute + totAttr : totAttr;
                if (tierValue.IsNotEmpty())
                {
                    _tierAttribute = _tierAttribute > 0 ? _tierAttribute + tierValue : tierValue;
                }
            }

            var minFilter = modLine.ItemFilter.Min;
            if (modLine.ItemFilter.Id.Contain(Strings.Stat.Generic.IncPhys)
                && minFilter > 0 && minFilter < 9999)
            {
                TotalPhysicalIncrease += minFilter;
            }
        }
    }

    private static int CalculateTotalResist(ReadOnlySpan<char> modEn, ReadOnlySpan<char> currentValue)
    {
        int returnVal = 0;
        if (!Strings.StatTotal.IsTotalStat(modEn, Stat.Resist))
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
        if (!Strings.StatTotal.IsTotalStat(modEn, Stat.Life))
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
        if (Strings.StatTotal.IsTotalStat(modEnLow, Stat.Es) && int.TryParse(currentValue, out int currentVal))
        {
            return currentVal;
        }
        return 0;
    }

    private static int CalculateAttribute(ReadOnlySpan<char> modEnLow, ReadOnlySpan<char> currentValue)
    {
        if (Strings.StatPoe2.IsAttribute(modEnLow) 
            && int.TryParse(currentValue, out int currentVal))
        {
            return Strings.StatPoe2.IsAllAttribute(modEnLow) ? currentVal * 3 : currentVal;
        }
        return 0;
    }
}
