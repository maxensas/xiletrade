using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main.Form.Search;

/// <summary>
/// Field of the custom search with its list of suggestions, displayed by group under a title (not selectable).
/// </summary>
/// <remarks>
/// The suggestions are searched outside the UI thread once the typing pauses, the results of an older query are dropped.
/// </remarks>
public abstract partial class SuggestionListViewModel : ViewModelBase
{
    // pause of the typing before searching : the keys typed quickly do not each start a search
    private const int TypingDelayMs = 120;

    // the list is opened or closed by the suggestions, not by a click outside it
    private bool _isUpdating;

    // text set by a selection : no suggestion searched
    private bool _suppressUpdate;

    // incremented by each query : results of an older query are dropped
    private int _version;

    [ObservableProperty]
    private AsyncObservableCollection<SearchItem> suggestions = new();

    [ObservableProperty]
    private SearchItem selectedSuggestion;

    [ObservableProperty]
    private string searchQuery;

    [ObservableProperty]
    private bool isDropdownOpen;

    /// <summary>Suggestions of the query (not empty, trimmed) with the titles of their groups.</summary>
    /// <remarks>Called outside the UI thread.</remarks>
    protected abstract List<SearchItem> Find(string query);

    /// <summary>Suggestion chosen, never a title.</summary>
    protected abstract void Select(SearchItem item);

    /// <summary>List closed by a click outside it, without choosing a suggestion.</summary>
    protected virtual void OnClosedOutside() { }

    partial void OnSearchQueryChanged(string value)
    {
        if (!_suppressUpdate)
        {
            UpdateSuggestionsAsync(value);
        }
    }

    partial void OnIsDropdownOpenChanged(bool value)
    {
        if (!value && !_isUpdating)
        {
            OnClosedOutside();
        }
    }

    [RelayCommand]
    private void SelectSuggestion(object parameter)
    {
        if (SelectedSuggestion is { IsHeader: false } item)
        {
            Select(item);
        }
    }

    /// <summary>Selects the first suggestion (enter key).</summary>
    internal void SelectFirst()
    {
        var first = Suggestions.FirstOrDefault(x => !x.IsHeader);
        if (first is not null)
        {
            Select(first);
        }
    }

    /// <summary>Text of the field without searching its suggestions : the list is closed.</summary>
    protected void SetQuery(string text)
    {
        _version++; // a search in progress is dropped
        _suppressUpdate = true;
        try
        {
            SearchQuery = text;
        }
        finally
        {
            _suppressUpdate = false;
        }
        Suggestions.Clear();
        SetDropdown(false);
    }

    private void SetDropdown(bool isOpen)
    {
        _isUpdating = true;
        IsDropdownOpen = isOpen;
        _isUpdating = false;
    }

    private async void UpdateSuggestionsAsync(string query)
    {
        var version = ++_version;
        query = query?.Trim() ?? string.Empty;
        try
        {
            if (query.Length is 0)
            {
                Suggestions.Clear();
                SetDropdown(false);
                return;
            }

            await Task.Delay(TypingDelayMs);
            if (version != _version)
            {
                return; // another key typed meanwhile
            }

            var results = await Task.Run(() => Find(query));
            if (version != _version)
            {
                return;
            }
            Suggestions.ReplaceRange(results);
            SetDropdown(Suggestions.Count > 0);
        }
        catch (Exception)
        {
            // no suggestion : the text can still be typed again
            Suggestions.Clear();
            SetDropdown(false);
        }
    }

    /// <summary>Title of a group of suggestions, not selectable.</summary>
    protected static SearchItem CreateHeader(string kind, string kindType = null)
        => new() { Flag = new(), IsHeader = true, Kind = kind, KindType = kindType, Text = kind };

    /// <summary>Suggestion with the text matched highlighted, the whole text when it is not found.</summary>
    protected static SearchItem Highlight(string text, string match, int score, SearchFlag flag)
    {
        int index = match.Length > 0 ? text.IndexOf(match, StringComparison.OrdinalIgnoreCase) : -1;
        if (index < 0)
        {
            return new() { Flag = flag, Score = score, Text = text, Before = text, Match = string.Empty, After = string.Empty };
        }
        return new()
        {
            Flag = flag,
            Score = score,
            Text = text,
            Before = text[..index],
            Match = text.Substring(index, match.Length),
            After = text[(index + match.Length)..]
        };
    }
}
