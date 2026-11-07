using System;
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
    // closest items kept : distance of 0 or 1 from the query (score 2 or 3)
    private const int MAX_DISTANCE = 1;

    private readonly IEnumerable<(string Text, bool IsUnique, string Category, int Group)> _sourceData;

    internal SuggestionSearchViewModel(IEnumerable<(string, bool, string, int)> sourceList)
    {
        _sourceData = sourceList;
    }

    // the item chosen is put in the field : the search is launched by the user
    protected override void Select(SearchItem item) => SetQuery(item.Text);

    protected override List<SearchItem> Find(string query)
    {
        // one per query (not cached : a query is rarely typed twice)
        var lev = new Levenshtein(query.ToLower());

        // only the items kept are sorted : starting with the query, containing it, then the closest ones
        var found = new List<((string Text, bool IsUnique, string Category, int Group) item, int score)>();
        foreach (var item in _sourceData)
        {
            var text = item.Text;
            int index = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
            int score;
            if (index >= 0)
            {
                score = index is 0 ? 0 : 1; // max priority, then medium priority
            }
            else
            {
                // a length too different is too far, without computing the distance
                if (Math.Abs(text.Length - query.Length) > MAX_DISTANCE)
                {
                    continue;
                }
                int distance = lev.DistanceFrom(text);
                if (distance > MAX_DISTANCE)
                {
                    continue;
                }
                score = distance + 2; // low priority, +2 to stay behind the texts containing the query
            }
            found.Add((item, score));
        }

        // best items, then by category (stable sort : the best first in each category)
        var best = found
            .OrderBy(x => x.score)
            .ThenBy(x => x.item.Text, StringComparer.Ordinal) // alphabetical sort if equal
            .Take(MAX_SUGGESTIONS)
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
