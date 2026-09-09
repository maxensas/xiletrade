using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain.Parser.Flag;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed record ItemState
{
    internal bool ExchangeCurrency { get; }
    internal bool SpecialBase { get; }
    internal bool ConquerorMap { get; }
    internal bool ImmutableSockets { get; }

    /// <summary>
    /// Record used to instantiate item state/flags after parsing steps.
    /// </summary>
    internal ItemState(DataManagerService dm, ItemFlag flag, ItemRule rule, List<ModLine> modList, ReadOnlySpan<char> type)
    {
        ExchangeCurrency = !flag.Tag.Unidentified && !flag.Map.IsMap && !flag.Waystones && !flag.Tablet && !flag.Tag.CapturedBeast
            && !flag.Wombgift && !flag.Incubator && !flag.Invitation
            && type.Length > 0 && dm.Currencies.FindEntryByType(type) is not null;
        SpecialBase = dm.Bases.FindBaseByName(type) is var findBase && findBase is not null
            && Strings.lSpecialBases.Contains(findBase.NameEn);
        if (!rule.Parseable)
        {
            return;
        }
        foreach (var mod in modList)
        {
            if (!ConquerorMap)
            {
                ConquerorMap = mod.ItemFilter.Id.StartWith(Strings.Stat.Option.MapOccupConq);
            }
            if (!ImmutableSockets)
            {
                ImmutableSockets = mod.ItemFilter.Id is Strings.Stat.SocketsUnmodifiable;
            }
        }
    }
}
