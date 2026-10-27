using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Shared;

/// <summary>
/// Fixes applied to the data downloaded from the trade website (leagues, filters), before it is saved.
/// </summary>
/// <remarks>
/// Shared by <see cref="Services.DataUpdaterService"/> and the DatToJson tool, so both write the same files.
/// </remarks>
internal static class TradeDataRules
{
    /// <summary>Fixes the downloaded filters, returns null when there is nothing to fix.</summary>
    /// <remarks>Removes the line tabulations of the portuguese filters.</remarks>
    internal static byte[] FixFilterJson(ReadOnlySpan<byte> json, int idxLang)
    {
        // remove line tabulation (portuguese)
        if (idxLang is not 5)
        {
            return null;
        }

        ReadOnlySpan<byte> tabulation = "\\u000b"u8;
        int index = json.IndexOf(tabulation);
        if (index < 0)
        {
            return null;
        }

        var result = new byte[json.Length];
        int length = 0;
        while (index >= 0)
        {
            json[..index].CopyTo(result.AsSpan(length));
            length += index;
            json = json[(index + tabulation.Length)..];
            index = json.IndexOf(tabulation);
        }
        json.CopyTo(result.AsSpan(length));
        length += json.Length;
        return result.AsSpan(0, length).ToArray();
    }

    /// <summary>Sorts the entries of every group: needed for files comparison following leagues updates.</summary>
    internal static FilterData SortFilterEntries(FilterData filter)
    {
        if (filter.Result.Length > 0)
        {
            for (int i = 0; i < filter.Result.Length; i++)
            {
                filter.Result[i].Entries = [.. filter.Result[i].Entries.OrderBy(x => x.ID)];
            }
        }
        return filter;
    }

    /// <summary>Removes the leagues listed twice, returns null when there is no league.</summary>
    internal static LeagueData RemoveDuplicateLeagues(LeagueData leagues)
    {
        List<LeagueResult> leagueList = new();
        foreach (var league in leagues.Result)
        {
            if (!leagueList.Exists(x => x.Id == league.Id))
            {
                leagueList.Add(league);
            }
        }
        if (leagueList.Count is 0)
        {
            return null;
        }
        leagues.Result = [.. leagueList];
        return leagues;
    }
}
