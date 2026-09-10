using System.Linq;
using System.Text.RegularExpressions;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed record ModFilter
{
    /// <summary>Empty fields will not be added to json</summary>
    internal const int EMPTYFIELD = 99999;

    internal ItemModifier Mod { get; }
    internal FilterResultEntrie Entrie { get; private set; } = new();
    internal ModValue ModValue { get; } = new();
    
    internal bool IsFetched { get; }

    internal ModFilter(DataManagerService dm, ItemModifier mod, ItemData item)
    {
        Mod = mod;

        var inputRegex = GetInputRegex(mod);

        foreach (var filter in dm.Filter.Result)
        {
            var entries = filter.FindEntries(mod, item, inputRegex);
            
            if (entries.Count > 0)
            {
                var (entrie, min, max) = entries.GetMinMaxEntrie(dm, mod, item);
                if (entrie is not null)
                {
                    ModValue.ListAffix.Add(new(dm, filter, entrie, item, mod.Affix));
                    if (Entrie.ID.Length is 0)
                    {
                        Entrie = entrie;
                        ModValue.Min = min;
                        ModValue.Max = max;
                    }
                }
                continue;
            }
        }

        IsFetched = Entrie.ID != string.Empty;
    }

    private static Regex GetInputRegex(ItemModifier mod)
    {
        string inputRegEscape = Regex.Escape(RegexUtil.DecimalPattern().Replace(mod.Parsed, "#"));
        string inputRegPattern = RegexUtil.DiezePattern().Replace(inputRegEscape, RegexUtil.DecimalPatternDieze);
        return new Regex("^" + inputRegPattern + "$", RegexOptions.IgnoreCase);
    }

    // not used anymore
    private static bool TryGetLogbookEntrie(FilterResult filter, ItemModifier mod
        , out FilterResultEntrie entrie)
    {
        entrie = null;
        var entrieSeek = filter.FindEntryById(Strings.Stat.Generic.LogbookBoss, sequenceEquality: false);
        if (entrieSeek is not null && entrieSeek.Option.Options.Length > 0
            && entrieSeek.Option.Options.Any(opt => mod.Parsed.Contain(opt.Text)))
        {
            entrie = entrieSeek;
            return true;
        }
        entrieSeek = filter.FindEntryByType(mod.Parsed, sequenceEquality: false);
        if (entrieSeek is not null && entrieSeek.ID.Contain(Strings.Words.Logbook))
        {
            entrie = entrieSeek;
            return true;
        }
        return false;
    }
}
