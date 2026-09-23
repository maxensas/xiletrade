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

    internal string GuardianName => Enslaver ? Strings.StatPoe1.Map.Enslaver.Name
        : Eradicator ? Strings.StatPoe1.Map.Eradicator.Name
        : Constrictor ? Strings.StatPoe1.Map.Constrictor.Name
        : Purifier ? Strings.StatPoe1.Map.Purifier.Name
        : Baran ? Strings.StatPoe1.Map.Baran.Name
        : Veritania ? Strings.StatPoe1.Map.Veritania.Name
        : AlHezmin ? Strings.StatPoe1.Map.AlHezmin.Name
        : Drox ? Strings.StatPoe1.Map.Drox.Name
        : string.Empty;

    internal MapInfluence(XiletradeItem xItem)
    {
        if (xItem.ItemFilters is null || xItem.ItemFilters.Count is 0)
        {
            return;
        }

        foreach (var filter in xItem.ItemFilters)
        {
            if (filter.Id == Strings.StatPoe1.Map.Enslaver.Id)
            {
                Enslaver = true;
                return;
            }
            if (filter.Id == Strings.StatPoe1.Map.Eradicator.Id)
            {
                Eradicator = true;
                return;
            }
            if (filter.Id == Strings.StatPoe1.Map.Constrictor.Id)
            {
                Constrictor = true;
                return;
            }
            if (filter.Id == Strings.StatPoe1.Map.Purifier.Id)
            {
                Purifier = true;
                return;
            }

            if (filter.Id == Strings.StatPoe1.Map.Baran.Id)
            {
                Baran = true;
                return;
            }
            if (filter.Id == Strings.StatPoe1.Map.Veritania.Id)
            {
                Veritania = true;
                return;
            }
            if (filter.Id == Strings.StatPoe1.Map.AlHezmin.Id)
            {
                AlHezmin = true;
                return;
            }
            if (filter.Id == Strings.StatPoe1.Map.Drox.Id)
            {
                Drox = true;
                return;
            }
        }
    }
}
