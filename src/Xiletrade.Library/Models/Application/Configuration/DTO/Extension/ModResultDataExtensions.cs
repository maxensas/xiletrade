using System;
using System.Collections.Generic;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

internal static class ModResultDataExtensions
{
    internal static List<string> GetMatchingAffixesList(this ModResultData[] mods,
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

    internal static List<string> GetMatchingAffixesEnList(this ModResultData[] mods,
        ReadOnlySpan<char> type)
    {
        var list = new List<string>();

        foreach (var mod in mods)
        {
            if (string.IsNullOrEmpty(mod.NameEn))
                continue;

            ReadOnlySpan<char> span = mod.NameEn.AsSpan();
            while (!span.IsEmpty)
            {
                int index = span.IndexOf('/');
                ReadOnlySpan<char> part = index is -1 ? span : span[..index];
                if (!part.IsEmpty && type.Contain(part))
                {
                    list.Add(part.ToString());
                    break;
                }

                if (index is -1)
                    break;

                span = span[(index + 1)..];
            }
        }
        return list;
    }

    internal static ModResultData FindModByName(this ModResultData[] mods,
        ReadOnlySpan<char> name, bool conjugation = false)
    {
        foreach (var mod in mods)
        {
            if (string.IsNullOrEmpty(mod.Name))
                continue;

            var modName = mod.Name.AsSpan();

            if (!conjugation)
            {
                if (modName.SequenceEqual(name))
                    return mod;

                continue;
            }

            // EN : "name_en": "Arcing"
            // FR : "name": "Amorçant/Amorçante/Amorçants/Amorçantes"
            while (true)
            {
                int separatorIndex = modName.IndexOf('/');

                if (separatorIndex < 0)
                {
                    // Last segment
                    if (modName.SequenceEqual(name))
                        return mod;

                    break;
                }

                var segment = modName.Slice(0, separatorIndex);
                if (segment.SequenceEqual(name))
                    return mod;

                modName = modName.Slice(separatorIndex + 1);
            }
        }
        return null;
    }
}
