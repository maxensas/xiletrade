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
}
