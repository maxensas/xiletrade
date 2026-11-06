namespace Xiletrade.Library.Shared.Enum;

/// <summary>
/// Group of a stat filter in a custom search, as the stat groups of the trade site.
/// </summary>
public enum StatGroup : byte
{
    /// <summary>The item has every stat of the group.</summary>
    And = 0,
    /// <summary>The item has none of the stats of the group.</summary>
    Not,
    /// <summary>The item has a number of the stats of the group, between the min and max of the group.</summary>
    Count
}
