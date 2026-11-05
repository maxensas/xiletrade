using System;
using System.IO;
using Xiletrade.Library.Models.Poe.Domain.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

/// <summary>
/// Resolution of the id sent to the trade api for a filter entry, with the rules of StatRules.json / StatRulesTwo.json.
/// The entries of the filters are never modified : the resolved id is returned.
/// </summary>
/// <remarks>Format of the files : Data/StatRules.md.</remarks>
internal static class StatRuleExtensions
{
    internal const string ALL = "all";
    internal const string FIRST = "first";
    internal const string UNIQUE = "unique";
    internal const string TYPE_EN = "typeEn";
    internal const string ENTRY_TYPE = "entryType";

    /// <summary>
    /// Returns the id to use for this entry and this item, <paramref name="skip"/> is true when the entry must not be used.
    /// </summary>
    internal static string Resolve<TItem>(this StatRule rules, string id, string entryType, TItem item, out bool skip)
        where TItem : IStatRuleItem
    {
        skip = false;
        if (rules is not null)
        {
            ApplyRules(rules, ref id, ref skip, entryType, item);
        }
        return id;
    }

    // a rule applies when its id conditions and its item conditions are true
    private static bool Apply<TItem>(StatRule rule, ref string id, ref bool skip, string entryType, TItem item)
        where TItem : IStatRuleItem
    {
        if (!IsMatch(rule.Match, id) || !IsTrue(rule.When, entryType, item))
        {
            return false;
        }
        if (rule.Id is not null)
        {
            id = rule.Id;
        }
        if (rule.SuffixId is not null)
        {
            id = string.Concat(id.AsSpan().FirstPartIncluding('.'), rule.SuffixId);
        }
        if (rule.Skip)
        {
            skip = true; // nothing can undo a skip : the resolution stops
            return true;
        }
        ApplyRules(rule, ref id, ref skip, entryType, item);
        return true;
    }

    // "all" : every child rule is tried, each one on the id left by the previous ones.
    // "first" : the first child rule that applies is the only one used.
    private static void ApplyRules<TItem>(StatRule rule, ref string id, ref bool skip, string entryType, TItem item)
        where TItem : IStatRuleItem
    {
        if (rule.Rules is null)
        {
            return;
        }
        bool first = rule.Mode is FIRST;
        foreach (var child in rule.Rules)
        {
            bool applied = Apply(child, ref id, ref skip, entryType, item);
            if (skip || applied && first)
            {
                return;
            }
        }
    }

    private static bool IsMatch(StatRuleMatch match, ReadOnlySpan<char> id)
    {
        if (match is null)
        {
            return true;
        }
        if (id.Length < match.MinLength)
        {
            return false;
        }
        if (match.Ids is not null && !AnyEqual(match.Ids, id))
        {
            return false;
        }
        if (match.Suffix is not null && !AnyEndsWith(match.Suffix, id))
        {
            return false;
        }
        if (match.Contains is not null && !AnyContained(match.Contains, id))
        {
            return false;
        }
        if (match.NotContains is not null && AnyContained(match.NotContains, id))
        {
            return false;
        }
        if (match.Stat is not null && (!TryGetStatPart(id, out var stat) || !AnyEqual(match.Stat, stat)))
        {
            return false;
        }
        return true;
    }

    private static bool IsTrue<TItem>(string[] when, string entryType, TItem item)
        where TItem : IStatRuleItem
    {
        if (when is null)
        {
            return true;
        }
        foreach (var condition in when)
        {
            if (!Check(condition, entryType, item))
            {
                return false;
            }
        }
        return true;
    }

    // "flag", "unique:<english name>", "typeEn:<english type>", "entryType:<type>", each one can be negated with '!'
    private static bool Check<TItem>(ReadOnlySpan<char> condition, string entryType, TItem item)
        where TItem : IStatRuleItem
    {
        bool negate = condition.Length > 0 && condition[0] is '!';
        if (negate)
        {
            condition = condition[1..];
        }

        bool result;
        int colon = condition.IndexOf(':');
        if (colon < 0)
        {
            result = item.HasFlag(ParseFlag(condition));
        }
        else
        {
            var value = condition[(colon + 1)..];
            result = condition[..colon] switch
            {
                UNIQUE => item.IsUnique(value),
                TYPE_EN => item.IsTypeEn(value),
                ENTRY_TYPE => value.SequenceEqual(entryType),
                _ => false // unknown names are rejected when the rules are loaded
            };
        }
        return negate ? !result : result;
    }

