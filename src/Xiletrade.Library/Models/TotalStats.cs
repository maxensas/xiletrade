using System.Linq;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
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

    internal void Fill(FilterResultEntrie modFilter, string currentValue, int idLang)
    {
        string modTextEnglish = modFilter.Text;
        if (idLang is not 0) // !("en-US")
        {
            var enResult =
                from result in DataManager.FilterEn.Result
                from Entrie in result.Entries
                where Entrie.ID == modFilter.ID
                select Entrie.Text;
            if (enResult.Any())
            {
                modTextEnglish = enResult.First();
            }
        }

        double totResist = Modifier.CalculateTotalResist(modTextEnglish, currentValue);
        if (totResist is not 0)
        {
            Resistance = Resistance > 0 ? Resistance + totResist : totResist;
        }
        double totLife = Modifier.CalculateTotalLife(modTextEnglish, currentValue);
        if (totLife is not 0)
        {
            Life = Life > 0 ? Life + totLife : totLife;
        }
        double totEs = Modifier.CalculateGlobalEs(modTextEnglish, currentValue);
        if (totEs is not 0)
        {
            EnergyShield = EnergyShield > 0 ? EnergyShield + totEs : totEs;
        }
    }
}
