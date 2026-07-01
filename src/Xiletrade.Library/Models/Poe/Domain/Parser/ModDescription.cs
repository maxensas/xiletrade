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
    internal bool IsEnhance { get; private set; }
    internal bool IsImplicit { get; private set; }
    internal bool IsImplicitCorruption { get; private set; }
    internal bool IsCorruptionEnhance { get; private set; }
    internal bool IsImplicitEater { get; private set; }
    internal bool IsImplicitExarch { get; private set; }

    // prefixs
    internal bool IsPrefix { get; private set; }
    internal bool IsPrefixCraft { get; private set; }
    internal bool IsPrefixDesecrated { get; private set; }
    internal bool IsPrefixFractured { get; private set; }
    internal bool IsPrefixFracturedCraft { get; private set; }

    // suffixs
    internal bool IsSuffix { get; private set; }
    internal bool IsSuffixCraft { get; private set; }
    internal bool IsSuffixDesecrated { get; private set; }
    internal bool IsSuffixFractured { get; private set; }
    internal bool IsSuffixFracturedCraft { get; private set; }

    internal bool IsCorruption => IsImplicitCorruption || IsCorruptionEnhance;
    internal bool IsCraft => IsPrefixCraft || IsSuffixCraft || IsSuffixFracturedCraft || IsPrefixFracturedCraft;
    internal bool IsImplicitAny => IsImplicit || IsImplicitEater || IsImplicitExarch || IsImplicitCorruption;
    internal bool IsFractured => IsPrefixFractured || IsSuffixFractured || IsSuffixFracturedCraft || IsPrefixFracturedCraft;
    internal bool IsDesecrated => IsPrefixDesecrated || IsSuffixDesecrated;
    internal bool IsMutated => IsAffixUniqueFoulborn || IsAffixUniqueVaal;

    internal string TierKind => IsCraft && Tier > -1 ? Strings.AffixKind.EnchantAndCraft
        : IsCorruption ? Strings.AffixKind.Corruption
        : IsEnhance ? Strings.AffixKind.Enhance
        : IsImplicitAny ? Strings.AffixKind.Implicit
        : IsPrefix || IsPrefixCraft || IsPrefixDesecrated || IsPrefixFractured ? Strings.AffixKind.Prefix
        : IsSuffix || IsSuffixCraft || IsSuffixDesecrated || IsSuffixFractured ? Strings.AffixKind.Suffix
        : IsAffixUnique || IsMutated ? Strings.AffixKind.Unique : string.Empty;

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
        var idxLang = dm.Config.Options.Language;
        var isPoe2 = dm.Config.Options.GameVersion is 1;
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
        int idxOpening = opt.IndexOf('(');
        int idxClosing = opt.LastIndexOf(')');
        if (idxOpening >= 0 && idxClosing > idxOpening)
        {
            var bracketString = opt.Substring(idxOpening + 1, idxClosing - idxOpening - 1);
            if (bracketString.AsSpan().StartWith(Resources.Resources.General178_ModDescTier))
            {
                var match = RegexUtil.DecimalNoPlusPattern().Match(bracketString);
                if (match.Success && int.TryParse(match.Value, out int tier))
                {
                    Tier = tier;
                }
            }
            // to update
            var except = idxLang is 4 && bracketString is "Verderbtheit";
            if (!except)
            {
                // remove bracket string value
                affixOptions[0] = string.Concat(opt.AsSpan(0, idxOpening), opt.AsSpan(idxClosing + 1)).Trim();
            }
        }
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
        Kind = affixOptions[0];

        IsImplicit = Kind.StartWithAny(Resources.Resources.General073_ModifierImplicit);
        IsAffixUnique = Kind.StartWithAny(Resources.Resources.General079_ModifierUnique);

        IsPrefix = Kind.StartWithAny(Resources.Resources.General075_ModifierPrefix);
        IsSuffix = Kind.StartWithAny(Resources.Resources.General077_ModifierSuffix);

        IsPrefixFractured = Kind.StartWithAny(Resources.Resources.General172_ModifierFracturedPrefix);
        IsPrefixFracturedCraft = Kind.StartWithAny(Resources.Resources.General206_FracturedCraftedPrefixModifier);
        IsSuffixFractured = Kind.StartWithAny(Resources.Resources.General173_ModifierFracturedSuffix);
        IsSuffixFracturedCraft = Kind.StartWithAny(Resources.Resources.General207_FracturedCraftedSuffixModifier);
        if (isPoe2)
        {
            IsEnhance = Kind.StartWith(Resources.Resources.General203_Enhancement);
            IsCorruptionEnhance = Kind.StartWith(Resources.Resources.General199_CorruptionEnhance);
            IsPrefixDesecrated = Kind.StartWith(Resources.Resources.General169_ModifierDesecratedPrefix);
            IsSuffixDesecrated = Kind.StartWith(Resources.Resources.General168_ModifierDesecratedSuffix);
            IsAffixUniqueVaal = Kind.StartWith(Resources.Resources.General176_ModifierVaalUnique);
            IsPrefixCraft = Kind.StartWith(Resources.Resources.General204_CraftedPrefixModifier);
            IsSuffixCraft = Kind.StartWith(Resources.Resources.General205_CraftedSuffixModifier);
        }
        else
        {
            IsImplicitCorruption = Kind.StartWith(Resources.Resources.General074_ModifierCorrupt);
            IsImplicitEater = Kind.StartWith(Resources.Resources.General170_ModifierEaterImplicit);
            IsImplicitExarch = Kind.StartWith(Resources.Resources.General171_ModifierExarchImplicit);
            IsPrefixCraft = Kind.StartWith(Resources.Resources.General076_ModifierPrefixCraft);
            IsSuffixCraft = Kind.StartWith(Resources.Resources.General078_ModifierSuffixCraft);
            IsAffixUniqueFoulborn = Kind.StartWith(Resources.Resources.General175_ModifierFoulbornUnique);
        }

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
