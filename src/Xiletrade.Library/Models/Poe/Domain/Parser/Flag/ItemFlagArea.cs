using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed record ItemFlagArea
{
    internal bool Chronicle { get; }
    internal bool Ultimatum { get; }
    internal bool Logbook { get; }
    internal bool SanctumResearch { get; }
    internal bool MirroredTablet { get; }
    internal bool TrialCoins { get; }

    // group
    internal bool IsArea { get; }

    internal ItemFlagArea(ReadOnlySpan<char> itemClass, ReadOnlySpan<char> itemType)
    {
        Logbook = itemClass.StartWith(Resources.Resources.ItemClass_expeditionLogbooks)
            || itemType.Contain(Resources.Resources.General094_Logbook);
        TrialCoins = itemClass.StartWith(Resources.Resources.ItemClass_trialCoins);
        Chronicle = itemType.Contain(Resources.Resources.General065_ChronicleAtzoatl);
        MirroredTablet = itemType.Contain(Resources.Resources.General108_MirroredTablet);
        Ultimatum = itemType.Contain(Resources.Resources.ItemClass_inscribedUltimatum);
        SanctumResearch = itemClass.Contain(Resources.Resources.ItemClass_sanctumResearch);

        IsArea = Chronicle || Ultimatum || Logbook || SanctumResearch || TrialCoins || MirroredTablet;
    }
}
