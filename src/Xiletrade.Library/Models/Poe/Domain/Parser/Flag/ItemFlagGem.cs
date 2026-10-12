using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed record ItemFlagGem
{
    internal bool Skill { get; }
    internal bool Support { get; }
    internal bool Uncut { get; }

    // group
    internal bool IsGem { get; }

    internal ItemFlagGem(ReadOnlySpan<char> itemClass)
    {
        Skill = itemClass.StartWith(Resources.Resources.ItemClass_skillGems);
        Support = itemClass.StartWith(Resources.Resources.ItemClass_supportGems);
        Uncut = itemClass.Contain(Resources.Resources.General151_UncutSpiritGem)
            || itemClass.Contain(Resources.Resources.General152_UncutSkillGem)
            || itemClass.Contain(Resources.Resources.General153_UncutSupportGem);

        IsGem = !Uncut && (Skill || Support);
    }
}
