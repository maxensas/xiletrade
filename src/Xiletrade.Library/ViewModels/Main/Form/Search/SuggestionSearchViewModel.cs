using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Raffinert.FuzzySharp;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.ViewModels.Main.Form.Search;

/// <summary>
/// Suggestions of the item search of the custom search : names and types of the items file, in the language used.
/// </summary>
/// <remarks>
/// The best items are kept (texts starting with the query, then containing it, then the closest ones),
/// then displayed by category under a title, in the order of the items file : accessories, armours...
/// </remarks>
public sealed partial class SuggestionSearchViewModel : SuggestionListViewModel
{
    private const int MAX_SUGGESTIONS = 15;

    private readonly ConcurrentDictionary<string, Levenshtein> _levCache = new();

    private readonly IEnumerable<(string Text, bool IsUnique, string Category, int Group)> _sourceData;

    internal SuggestionSearchViewModel(IEnumerable<(string, bool, string, int)> sourceList)
    {
        _sourceData = sourceList;
    }

    // the item chosen is put in the field : the search is launched by the user
    protected override void Select(SearchItem item) => SetQuery(item.Text);

    protected override List<SearchItem> Find(string query)
    {
        var lev = _levCache.GetOrAdd(query, q => new Levenshtein(q.ToLower()));

        // best items, then by category (stable sort : the best first in each category)
        var best = _sourceData
            .Select(item =>
            {
                int score = item.Text.StartsWith(query, StringComparison.OrdinalIgnoreCase) ? 0 // max priority
                    : item.Text.Contains(query, StringComparison.OrdinalIgnoreCase) ? 1 // medium priority
                    : lev.DistanceFrom(item.Text) + 2; // Low priority, +2 to stay behind StartsWith/Contains
                return (item, score);
            })
            .OrderBy(x => x.score)
            .ThenBy(x => x.item.Text, StringComparer.Ordinal) // alphabetical sort if equal
            .Take(MAX_SUGGESTIONS).TakeWhile(x => x.score < 4)
            .OrderBy(x => x.item.Group);

        var list = new List<SearchItem>(MAX_SUGGESTIONS * 2);
        int lastGroup = -1;
        foreach (var (item, score) in best)
        {
            if (item.Group != lastGroup)
            {
                lastGroup = item.Group;
                list.Add(CreateHeader(item.Category));
            }
            var suggestion = Highlight(item.Text, query, score, new() { IsUnique = item.IsUnique });
            suggestion.Kind = item.Category;
            list.Add(suggestion);
        }
        return list;
    }
}
