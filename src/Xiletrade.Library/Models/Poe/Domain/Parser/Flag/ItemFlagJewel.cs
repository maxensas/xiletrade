using System;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser.Flag;

internal sealed record ItemFlagJewel
{
    internal bool IsJewel { get; }
    internal bool Cluster { get; }
    internal bool Cobalt { get; }
    internal bool Crimson { get; }
    internal bool Viridian { get; }
    internal bool Sapphire { get; }
    internal bool Ruby { get; }
    internal bool Emerald { get; }
    internal bool Prismatic { get; }
    internal bool Timeless { get; }

    internal bool Murderous { get; }
    internal bool Searching { get; }
    internal bool Hypnotic { get; }
    internal bool Ghastly { get; }

    internal bool ClusterLarge { get; }
    internal bool ClusterMedium { get; }
    internal bool ClusterSmall { get; }

    internal ItemFlagJewel(ReadOnlySpan<char> itemClass, ReadOnlySpan<char> itemType)
    {
        IsJewel = itemClass.Contain(Resources.Resources.ItemClass_jewels);

        if (!IsJewel)
        {
            return;
        }

        Cluster = itemType.Contain(Resources.Resources.General022_Cluster);

        if (Cluster)
        {
            ClusterLarge = itemType.Contain(Resources.Resources.General191_ClusterLarge);
            ClusterMedium = itemType.Contain(Resources.Resources.General192_ClusterMedium);
            ClusterSmall = itemType.Contain(Resources.Resources.General193_ClusterSmall);
            return;
        }

        //poe2
        Sapphire = itemType.Contain(Resources.Resources.General182_Sapphire);
        Ruby = itemType.Contain(Resources.Resources.General183_Ruby);
        Emerald = itemType.Contain(Resources.Resources.General184_Emerald);

        //poe1
        Cobalt = itemType.Contain(Resources.Resources.General179_Cobalt);
        Crimson = itemType.Contain(Resources.Resources.General180_Crimson);
        Viridian = itemType.Contain(Resources.Resources.General181_Viridian);

        Prismatic = itemType.Contain(Resources.Resources.General185_PrismaticJewel);
        Timeless = itemType.Contain(Resources.Resources.General186_TimelessJewel);

        Murderous = itemType.Contain(Resources.Resources.General187_MurderousJewel);
        Searching = itemType.Contain(Resources.Resources.General188_SearchingJewel);
        Hypnotic = itemType.Contain(Resources.Resources.General189_HypnoticJewel);
        Ghastly = itemType.Contain(Resources.Resources.General190_GhastlyJewel);
    }
}