    // part between the first and the second '.' : "explicit.stat_123" -> "stat_123"
    private static bool TryGetStatPart(ReadOnlySpan<char> id, out ReadOnlySpan<char> stat)
    {
        int dot = id.IndexOf('.');
        if (dot < 0)
        {
            stat = default;
            return false;
        }
        stat = id[(dot + 1)..];
        int next = stat.IndexOf('.');
        if (next >= 0)
        {
            stat = stat[..next];
        }
        return true;
    }

    private static bool AnyEqual(string[] values, ReadOnlySpan<char> id)
    {
        foreach (var value in values)
        {
            if (id.SequenceEqual(value))
            {
                return true;
            }
        }
        return false;
    }

    private static bool AnyEndsWith(string[] values, ReadOnlySpan<char> id)
    {
        foreach (var value in values)
        {
            if (id.EndsWith(value, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private static bool AnyContained(string[] values, ReadOnlySpan<char> id)
    {
        foreach (var value in values)
        {
            if (id.Contains(value, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks the rules once loaded : a mistake in the file (unknown mode, flag or condition) stops the loading
    /// with its location, instead of a rule silently never applied.
    /// </summary>
    /// <exception cref="InvalidDataException"></exception>
    internal static void Validate(this StatRule rule, string fileName, string path = "root")
    {
        if (rule.Mode is not null and not ALL and not FIRST)
        {
            throw new InvalidDataException($"{fileName}, {path} : unknown mode '{rule.Mode}' (expected '{ALL}' or '{FIRST}').");
        }
        if (rule.When is not null)
        {
            foreach (var condition in rule.When)
            {
                if (!IsKnownCondition(condition))
                {
                    throw new InvalidDataException($"{fileName}, {path} : unknown condition '{condition}' (expected a flag "
                        + $"(see StatRuleExtensions.{nameof(ParseFlag)}), or '{UNIQUE}:', '{TYPE_EN}:', '{ENTRY_TYPE}:' followed by a value, '!' to negate).");
                }
            }
        }
        if (rule.Rules is not null)
        {
            for (int i = 0; i < rule.Rules.Length; i++)
            {
                rule.Rules[i].Validate(fileName, $"{path}.rules[{i}]");
            }
        }
    }

    private static bool IsKnownCondition(ReadOnlySpan<char> condition)
    {
        if (condition.Length > 0 && condition[0] is '!')
        {
            condition = condition[1..];
        }
        int colon = condition.IndexOf(':');
        if (colon < 0)
        {
            return ParseFlag(condition) is not StatRuleFlag.None;
        }
        return colon < condition.Length - 1 && condition[..colon] is UNIQUE or TYPE_EN or ENTRY_TYPE;
    }

    /// <summary>
    /// Flag of a 'when' condition : the only place where the flag names of the files are written.
    /// </summary>
    /// <returns><see cref="StatRuleFlag.None"/> for an unknown name (rejected by <see cref="Validate"/>).</returns>
    private static StatRuleFlag ParseFlag(ReadOnlySpan<char> name) => name switch
    {
        "weapon" => StatRuleFlag.Weapon,
        "armour" => StatRuleFlag.Armour,
        "unique" => StatRuleFlag.Unique,
        "jewel" => StatRuleFlag.Jewel,
        "cluster" => StatRuleFlag.Cluster,
        "tincture" => StatRuleFlag.Tincture,
        "charm" => StatRuleFlag.Charm,
        "flask" => StatRuleFlag.Flask,
        "amulet" => StatRuleFlag.Amulet,
        "shield" => StatRuleFlag.Shield,
        "waystone" => StatRuleFlag.Waystone,
        "tablet" => StatRuleFlag.Tablet,
        "chronicle" => StatRuleFlag.Chronicle,
        "logbook" => StatRuleFlag.Logbook,
        "sanctumRelic" => StatRuleFlag.SanctumRelic,
        _ => StatRuleFlag.None
    };
}
