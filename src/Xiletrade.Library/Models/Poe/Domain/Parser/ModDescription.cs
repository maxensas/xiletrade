using System;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

// TODO: parse Implicit/Prefix/Suffix/Unique/.. in all languages and update affix flag.
// Keep current behaviour in order to work for both CTRL+C and CTRL+ALT+C item desc.
internal sealed class ModDescription
{
    internal bool IsParsed { get; private set; }
    internal string Kind { get; private set; } = string.Empty;
    internal string Tags { get; private set; } = string.Empty;
    internal string Name { get; private set; } = string.Empty;
    internal string Quality { get; private set; } = string.Empty;
    internal string Level { get; private set; } = string.Empty;
    internal int Tier { get; private set; } = -1;
    internal int AugmentPerCent { get; private set; } = -1;

    internal ModDescription()
    {

    }

    /// <summary>
    /// Class used to parse the "advanced" mod description before the mod line.
    /// </summary>
    /// <example>
    /// { Prefix Modifier "Cruel" (Tier: 6) — Damage, Physical, Attack }
    /// 139(135-154)% increased Physical Damage
    /// </example>
    internal ModDescription(DataManagerService dm, AffixFlag affix, bool impLogbook)
    {
        if (!(affix.ParsedData.StartsWith('{') && affix.ParsedData.EndsWith('}')))
        {
            return;
        }

        var affixOptions = affix.ParsedData.Split('—', StringSplitOptions.TrimEntries);

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

        Kind = impLogbook ? Resources.Resources.General073_ModifierImplicit
            : affixOptions[0].Replace(":", string.Empty).Trim(); // french version use ":"

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
