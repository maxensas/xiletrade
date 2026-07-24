using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

internal static class LeagueResultExtension
{
    internal static bool HasEventLeague(this LeagueResult[] leagueList)
    {
        foreach (var league in leagueList)
        {
            if (league.Text.Contain('(') && league.Text.Contain(')') && league.Text.Contain("00"))
            {
                return true;
            }
        }
        return false;
    }
}
