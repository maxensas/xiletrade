using System;
using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

internal static class BaseResultDataExtensions
{
    internal static BaseResultData FindBaseByName(this BaseResultData[] bases,
        ReadOnlySpan<char> name)
    {
        for (int i = 0; i < bases.Length; i++)
        {
            if (bases[i].Name.AsSpan().SequenceEqual(name))
                return bases[i];
        }
        return null;
    }

    internal static BaseResultData FindBaseByNameEn(this BaseResultData[] bases,
        ReadOnlySpan<char> nameEn)
    {
        for (int i = 0; i < bases.Length; i++)
        {
            if (bases[i].NameEn.AsSpan().SequenceEqual(nameEn))
                return bases[i];
        }
        return null;
    }

    internal static string GetLongestMatchingNameEn(this BaseResultData[] bases,
        ReadOnlySpan<char> type)
    {
        string longestName = string.Empty;

        foreach (var item in bases)
        {
            if (string.IsNullOrEmpty(item.NameEn))
                continue;

            if (item.Id.StartWith("Gems"))
                continue;

            if (!type.Contain(item.NameEn.AsSpan()))
                continue;

            if (longestName.Length > 0 || item.NameEn.Length > longestName.Length)
            {
                longestName = item.NameEn;
            }
        }

        return longestName;
    }

    internal static string GetLongestMatchingName(this BaseResultData[] bases,
        ReadOnlySpan<char> type)
    {
        string longestName = string.Empty;

        foreach (var item in bases)
        {
            if (string.IsNullOrEmpty(item.Name))
                continue;

            if (item.Id.StartWith("Gems"))
                continue;

            if (!type.Contain(item.Name.AsSpan()))
                continue;

            if (item.Name.Length > longestName.Length)
            {
                longestName = item.Name;
            }
        }

        return longestName;
    }

    internal static List<string> GetMatchingAffixesList(this BaseResultData[] mods,
        ReadOnlySpan<char> type)
    {
        var list = new List<string>();

        foreach (var mod in mods)
        {
            if (string.IsNullOrEmpty(mod.Name))
                continue;

            var parts = mod.Name.Split('/');

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                if (type.Contain(part.AsSpan()))
                {
                    list.Add(part);
                }
            }
        }
        return list;
    }

    public static BaseResultData FindMonsterByName(this BaseResultData[] monsters,
        ReadOnlySpan<char> monsterName, bool nospirit = false)
    {
        foreach (var monster in monsters)
        {
            if (monster.Name.AsSpan().Contain(monsterName))
            {
                if (nospirit && monster.NameEn.AsSpan().Contain("Spirit"))
                {
                    continue;
                }
                return monster;
            }
        }

        return null;
    }
}
