using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.ViewModels.Main.Form.Search;

/// <summary>
/// Stat group of the custom search, as on the trade site : and (stat filters), not, count (with its min / max).
/// </summary>
public sealed partial class StatGroupViewModel : ViewModelBase
{
    public StatGroup Type { get; }

    public string Title => Type switch
    {
        StatGroup.Not => Resources.Resources.Main268_StatGroupNot,
        StatGroup.Count => Resources.Resources.Main269_StatGroupCount,
        _ => Resources.Resources.Main267_StatGroupAnd
    };

    /// <summary>The count group has its own min / max : number of its stats the item must have.</summary>
    public bool IsCount => Type is StatGroup.Count;

    /// <summary>Unchecked : sent disabled, as on the trade site.</summary>
    [ObservableProperty]
    private bool enabled = true;

    [ObservableProperty]
    private string min = string.Empty;

    [ObservableProperty]
    private string max = string.Empty;

    [ObservableProperty]
    private AsyncObservableCollection<CustomStatViewModel> filters = new();

    /// <summary>Stats and field of the group displayed : a click on its title reduces or expands it.</summary>
    [ObservableProperty]
    private bool isExpanded = true;

    /// <summary>Stat suggestions of the field of the group : to add a stat, or to change the stat clicked.</summary>
    [ObservableProperty]
    private StatSuggestionViewModel stat;

    /// <summary>The field of the group takes the keyboard focus.</summary>
    [ObservableProperty]
    private bool isSearchFocused;

    // stat being changed, null : the stat selected is added
    private CustomStatViewModel _editing;

    // criteria of the group changed : checked, min / max, stats and their values
    private readonly Action _onChanged;

    /// <param name="onChanged">Criteria of the group changed by the user.</param>
    internal StatGroupViewModel(DataManagerService dm, StatGroup type, Action onChanged)
    {
        Type = type;
        _onChanged = onChanged;
        stat = new(dm, OnStatSelected, EndEdit);
    }

    partial void OnEnabledChanged(bool value) => _onChanged?.Invoke();

    partial void OnMinChanged(string value) => _onChanged?.Invoke();

    partial void OnMaxChanged(string value) => _onChanged?.Invoke();

    internal void AddStat(string id, string text, string kind, string kindType, string min, string max)
    {
        Filters.Add(new(id, text, kind, kindType, BeginEdit, _onChanged) { Min = min, Max = max });
    }

    private void OnStatSelected(SearchItem item)
    {
        if (item?.Id is null)
        {
            return;
        }
        if (_editing is not null)
        {
            _editing.Replace(item);
            EndEdit();
            _onChanged?.Invoke();
            return;
        }
        Filters.Add(new(item.Id, item.Text, item.Kind, item.KindType, BeginEdit, _onChanged));
        _onChanged?.Invoke();
    }

    // the text of the stat in the field of the group : its suggestions are displayed
    private void BeginEdit(CustomStatViewModel statFilter)
    {
        EndEdit();
        _editing = statFilter;
        statFilter.IsEditing = true;
        Stat.SearchQuery = statFilter.Text;
        IsSearchFocused = false;
        IsSearchFocused = true;
    }

    private void EndEdit()
    {
        if (_editing is not null)
        {
            _editing.IsEditing = false;
            _editing = null;
        }
    }

    [RelayCommand]
    private void RemoveStat(object commandParameter)
    {
        if (commandParameter is CustomStatViewModel statFilter)
        {
            if (statFilter == _editing)
            {
                EndEdit();
            }
            Filters.Remove(statFilter);
            _onChanged?.Invoke();
        }
    }

    // enter key in the field : first suggestion
    [RelayCommand]
    private void StatSearch(object commandParameter) => Stat.SelectFirst();
}
