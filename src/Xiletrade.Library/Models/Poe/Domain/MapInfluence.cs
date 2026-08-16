using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed record MapInfluence
{
    // Elder
    internal bool Enslaver { get; }
    internal bool Eradicator { get; }
    internal bool Constrictor { get; }
    internal bool Purifier { get; }

    // Conq
    internal bool Baran { get; }
    internal bool Veritania { get; }
    internal bool AlHezmin { get; }
    internal bool Drox { get; }

    internal string GuardianName => Enslaver ? Strings.Stat.Map.Enslaver.Name
        : Eradicator ? Strings.Stat.Map.Eradicator.Name
        : Constrictor ? Strings.Stat.Map.Constrictor.Name
        : Purifier ? Strings.Stat.Map.Purifier.Name
        : Baran ? Strings.Stat.Map.Baran.Name
        : Veritania ? Strings.Stat.Map.Veritania.Name
        : AlHezmin ? Strings.Stat.Map.AlHezmin.Name
        : Drox ? Strings.Stat.Map.Drox.Name
        : string.Empty;

    internal MapInfluence(XiletradeItem xItem)
    {
        if (xItem.ItemFilters is null || xItem.ItemFilters.Count is 0)
        {
            return;
        }

        foreach (var filter in xItem.ItemFilters)
        {
            if (filter.Id == Strings.Stat.Map.Enslaver.Id)
            {
                Enslaver = true;
                return;
            }
            if (filter.Id == Strings.Stat.Map.Eradicator.Id)
            {
                Eradicator = true;
                return;
            }
            if (filter.Id == Strings.Stat.Map.Constrictor.Id)
            {
                Constrictor = true;
                return;
            }
            if (filter.Id == Strings.Stat.Map.Purifier.Id)
            {
                Purifier = true;
                return;
            }

            if (filter.Id == Strings.Stat.Map.Baran.Id)
            {
                Baran = true;
                return;
            }
            if (filter.Id == Strings.Stat.Map.Veritania.Id)
            {
                Veritania = true;
                return;
            }
            if (filter.Id == Strings.Stat.Map.AlHezmin.Id)
            {
                AlHezmin = true;
                return;
            }
            if (filter.Id == Strings.Stat.Map.Drox.Id)
            {
                Drox = true;
                return;
            }
        }
    }
}
