using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raffinert.FuzzySharp;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main;

public partial class SuggestionSearchViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<string, Levenshtein> _levCache = new();

    private IEnumerable<string> _sourceData;

    private bool _suppressUpdate;

    [ObservableProperty]
    private AsyncObservableCollection<SearchItem> suggestions = new();

    [ObservableProperty]
    private SearchItem selectedSuggestion;

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private bool isDropdownOpen;

    public SuggestionSearchViewModel(IServiceProvider serviceProvider, IEnumerable<string> sourceList)
    {
        _serviceProvider = serviceProvider;
        _sourceData = sourceList;

        PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(SearchQuery))
            {
                if (_suppressUpdate)
                {
                    _suppressUpdate = false;
                    return;
                }
                await UpdateSuggestionsAsync();
            }
        };
    }

    [RelayCommand]
    private void SelectSuggestion(object parameter)
    {
        if (SelectedSuggestion is null)
            return;

        _suppressUpdate = true;
        SearchQuery = SelectedSuggestion.FullText;

        IsDropdownOpen = false;

        Suggestions.Clear();

        var vm = _serviceProvider.GetRequiredService<MainViewModel>();
        vm.LaunchCustomSearch();
    }

    private async Task UpdateSuggestionsAsync()
    {
        string query = SearchQuery?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(query))
        {
            Suggestions.Clear();
            IsDropdownOpen = false;
            return;
        }

        var results = await Task.Run(() => SearchLevenshtein(query));

        // Update collection
        Suggestions.ReplaceRange(results);

        IsDropdownOpen = Suggestions.Count > 0;
    }

    private IEnumerable<SearchItem> SearchLevenshtein(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        var lev = GetLevenshtein(query.ToLower());

        return _sourceData
            .Select(item =>
            {
                string text = item.ToLower();
                int score;

                if (text.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                {
                    score = 0; // max priority
                }
                else if (text.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    score = 1; // medium priority
                }
                else
                {
                    score = lev.DistanceFrom(item) + 2; // Low priority, +2 to stay behind StartsWith/Contains
                }

                return new { item, score };
            })
            .OrderBy(x => x.score)
            .ThenBy(x => x.item) // Optional: alphabetical sort if equal
            .Take(7).TakeWhile(x => x.score < 4)
            .Select(x => Highlight(x.item, query, x.score));
    }

    public Levenshtein GetLevenshtein(string query)
    {
        var key = query.ToLower();

        while (true)
        {
            if (_levCache.TryGetValue(key, out var existing))
                return existing;

            var created = new Levenshtein(key);

            if (_levCache.TryAdd(key, created))
                return created;

            // Someone else added one first -> dispose ours
            created.Dispose();
        }
    }

    private static SearchItem Highlight(string text, string query, int score)
    {
        int index = text.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);

        if (index < 0)
        {
            return new SearchItem
            {
                Score = score,
                Text = text,
                Before = text,
                Match = string.Empty,
                After = string.Empty
            };
        }

        return new SearchItem
        {
            Score = score,
            Text = text,
            Before = text[..index],
            Match = text.Substring(index, query.Length),
            After = text[(index + query.Length)..]
        };
    }
}
