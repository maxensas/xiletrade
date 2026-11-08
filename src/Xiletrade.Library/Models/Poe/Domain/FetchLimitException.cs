using System;

namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Price check refused : live searches are running and the fetch limits of the trade api are nearly reached.
/// </summary>
/// <remarks>The request which would exceed a limit is not sent : its penalty would stop the live searches too.</remarks>
public sealed class FetchLimitException(TimeSpan wait) : Exception(Resources.Resources.Main316_LiveFetchLimit)
{
    /// <summary>Time before the price check can be done.</summary>
    public TimeSpan Wait { get; } = wait;
}
