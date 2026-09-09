using System;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed record ItemFlagRarity
{
    internal bool Unique { get; }
    internal bool Rare { get; }
    internal bool Magic { get; }
    internal bool Normal { get; }

    internal ItemFlagRarity(ReadOnlySpan<char> itemRarity)
    {
        Unique = itemRarity.SequenceEqual(Resources.Resources.General006_Unique);
        Rare = itemRarity.SequenceEqual(Resources.Resources.General007_Rare);
        Magic = itemRarity.SequenceEqual(Resources.Resources.General008_Magic);
        Normal = itemRarity.SequenceEqual(Resources.Resources.General009_Normal);
    }
}
