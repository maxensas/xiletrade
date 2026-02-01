using System;
using System.Text;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

internal sealed record ModInfoParse : ModInfo
{
    internal string ParsedMod { get; }
    internal bool IsNegative { get; }

    /// <summary>
    /// Parse Static And Negative Mod
    /// </summary>
    /// <param name="dm"></param>
    /// <param name="mod"></param>
    internal ModInfoParse(DataManagerService dm, string mod) : base(dm, mod)
    {
        ParsedMod = mod;

        var reduced = Resources.Resources.General102_reduced.Split('/');
        var increased = Resources.Resources.General101_increased.Split('/');
        if (reduced.Length != increased.Length)
        {
            return;
        }

        for (int j = 0; j < reduced.Length; j++)
        {
            if (!ModKind.Contain(reduced[j]))
            {
                continue;
            }
            if (!IsKindFilter) // mod with reduced stat not found
            {
                string modIncreased = ModKind.Replace(reduced[j], increased[j]);
                if (IsFilterMod(modIncreased)) // mod with increased stat found
                {
                    IsNegative = true;
                    if (Match.Count > 0)
                    {
                        StringBuilder sbInc = new(modIncreased);
                        sbInc.Replace(reduced[j], increased[j]);
                        for (int i = 0; i < Match.Count; i++)
                        {
                            int idx = sbInc.ToString().IndexOf('#', StringComparison.Ordinal);
                            if (idx > -1)
                            {
                                sbInc.Replace("#", "-" + Match[i].Value, idx, 1);
                                ParsedMod = sbInc.ToString();
                            }
                        }
                    }
                    ModKind = modIncreased.Replace("#", "-#");
                    return;
                }
            }
        }
    }
}