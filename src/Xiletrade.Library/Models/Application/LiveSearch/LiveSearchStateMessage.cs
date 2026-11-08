namespace Xiletrade.Library.Models.Application.LiveSearch;

/// <summary>
/// State of the live searches, sent by the live search service (weak messenger) to the view models displaying it.
/// </summary>
/// <param name="Sessions">Live searches listening, oldest first.</param>
/// <param name="IsPaused">All the live searches paused (overlay), each one can be paused or resumed alone.</param>
public sealed record LiveSearchStateMessage(LiveSearchInfo[] Sessions, bool IsPaused)
{
    /// <summary>A live search is listening.</summary>
    public bool IsRunning => Sessions.Length > 0;
}
