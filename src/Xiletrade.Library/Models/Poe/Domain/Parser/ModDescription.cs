using System;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed record ModDescription
{
    internal bool IsParsed { get; private set; }
    internal string Kind { get; private set; }
    internal string Tags { get; private set; }
    internal string Name { get; private set; }
    internal string Quality { get; private set; }
    internal string Level { get; private set; }
    internal int Tier { get; private set; } = -1;
    internal int AugmentPerCent { get; private set; } = -1;

    // unique
    internal bool IsAffixUnique { get; private set; }
    internal bool IsAffixUniqueFoulborn { get; private set; } // poe1
    internal bool IsAffixUniqueVaal { get; private set; } // poe2

    // implicits
    internal bool IsImplicit { get; private set; }
    internal bool IsImplicitCorruption { get; private set; }
    internal bool IsImplicitEater { get; private set; }
    internal bool IsImplicitExarch { get; private set; }

    // prefixs
    internal bool IsPrefix { get; private set; }
    internal bool IsPrefixCraft { get; private set; }
    internal bool IsPrefixDesecrated { get; private set; }
    internal bool IsPrefixFractured { get; private set; }

    // suffixs
    internal bool IsSuffix { get; private set; }
    internal bool IsSuffixCraft { get; private set; }
    internal bool IsSuffixDesecrated { get; private set; }
    internal bool IsSuffixFractured { get; private set; }

    internal bool IsCraft => IsPrefixCraft || IsSuffixCraft;
    internal bool IsImplicitAny => IsImplicit || IsImplicitCorruption || IsImplicitEater || IsImplicitExarch;
    internal bool IsFractured => IsPrefixFractured || IsSuffixFractured;
    internal bool IsDesecrated => IsPrefixDesecrated || IsSuffixDesecrated;

    internal string TierKind => IsCraft && Tier > -1 ? Strings.TierKind.EnchantAndCraft
            : IsImplicitAny ? Strings.TierKind.Implicit
            : IsPrefix || IsPrefixCraft || IsPrefixDesecrated || IsPrefixFractured ? Strings.TierKind.Prefix
            : IsSuffix || IsSuffixCraft || IsSuffixDesecrated || IsSuffixFractured ? Strings.TierKind.Suffix
            : IsAffixUnique ? Strings.TierKind.Unique : string.Empty;

    /// <summary>
    /// Class used to parse the "advanced" mod description before the mod line.
    /// </summary>
    /// <example>
    /// { Prefix Modifier "Cruel" (Tier: 6) — Damage, Physical, Attack }
    /// 139(135-154)% increased Physical Damage
    /// </example>
    internal ModDescription(DataManagerService dm, ReadOnlySpan<char> data)
    {
        if (!(data.StartsWith('{') && data.EndsWith('}')))
        {
            return;
        }

        var affixOptions = data.SplitTrimToArray('—');

        for (int i = 0; i < affixOptions.Length; i++)
        {
            var str = affixOptions[i];
            var filtered = new char[str.Length];
            int pos = 0;

            foreach (var c in str)
            {
                if (c is not '{' && c is not '}')
                    filtered[pos++] = c;
            }

            affixOptions[i] = new string(filtered, 0, pos).Trim();
        }

        var opt = affixOptions[0];
        int idx1 = opt.IndexOf('(');
        int idx2 = opt.LastIndexOf(')');
        if (idx1 >= 0 && idx2 > idx1)
        {
            bool isChinese = dm.Config.Options.Language is 8 or 9;
            char separator = isChinese ? '：' : ':';

            var tierString = opt.Substring(idx1 + 1, idx2 - idx1 - 1);
            if (tierString.Contains(separator))
            {
                var match = RegexUtil.DecimalNoPlusPattern().Match(tierString);
                if (match.Success && int.TryParse(match.Value, out int tier))
                {
                    Tier = tier;
                }
            }
            affixOptions[0] = string.Concat(opt.AsSpan(0, idx1), opt.AsSpan(idx2 + 1)).Trim();
        }
        var idxLang = dm.Config.Options.Language;
        char[] splitChars = idxLang is 10 ? ['「', '」'] : idxLang is 2 ? ['«', '»'] : ['"']; 
        var affixOpt = affixOptions[0].Split(splitChars);
        if (affixOpt.Length is 3)
        {
            Name = affixOpt[1].Trim();

            affixOptions[0] = (affixOpt[0] + affixOpt[2]).Trim();

            var conjugation = idxLang is 2 or 3 or 4 or 5 or 6;
            var entry = dm.Mods.FindModByName(Name, conjugation);
            if (entry is not null)
            {
                Level = entry.Level;
            }
        }
        //impLogbook ? Resources.Resources.General073_ModifierImplicit
        Kind = affixOptions[0].Replace(":", string.Empty).Trim(); // french version use ":"

        IsImplicit = Kind.StartWith(Resources.Resources.General073_ModifierImplicit);
        IsImplicitCorruption = Kind.StartWith(Resources.Resources.General074_ModifierCorrupt);
        IsImplicitEater = Kind.StartWith(Resources.Resources.General170_ModifierEaterImplicit);
        IsImplicitExarch = Kind.StartWith(Resources.Resources.General171_ModifierExarchImplicit);
        IsPrefix = Kind.StartWith(Resources.Resources.General075_ModifierPrefix);
        IsPrefixCraft = Kind.StartWith(Resources.Resources.General076_ModifierPrefixCraft);
        IsPrefixDesecrated = Kind.StartWith(Resources.Resources.General169_ModifierDesecratedPrefix);
        IsPrefixFractured = Kind.StartWith(Resources.Resources.General172_ModifierFracturedPrefix);
        IsSuffix = Kind.StartWith(Resources.Resources.General077_ModifierSuffix);
        IsSuffixCraft = Kind.StartWith(Resources.Resources.General078_ModifierSuffixCraft);
        IsSuffixDesecrated = Kind.StartWith(Resources.Resources.General168_ModifierDesecratedSuffix);
        IsSuffixFractured = Kind.StartWith(Resources.Resources.General173_ModifierFracturedSuffix);
        IsAffixUnique = Kind.StartWith(Resources.Resources.General079_ModifierUnique);
        IsAffixUniqueFoulborn = Kind.StartWith(Resources.Resources.General175_ModifierFoulbornUnique);
        IsAffixUniqueVaal = Kind.StartWith(Resources.Resources.General176_ModifierVaalUnique);

        if (affixOptions.Length > 1)
        {
            Tags = affixOptions[1];
        }
        if (affixOptions.Length > 2)
        {
            Quality = affixOptions[2];
            var match = RegexUtil.DecimalNoPlusPattern().Matches(Quality);
            if (match.Count > 0)
            {
                _ = int.TryParse(match[0].Value, out int quality);
                if (quality > 0)
                {
                    AugmentPerCent = quality;
                }
            }
        }
        IsParsed = true;
    }
}
