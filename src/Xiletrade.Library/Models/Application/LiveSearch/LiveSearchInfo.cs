namespace Xiletrade.Library.Models.Application.LiveSearch;

/// <summary>Live search listening, as displayed to the user.</summary>
/// <param name="Number">Number of the live search, from 1 to the maximum : displayed on its toasts.</param>
/// <param name="Label">Name of the preset or search text when it started, otherwise "Live" and its number.</param>
/// <param name="IsPaused">Its live results are skipped, the connection is kept.</param>
public sealed record LiveSearchInfo(int Number, string Label, bool IsPaused);
