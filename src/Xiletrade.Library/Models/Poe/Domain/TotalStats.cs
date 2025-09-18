using System;
using System.Linq;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain.Parser;
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

    internal void Fill(FilterData filterEn, ModFilter modFilter, ItemData item, string currentValue)
    {
        string modTextEnglish = modFilter.Entrie.Text;
        if (item.Lang is not Lang.English)
        {
            modTextEnglish = filterEn.Result.SelectMany(result => result.Entries)
                .FirstOrDefault(entry => entry.ID == modFilter.Entrie.ID)?.Text ?? modTextEnglish;
        }

        double totResist = CalculateTotalResist(modTextEnglish, currentValue, includeChaos: true);
        if (totResist is not 0)
        {
            Resistance = Resistance > 0 ? Resistance + totResist : totResist;
        }
        double totLife = CalculateTotalLife(modTextEnglish, currentValue);
        if (totLife is not 0)
        {
            Life = Life > 0 ? Life + totLife : totLife;
        }
        double totEs = CalculateGlobalEs(modTextEnglish, currentValue);
        if (totEs is not 0)
        {
            EnergyShield = EnergyShield > 0 ? EnergyShield + totEs : totEs;
        }
    }

    internal static bool IsTotalStat(string modEnglish, Stat stat)
    {
        bool cond = false;
        string modLower = modEnglish.ToLowerInvariant();

        foreach (var words in stat is Stat.Life ? Strings.lTotalStatLifeUnwanted :
            stat is Stat.Es ? Strings.lTotalStatEsUnwanted : Strings.lTotalStatResistUnwanted)
        {
            cond = cond || modLower.Contain(words);
        }

        cond = (stat is Stat.Life ? modLower.Contain(Strings.Words.ToMaxLife)
            || modLower.Contain(Strings.Words.ToStrength) :
            stat is Stat.Es ? modLower.Contain(Strings.Words.ToMaxEs) :
            modLower.Contain(Strings.Words.Resistance)) && !cond;

        return cond;
    }

    //private
    private static int CalculateTotalResist(string mod, string currentValue, bool includeChaos)
    {
        int returnVal = 0;
        if (IsTotalStat(mod, Stat.Resist)
            && double.TryParse(currentValue.Replace(".", ","), out double currentVal))
        {
            if (mod.Contain(Strings.Words.ToAllResist))
            {
                return Convert.ToInt32(currentVal) * 3;
            }

            string modLower = mod.ToLowerInvariant();
            if (modLower.Contain(Strings.Words.Fire))
            {
                returnVal = Convert.ToInt32(currentVal);
            }
            if (modLower.Contain(Strings.Words.Cold))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
            if (modLower.Contain(Strings.Words.Lightning))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
            if (includeChaos && modLower.Contain(Strings.Words.Chaos))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
        }
        return returnVal;
    }

    private static double CalculateTotalLife(string mod, string currentValue)
    {
        if (IsTotalStat(mod, Stat.Life)
            && double.TryParse(currentValue.Replace(".", ","), out double currentVal))
        {
            var cond = mod.ToLowerInvariant().Contain(Strings.Words.ToStrength);
            return cond ? Math.Truncate(currentVal / 2) : currentVal;
        }
        return 0;
    }

    private static int CalculateGlobalEs(string mod, string currentValue)
    {
        if (IsTotalStat(mod, Stat.Es) && int.TryParse(currentValue, out int currentVal))
        {
            return currentVal;
        }
        return 0;
    }
}
