namespace Xiletrade.Library.Models.Application.LiveSearch;

/// <summary>
/// State of the live searches, sent by the live search service (weak messenger) to the view models displaying it.
/// </summary>
/// <param name="IsRunning">A live search is listening.</param>
/// <param name="IsPaused">The live results are skipped, the connection is kept.</param>
public sealed record LiveSearchStateMessage(bool IsRunning, bool IsPaused);
