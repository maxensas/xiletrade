using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Xiletrade.Library.Models.Poe.Domain;

namespace Xiletrade.Library.ViewModels.Main.Form.Search;

/// <summary>
/// Stat filter of a stat group of the custom search : stat of the trade filters with its min / max.
/// </summary>
/// <remarks>A click on the stat puts its text in the field of the group : the stat selected replaces it.</remarks>
public sealed partial class CustomStatViewModel : ViewModelBase
{
    private readonly Action<CustomStatViewModel> _onEdit;
    private readonly Action _onChanged;

    /// <summary>Id of the stat sent to the trade api.</summary>
    internal string Id { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    private string text;

    /// <summary>Label of the kind of stat in the filters (Pseudo, Explicit, Enchant...), in the language used.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToolTip))]
    private string kind;

    /// <summary>Kind of stat of the trade api (pseudo, explicit, enchant...), used for its color.</summary>
    [ObservableProperty]
    private string kindType;

    public string ToolTip => Kind + " : " + Text;

    /// <summary>Unchecked : sent disabled, as on the trade site.</summary>
    [ObservableProperty]
    private bool enabled = true;

    [ObservableProperty]
    private string min = string.Empty;

    [ObservableProperty]
    private string max = string.Empty;

    /// <summary>The stat is being changed with the field of its group.</summary>
    [ObservableProperty]
    private bool isEditing;

    /// <param name="onChanged">Checked, min or max changed by the user.</param>
    internal CustomStatViewModel(string id, string text, string kind, string kindType, Action<CustomStatViewModel> onEdit,
        Action onChanged)
    {
        _onChanged = onChanged;
        Id = id;
        this.text = text;
        this.kind = kind;
        this.kindType = kindType;
        _onEdit = onEdit;
    }

    partial void OnEnabledChanged(bool value) => _onChanged?.Invoke();

    partial void OnMinChanged(string value) => _onChanged?.Invoke();

    partial void OnMaxChanged(string value) => _onChanged?.Invoke();

    [RelayCommand]
    private void Edit(object commandParameter) => _onEdit?.Invoke(this);

    /// <summary>Another stat of the filters, the min / max are kept.</summary>
    internal void Replace(SearchItem item)
    {
        Id = item.Id;
        Text = item.Text;
        Kind = item.Kind;
        KindType = item.KindType;
    }
}
