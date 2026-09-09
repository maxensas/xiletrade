using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag
{
    internal sealed record ItemFlagOffhand
    {
        internal bool Quivers { get; }

        //armours
        internal bool Shield { get; }
        internal bool Bucklers { get; }
        internal bool Focus { get; }

        internal ItemFlagOffhand(ReadOnlySpan<char> itemClass)
        {
            Quivers = itemClass.Contain(Resources.Resources.ItemClass_quivers);

            Shield = itemClass.Contain(Resources.Resources.ItemClass_shields);
            Bucklers = itemClass.Contain(Resources.Resources.ItemClass_bucklers);
            Focus = itemClass.StartWith(Resources.Resources.ItemClass_foci);
        }
    }
}
