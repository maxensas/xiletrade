using Xiletrade.Library.Models.Application.Configuration.DTO;

namespace Xiletrade.Library.Models.Application.LiveSearch;

/// <summary>
/// Criteria of the custom search when a live search started : given back to the custom search by the dot of the live search.
/// </summary>
public sealed class LiveSearchCriteria
{
    /// <summary>Criteria of the form, as a search preset without name (game included).</summary>
    internal CustomSearchPreset Search { get; init; }

    /// <summary>Text of the search field.</summary>
    internal string Text { get; init; }

    /// <summary>Search preset selected and not changed, null : none.</summary>
    internal CustomSearchPreset Preset { get; init; }

    /// <summary>Unidentified unique selected, null : none.</summary>
    internal UniqueUnidentified Unid { get; init; }
}
