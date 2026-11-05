using System;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Poe.Domain.Interface;

/// <summary>
/// Item as seen by the stat rules (StatRuleExtensions.Resolve) : flags, english unique name and english type.
/// </summary>
internal interface IStatRuleItem
{
    /// <summary>The item has this flag.</summary>
    bool HasFlag(StatRuleFlag flag);

    /// <summary>The item is the unique with this english name.</summary>
    bool IsUnique(ReadOnlySpan<char> nameEn);

    /// <summary>The english type (base) of the item is this one.</summary>
    bool IsTypeEn(ReadOnlySpan<char> typeEn);
}
