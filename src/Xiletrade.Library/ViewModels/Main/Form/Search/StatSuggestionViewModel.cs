using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;

namespace Xiletrade.Library.ViewModels.Main.Form.Search;

/// <summary>
/// Suggestions of stats for the custom search, taken from the trade filters of the current language.
/// </summary>
/// <remarks>
/// Every word typed must be in the text of the stat ("modif" or "crit chance" for instance).
/// The best stats are kept (texts starting with the query, then containing it, then the others),
/// then displayed by kind under a title, in the order of the filters : pseudo, explicit, implicit...
/// </remarks>
public sealed partial class StatSuggestionViewModel : SuggestionListViewModel
{
    private const int MAX_SUGGESTIONS = 50;

    private readonly DataManagerService _dm;
    private readonly Action<SearchItem> _onSelected;
    private readonly Action _onCancel;

    internal StatSuggestionViewModel(DataManagerService dm, Action<SearchItem> onSelected, Action onCancel = null)
    {
        _dm = dm;
        _onSelected = onSelected;
        _onCancel = onCancel;
    }

    protected override void Select(SearchItem item)
    {
        _onSelected(item);
        SetQuery(string.Empty);
    }

    // the text typed is removed
    protected override void OnClosedOutside()
    {
        SetQuery(string.Empty);
        _onCancel?.Invoke();
    }

    protected override List<SearchItem> Find(string query)
    {
        var results = _dm.Filter?.Result;
        if (results is null)
        {
            return [];
        }

        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var found = new List<(FilterResultEntrie Entry, string Kind, int Group, int Score)>();
        for (int group = 0; group < results.Length; group++)
        {
            var result = results[group];
            if (result.Entries is null)
            {
                continue;
            }
            foreach (var entry in result.Entries)
            {
                var text = entry.Text;
                if (string.IsNullOrEmpty(text) || !ContainsAll(text, words))
                {
                    continue;
                }
                int score = text.StartsWith(query, StringComparison.OrdinalIgnoreCase) ? 0
                    : text.Contains(query, StringComparison.OrdinalIgnoreCase) ? 1 : 2;
                found.Add((entry, result.Label, group, score));
            }
        }

        // best suggestions, then by kind (stable sort : the best first in each kind)
        var best = found.OrderBy(x => x.Score).ThenBy(x => x.Entry.Text.Length)
            .Take(MAX_SUGGESTIONS).OrderBy(x => x.Group);

        // only the suggestions displayed are highlighted : the query, or its first word when the words are apart
        var list = new List<SearchItem>(MAX_SUGGESTIONS + results.Length);
        int lastGroup = -1;
        foreach (var x in best)
        {
            if (x.Group != lastGroup)
            {
                lastGroup = x.Group;
                list.Add(CreateHeader(x.Kind, x.Entry.Type));
            }
            var match = x.Score < 2 ? query : words[0];
            var item = Highlight(x.Entry.Text, match, x.Score, new());
            item.Id = x.Entry.ID;
            item.Kind = x.Kind;
            item.KindType = x.Entry.Type;
            list.Add(item);
        }
        return list;
    }

    private static bool ContainsAll(string text, string[] words)
    {
        foreach (var word in words)
        {
            if (!text.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }
}
