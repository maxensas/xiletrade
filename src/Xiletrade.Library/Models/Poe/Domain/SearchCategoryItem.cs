namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Item category of the trade site in the custom search : a category (weapon) or one of its sub-categories (bow).
/// </summary>
public sealed class SearchCategoryItem
{
    /// <summary>Id of the trade api (e.g. "weapon", "weapon.bow"), empty : any category.</summary>
    public string Id { get; init; }

    public string Label { get; init; }

    /// <summary>Sub-category : displayed under its category.</summary>
    public bool IsSub { get; init; }
}
