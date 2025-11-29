using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fastenshtein;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class CustomSearchViewModel : ViewModelBase
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
    private AsyncObservableCollection<UniqueUnidentified> unidUniques;

    [ObservableProperty]
    private int unidUniquesIndex;

    [ObservableProperty]
    private string search;

    [ObservableProperty]
    private bool isDropdownOpen;

    public CustomSearchViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var dm = _serviceProvider.GetRequiredService<DataManagerService>();

        // lot of strings - TO UPDATE
        _sourceData = dm.Words.Select(x => x.Name).Concat(dm.Bases.Select(x => x.Name));

        PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(Search))
            {
                if (_suppressUpdate)
                {
                    _suppressUpdate = false;
                    return;
                }
                await UpdateSuggestionsAsync();
            }
        };

        var vm = _serviceProvider.GetRequiredService<MainViewModel>();
        vm.Result.Rate.ShowMin = false;
        vm.Result.Quick.RightString = Resources.Resources.Main245_SearchPreset;
        vm.Result.Quick.LeftString = string.Empty;

        var isPoe2 = dm.Config.Options.GameVersion is 1;
        var header = new UniqueUnidentified() { Name = Resources.Resources.Main249_SelectPreset };
        var list = dm.SearchPreset.UnidUnique.Where(x => x.Poe2 == isPoe2);

        if (dm.Config.Options.Language > 0)
        {
            foreach (var unid in list)
            {
                if (unid.Name?.Length > 0)
                {
                    var word = dm.Words.FirstOrDefault(x => x.NameEn == unid.Name);
                    if (word is not null)
                    {
                        unid.Name = word.Name;
                    }
                }
                if (unid.Type?.Length > 0)
                {
                    var word = dm.Bases.FirstOrDefault(x => x.NameEn == unid.Type);
                    if (word is not null)
                    {
                        unid.Type = word.Name;
                    }
                }
            }
        }
        unidUniques = [header, .. list];
        unidUniquesIndex = 0;
    }

    [RelayCommand]
    private void SelectSuggestion(object parameter)
    {
        if (SelectedSuggestion is null)
            return;

        _suppressUpdate = true;
        Search = SelectedSuggestion.FullText;

        IsDropdownOpen = false;

        Suggestions.Clear();

        var vm = _serviceProvider.GetRequiredService<MainViewModel>();
        vm.LaunchCustomSearch();
    }

    private async Task UpdateSuggestionsAsync()
    {
        string query = Search?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(query))
        {
            Suggestions.Clear();
            IsDropdownOpen = false;
            return;
        }

        var results = await Task.Run(() => SearchFastenshtein(query));

        // Update collection
        Suggestions.ReplaceRange(results);

        IsDropdownOpen = Suggestions.Count > 0;
    }

    private IEnumerable<SearchItem> SearchFastenshtein(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        var lev = _levCache.GetOrAdd(query, q => new Levenshtein(q.ToLower()));

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
                    score = lev.DistanceFrom(item) + 10; // Low priority, +10 to stay behind StartsWith/Contains
                }

                return new { item, score };
            })
            .OrderBy(x => x.score)
            .ThenBy(x => x.item) // Optional: alphabetical sort if equal
            .Take(7)
            .Select(x => Highlight(x.item, query));
    }

    private static SearchItem Highlight(string text, string query)
    {
        int index = text.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);

        if (index < 0)
        {
            return new SearchItem
            {
                Text = text,
                Before = text,
                Match = "",
                After = ""
            };
        }

        return new SearchItem
        {
            Text = text,
            Before = text[..index],
            Match = text.Substring(index, query.Length),
            After = text[(index + query.Length)..]
        };
    }
}
