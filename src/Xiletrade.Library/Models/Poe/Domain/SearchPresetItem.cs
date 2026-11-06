using Xiletrade.Library.Models.Application.Configuration.DTO;

namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Entry of the preset list of the custom search : header, search preset, separator or unidentified unique.
/// </summary>
public sealed class SearchPresetItem
{
    public string Name { get; init; }

    public string Image { get; init; }

    /// <summary>Line between the search presets and the unidentified uniques, not selectable.</summary>
    public bool IsSeparator { get; init; }

    internal UniqueUnidentified Unid { get; init; }

    internal CustomSearchPreset Preset { get; init; }
}
