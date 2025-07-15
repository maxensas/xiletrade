using System;
using System.Linq;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Parser;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal sealed class TotalStats
{
    internal double Resistance { get; private set; } = 0;
    internal double Life { get; private set; } = 0;
    internal double EnergyShield { get; private set; } = 0;

    internal TotalStats()
    {

    }

    internal void Fill(FilterData filterEn, ModFilter modFilter, ItemData item, string currentValue)
    {
        string modTextEnglish = modFilter.Text;
        if (item.Lang is not Lang.English)
        {
            var enResult =
                from result in filterEn.Result
                from Entrie in result.Entries
                where Entrie.ID == modFilter.ID
                select Entrie.Text;
            if (enResult.Any())
            {
                modTextEnglish = enResult.First();
            }
        }

        double totResist = CalculateTotalResist(modTextEnglish, currentValue, includeChaos: !item.IsPoe2);
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

        cond = (stat is Stat.Life ? modLower.Contain("to maximum life")
            || modLower.Contain("to strength") :
            stat is Stat.Es ? modLower.Contain("to maximum energy shield") :
            modLower.Contain("resistance")) && !cond;

        return cond;
    }

    //private
    private static int CalculateTotalResist(string mod, string currentValue, bool includeChaos)
    {
        int returnVal = 0;
        if (IsTotalStat(mod, Stat.Resist)
            && double.TryParse(currentValue.Replace(".", ","), out double currentVal))
        {
            if (mod.Contain("to all Elemental Resistances"))
            {
                return Convert.ToInt32(currentVal) * 3;
            }

            string modLower = mod.ToLowerInvariant();
            if (modLower.Contain("fire"))
            {
                returnVal = Convert.ToInt32(currentVal);
            }
            if (modLower.Contain("cold"))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
            if (modLower.Contain("lightning"))
            {
                returnVal += Convert.ToInt32(currentVal);
            }
            if (includeChaos && modLower.Contain("chaos"))
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
            var cond = mod.ToLowerInvariant().Contain("to strength");
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
