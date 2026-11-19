using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.Models.Application.LiveSearch;

/// <summary>Live search listening, as displayed to the user.</summary>
/// <param name="Number">Number of the live search, from 1 to the maximum : displayed on its toasts.</param>
/// <param name="Label">Name of the preset or search text when it started, otherwise "Live" and its number.</param>
/// <param name="IsPaused">Its live results are skipped, the connection is kept.</param>
/// <param name="Criteria">Criteria of the custom search when it started, null : unknown.</param>
/// <param name="History">Last live results fetched, newest first.</param>
/// <param name="Url">Page of the live search on the trade site.</param>
public sealed record LiveSearchInfo(int Number, string Label, bool IsPaused, LiveSearchCriteria Criteria,
    AsyncObservableCollection<ResultListItemViewModel> History, string Url);
