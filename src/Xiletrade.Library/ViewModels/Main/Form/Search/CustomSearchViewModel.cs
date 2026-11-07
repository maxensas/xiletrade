using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Application.LiveSearch;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form.Search;

public sealed partial class CustomSearchViewModel : ViewModelBase
{
    private readonly IViewManager _view;
    private readonly IMessageAdapterService _message;
    private readonly DataManagerService _dm;
    private readonly MainViewModel _vm;
    private readonly LiveSearchService _live;

    private const string PRESET_EXTENSION = "json";
    private const string PRESET_FILE = "XiletradeSearchPresets.json";

    // options of the miscellaneous section : index of the form (any, no, yes) <-> option of the presets (null, false, true)
    private static readonly (Func<FormViewModel, int> GetIndex, Action<FormViewModel, int> SetIndex,
        Func<CustomSearchPresetOptions, bool?> Get, Action<CustomSearchPresetOptions, bool?> Set)[] _options =
    [
        (f => f.IdentifiedIndex, (f, v) => f.IdentifiedIndex = v, o => o.Identified, (o, v) => o.Identified = v),
        (f => f.CorruptedIndex, (f, v) => f.CorruptedIndex = v, o => o.Corrupted, (o, v) => o.Corrupted = v),
        (f => f.DoubleCorruptedIndex, (f, v) => f.DoubleCorruptedIndex = v, o => o.TwiceCorrupted, (o, v) => o.TwiceCorrupted = v),
        (f => f.MirroredIndex, (f, v) => f.MirroredIndex = v, o => o.Mirrored, (o, v) => o.Mirrored = v),
        (f => f.FracturedIndex, (f, v) => f.FracturedIndex = v, o => o.Fractured, (o, v) => o.Fractured = v),
        (f => f.SplitIndex, (f, v) => f.SplitIndex = v, o => o.Split, (o, v) => o.Split = v),
        (f => f.CraftedIndex, (f, v) => f.CraftedIndex = v, o => o.Crafted, (o, v) => o.Crafted = v),
        (f => f.MutatedIndex, (f, v) => f.MutatedIndex = v, o => o.Mutated, (o, v) => o.Mutated = v),
        (f => f.DesecratedIndex, (f, v) => f.DesecratedIndex = v, o => o.Desecrated, (o, v) => o.Desecrated = v),
        (f => f.VeiledIndex, (f, v) => f.VeiledIndex = v, o => o.Veiled, (o, v) => o.Veiled = v),
        (f => f.SanctifiedIndex, (f, v) => f.SanctifiedIndex = v, o => o.Sanctified, (o, v) => o.Sanctified = v)
    ];

    [ObservableProperty]
    private SuggestionSearchViewModel search;

    /// <summary>Header, search presets, separator, then unidentified uniques.</summary>
    [ObservableProperty]
    private AsyncObservableCollection<SearchPresetItem> presets;

    [ObservableProperty]
    private int presetIndex;

    /// <summary>A search preset is selected : it can be updated with the criteria of the form.</summary>
    [ObservableProperty]
    private bool canUpdatePreset;

    /// <summary>Name of the preset added with the criteria of the form.</summary>
    [ObservableProperty]
    private string presetName = string.Empty;

    /// <summary>Field of the name of the preset added, opened by "add preset".</summary>
    [ObservableProperty]
    private bool isPresetMenuOpen;

    // list of presets rebuilt after a change of the file : the preset selected is not applied
    private bool _isReloading;

    /// <summary>Item categories of the trade site, the first one is any category.</summary>
    [ObservableProperty]
    private AsyncObservableCollection<SearchCategoryItem> categories;

    [ObservableProperty]
    private int categoryIndex;

    /// <summary>Stat groups of the search, as on the trade site : and, not, count.</summary>
    [ObservableProperty]
    private AsyncObservableCollection<StatGroupViewModel> statGroups = new();

    /// <summary>Menu of the stat group kinds, opened by "add stat group".</summary>
    [ObservableProperty]
    private bool isGroupMenuOpen;

    /// <summary>Search criteria displayed, otherwise the results : hidden when a search is launched.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleText))]
    private bool showCriteria = true;

    /// <summary>A search was launched : the results can be displayed.</summary>
    [ObservableProperty]
    private bool hasResults;

    public string ToggleText => ShowCriteria ? Resources.Resources.Main266_ShowResults : Resources.Resources.Main265_ShowFilters;

    /// <summary>Min / max of the filters, all sections : read by XiletradeItem.</summary>
    [ObservableProperty]
    private AsyncObservableCollection<MinMaxViewModel> minMaxList = new();

    /// <summary>Sections of the filters, as on the trade site.</summary>
    [ObservableProperty]
    private AsyncObservableCollection<FilterSectionViewModel> filterSections;

    /// <summary>A live search is listening (live button).</summary>
    [ObservableProperty]
    private bool isLiveRunning;

    /// <summary>The live search is paused (live button).</summary>
    [ObservableProperty]
    private bool isLivePaused;

    private bool IsPoe2 => _dm.Config.Options.GameVersion is 1;

    /// <summary>Category sent to the trade api, null : any category.</summary>
    internal string CategoryId => CategoryIndex > 0 && CategoryIndex < Categories.Count ? Categories[CategoryIndex].Id : null;

    /// <summary>Unidentified unique selected in the presets, null : none.</summary>
    internal UniqueUnidentified SelectedUnid => PresetIndex > 0 && PresetIndex < Presets.Count ? Presets[PresetIndex].Unid : null;

    public CustomSearchViewModel(DataManagerService dm, MainViewModel vm,
        IViewManager view, IMessageAdapterService message, LiveSearchService live)
    {
        _live = live;
        _vm = vm;
        _dm = dm;
        _view = view;
        _message = message;

        // names and types with their category, and its position in the items file (order of the suggestions)
        var searchList = dm.Items.SelectMany((cat, index) => cat.Entries
            .Select(x => (Value: x.Text ?? x.Type, IsUnique: x.Text is not null, Category: cat.Label, Group: index))).ToList();

        search = new(searchList);

        // state of the live searches : the live search goes on when this form is closed
        IsLiveRunning = live.IsRunning;
        IsLivePaused = live.IsPaused;
        WeakReferenceMessenger.Default.Register<CustomSearchViewModel, LiveSearchStateMessage>(this,
            static (vm, state) => (vm.IsLiveRunning, vm.IsLivePaused) = (state.IsRunning, state.IsPaused));

        _vm.Result.Rate.ShowMin = false;
        _vm.Result.Quick.RightString = Resources.Resources.Main245_SearchPreset;
        _vm.Result.Quick.LeftString = string.Empty;

        var isPoe2 = IsPoe2;
        presets = GetPresets(isPoe2);
        presetIndex = 0;

        categories = GetCategories(isPoe2);
        categoryIndex = 0;

        filterSections = GetFilterSections(isPoe2);
        statGroups.Add(new(dm, StatGroup.And));
    }

    partial void OnPresetIndexChanged(int value)
    {
        CanUpdatePreset = value > 0 && value < Presets.Count && Presets[value].Preset is not null;
        if (_isReloading || value <= 0 || value >= Presets.Count)
        {
            return;
        }
        _view.ClearFocus();

        if (Search is not null)
        {
            Search.SearchQuery = string.Empty;
        }

        var item = Presets[value];
        if (item.Unid is not null)
        {
            // only the unique : criteria of a previous search preset are removed
            CategoryIndex = 0;
            StatGroups.Clear();
            StatGroups.Add(new(_dm, StatGroup.And));
            ClearFilters();

            _vm.Form.IdentifiedIndex = 1;
            _vm.Form.Rarity.Index = 4;

            if (item.Unid.AutoSearch)
            {
                LaunchCustomSearchAsync();
            }
            return;
        }
        if (item.Preset is not null)
        {
            // criteria set, the search is launched by the user unless the preset says otherwise
            ApplyPreset(item.Preset);
            if (item.Preset.AutoSearch)
            {
                LaunchCustomSearchAsync();
            }
        }
    }

    [RelayCommand]
    private void ResetCustomSearch(object commandParameter)
    {
        Search.SearchQuery = string.Empty;

        ClearFilters();
        StatGroups.Clear();
        StatGroups.Add(new(_dm, StatGroup.And));
        ShowCriteria = true;

        PresetIndex = CategoryIndex = _vm.Form.Rarity.Index = 0;
    }

    // min / max of the sections and options of the miscellaneous section
    private void ClearFilters()
    {
        foreach (var minMax in MinMaxList)
        {
            minMax.Selected = false;
            minMax.Min = string.Empty;
            minMax.Max = string.Empty;
        }

        foreach (var option in _options)
        {
            option.SetIndex(_vm.Form, 0);
        }
    }

    [RelayCommand]
    private void CustomSearch(object commandParameter)
    {
        // a text typed replaces the unidentified unique selected, a preset without auto search keeps it
        if (!string.IsNullOrEmpty(Search.SearchQuery))
        {
            PresetIndex = 0;
        }
        LaunchCustomSearchAsync();
    }

    // live search of the criteria : started, or stopped when one is running
    [RelayCommand]
    private async Task ToggleLive(object commandParameter)
    {
        if (_live.IsRunning)
        {
            _live.StopAll();
            return;
        }
        if (!HasCriteria())
        {
            return;
        }
        _view.ClearFocus();
        var json = _vm.GetSerialized(_vm.Form.Market[_vm.Form.MarketIndex], customSearch: true);
        if (json is { Length: > 0 })
        {
            await _live.StartAsync(json, _vm.Form.League[_vm.Form.LeagueIndex]);
        }
    }

    // new search preset with the criteria of the form, added after the other search presets
    [RelayCommand]
    private void AddPreset(object commandParameter)
    {
        var name = PresetName?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        IsPresetMenuOpen = false;
        PresetName = string.Empty;

        var preset = new CustomSearchPreset { Name = name, Poe2 = IsPoe2 };
        FillPreset(preset);
        if (SavePresets([.. _dm.SearchPreset.CustomSearch ?? [], preset]))
        {
            ReloadPresets(preset);
        }
    }

    // the search preset selected takes the criteria of the form, its name and options are kept
    [RelayCommand]
    private void UpdatePreset(object commandParameter)
    {
        var index = PresetIndex;
        var preset = index > 0 && index < Presets.Count ? Presets[index].Preset : null;
        if (preset is null)
        {
            return;
        }
        FillPreset(preset);
        _dm.SaveSearchPreset();
    }

    // the search preset selected is removed from the file, after confirmation
    [RelayCommand]
    private void DeletePreset(object commandParameter)
    {
        var index = PresetIndex;
        var preset = index > 0 && index < Presets.Count ? Presets[index].Preset : null;
        if (preset is null || !_message.ShowResult(string.Format(Resources.Resources.Main276_DeletePresetConfirm, preset.Name),
            Resources.Resources.Main275_DeletePreset, MessageStatus.Exclamation, yesNo: true))
        {
            return;
        }
        if (SavePresets([.. _dm.SearchPreset.CustomSearch.Where(x => x != preset)]))
        {
            ReloadPresets();
        }
    }

    // presets of a file added, a preset with the same name in the same game is replaced :
    // their stats are checked with the filters of their game
    [RelayCommand]
    private async Task ImportPresets(object commandParameter)
    {
        var path = await _view.ShowOpenFileViewAsync(Resources.Resources.Main287_ImportPresets,
            Resources.Resources.Main289_SearchPresets, PRESET_EXTENSION);
        if (path is null)
        {
            return;
        }
        var imported = _dm.ReadSearchPresets(path);
        if (imported is null)
        {
            return;
        }
        imported = [.. imported.Where(x => !string.IsNullOrWhiteSpace(x?.Name))];
        if (imported.Length is 0)
        {
            _message.Show(Resources.Resources.Main294_NoPresetFound, Resources.Resources.Main289_SearchPresets, MessageStatus.Exclamation);
            return;
        }
        var report = _dm.RepairSearchPresets(imported);

        if (!SavePresets(_dm.SearchPreset.CustomSearch.Merge(imported)))
        {
            return;
        }
        ReloadPresets();

        var message = string.Format(Resources.Resources.Main290_PresetsImported, imported.Length);
        if (report.Count > 0)
        {
            message += "\n\n" + DataManagerService.GetRepairMessage(report);
        }
        _message.Show(message, Resources.Resources.Main289_SearchPresets, MessageStatus.Information);
    }

    // presets of both games written in a file
    [RelayCommand]
    private async Task ExportPresets(object commandParameter)
    {
        var count = _dm.SearchPreset.CustomSearch?.Length ?? 0;
        if (count is 0)
        {
            _message.Show(Resources.Resources.Main294_NoPresetFound, Resources.Resources.Main289_SearchPresets, MessageStatus.Exclamation);
            return;
        }
        var path = await _view.ShowSaveFileViewAsync(Resources.Resources.Main288_ExportPresets,
            Resources.Resources.Main289_SearchPresets, PRESET_EXTENSION, PRESET_FILE);
        if (path is null || !_dm.ExportSearchPresets(path))
        {
            return;
        }
        _message.Show(string.Format(Resources.Resources.Main295_PresetsExported, count),
            Resources.Resources.Main289_SearchPresets, MessageStatus.Information);
    }

    // the presets in memory are restored when the save of the configuration fails
    private bool SavePresets(CustomSearchPreset[] presets)
    {
        var previous = _dm.SearchPreset.CustomSearch;
        _dm.SearchPreset.CustomSearch = presets?.Length > 0 ? presets : null;
        if (_dm.SaveSearchPreset())
        {
            return true;
        }
        _dm.SearchPreset.CustomSearch = previous;
        return false;
    }

    // list rebuilt after a change of the file : the preset given is selected without being applied, none : header
    private void ReloadPresets(CustomSearchPreset selected = null)
    {
        _isReloading = true;
        Presets = GetPresets(IsPoe2);
        var index = 0;
        if (selected is not null)
        {
            for (int i = 1; i < Presets.Count; i++)
            {
                if (Presets[i].Preset == selected)
                {
                    index = i;
                    break;
                }
            }
        }
        PresetIndex = index;
        CanUpdatePreset = index > 0;
        _isReloading = false;
    }

    // criteria of the form : category, rarity, min / max and options of the sections,
    // enabled stats of the enabled groups, count of the first count group
    private void FillPreset(CustomSearchPreset preset)
    {
        preset.Category = CategoryId;
        preset.Rarity = GetRarityId(_vm.Form.Rarity.Index);

        CustomSearchPresetFilter[] filters = [.. MinMaxList
            .Where(minMax => minMax.Min.Length > 0 || minMax.Max.Length > 0)
            .Select(minMax => new CustomSearchPresetFilter { Id = minMax.Id.ToString(), Min = minMax.Min.ToNullableDouble(), Max = minMax.Max.ToNullableDouble() })
            .Where(filter => filter.Min is not null || filter.Max is not null)];
        preset.Filters = filters.Length > 0 ? filters : null;
        preset.Options = GetOptions();

        var count = StatGroups.FirstOrDefault(x => x.IsCount && x.Enabled);
        preset.CountMin = count?.Min.ToNullableDouble();
        preset.CountMax = count?.Max.ToNullableDouble();

        CustomSearchPresetStat[] stats = [.. StatGroups.Where(group => group.Enabled)
            .SelectMany(group => group.Filters.Where(stat => stat.Enabled).Select(stat => new CustomSearchPresetStat
            {
                Id = stat.Id,
                Min = stat.Min.ToNullableDouble(),
                Max = stat.Max.ToNullableDouble(),
                Group = group.Type is StatGroup.And ? null : group.Type.ToApi() // and : default group
            }))];
        preset.Stats = stats.Length > 0 ? stats : null;
    }

    // options of the form, null : all options are "any"
    private CustomSearchPresetOptions GetOptions()
    {
        var options = new CustomSearchPresetOptions();
        var isAny = true;
        foreach (var option in _options)
        {
            bool? value = option.GetIndex(_vm.Form) switch
            {
                1 => false,
                2 => true,
                _ => null
            };
            option.Set(options, value);
            isAny &= value is null;
        }
        return isAny ? null : options;
    }

    private void SetOptions(CustomSearchPresetOptions options)
    {
        if (options is null)
        {
            return;
        }
        foreach (var option in _options)
        {
            option.SetIndex(_vm.Form, option.Get(options) switch
            {
                false => 1,
                true => 2,
                _ => 0
            });
        }
    }

    // a filter of the other game or no longer existing is ignored
    private void SetFilters(CustomSearchPresetFilter[] filters)
    {
        foreach (var filter in filters ?? [])
        {
            if (filter is null || !filter.TryGetPanel(out var id))
            {
                continue;
            }
            var minMax = MinMaxList.FirstOrDefault(x => x.Id == id);
            if (minMax is null)
            {
                continue;
            }
            minMax.Min = filter.Min.ToStr();
            minMax.Max = filter.Max.ToStr();
        }
    }

    // index of the rarity list (RarityViewModel) -> rarity of the trade site, null : none
    private string GetRarityId(int index)
    {
        var list = _vm.Form.Rarity.ComboBox;
        var id = index >= 0 && index < list.Count ? list[index].AsSpan().ToRarityApi() : string.Empty;
        return id.Length > 0 ? id : null;
    }

    // results displayed only when a search was launched
    [RelayCommand]
    private void ToggleCriteria(object commandParameter) => ShowCriteria = !ShowCriteria || !HasResults;

    // "and", "not" or "count"
    [RelayCommand]
    private void AddStatGroup(object commandParameter)
    {
        IsGroupMenuOpen = false;
        StatGroups.Add(new(_dm, (commandParameter as string).ToStatGroup()));
    }

    [RelayCommand]
    private void RemoveStatGroup(object commandParameter)
    {
        if (commandParameter is StatGroupViewModel group)
        {
            StatGroups.Remove(group);
        }
    }

    private void ApplyPreset(CustomSearchPreset preset)
    {
        // criteria of the previous preset or unidentified unique removed
        ClearFilters();
        SetFilters(preset.Filters);
        SetOptions(preset.Options);

        CategoryIndex = GetCategoryIndex(preset.Category);
        var rarityIndex = GetRarityIndex(preset.Rarity);
        if (rarityIndex >= 0)
        {
            _vm.Form.Rarity.Index = rarityIndex;
        }

        // one group by kind used, in the order of the trade site
        StatGroups.Clear();
        foreach (var type in new[] { StatGroup.And, StatGroup.Not, StatGroup.Count })
        {
            var group = new StatGroupViewModel(_dm, type);
            if (type is StatGroup.Count)
            {
                group.Min = preset.CountMin.ToStr();
                group.Max = preset.CountMax.ToStr();
            }
            foreach (var presetStat in preset.Stats ?? [])
            {
                // a stat no longer in the trade filters is ignored
                if (presetStat.Group.ToStatGroup() != type || !TryGetFilterEntry(presetStat.Id, out var entry, out var kind))
                {
                    continue;
                }
                group.AddStat(entry.ID, entry.Text, kind, entry.Type, presetStat.Min.ToStr(), presetStat.Max.ToStr());
            }
            if (group.Filters.Count > 0 || type is StatGroup.And)
            {
                StatGroups.Add(group);
            }
        }
    }

    private AsyncObservableCollection<SearchPresetItem> GetPresets(bool isPoe2)
    {
        var list = new AsyncObservableCollection<SearchPresetItem>
        {
            new() { Name = Resources.Resources.Main249_SelectPreset }
        };

        foreach (var preset in _dm.SearchPreset.CustomSearch?.Where(x => x.Poe2 == isPoe2) ?? [])
        {
            list.Add(new() { Name = preset.Name, Preset = preset });
        }

        var separator = list.Count > 1;
        foreach (var unid in _dm.SearchPreset.UnidUnique?.Where(x => x.Poe2 == isPoe2) ?? [])
        {
            if (separator)
            {
                list.Add(new() { IsSeparator = true });
                separator = false;
            }
            var translated = Translate(unid);
            list.Add(new() { Name = translated.Name, Image = unid.Image, Unid = translated });
        }
        return list;
    }

    // name and type in the language used : the data of the file are not modified
    private UniqueUnidentified Translate(UniqueUnidentified unid)
    {
        if (_dm.Config.Options.Language is 0)
        {
            return unid;
        }
        return new()
        {
            Name = unid.Name?.Length > 0 ? _dm.Words.FindWordByNameEn(unid.Name)?.Name ?? unid.Name : unid.Name,
            Type = unid.Type?.Length > 0 ? _dm.Bases.FindBaseByNameEn(unid.Type)?.Name ?? unid.Type : unid.Type,
            IlvlVariant = unid.IlvlVariant,
            Valuable = unid.Valuable,
            Image = unid.Image,
            Poe2 = unid.Poe2,
            AutoSearch = unid.AutoSearch
        };
    }

    // categories of the items file, each one followed by its sub-categories named with the item classes :
    // ids of the trade api (see ItemFlag.GetItemCategoryApi), item class of poe 1, item class of poe 2
    private AsyncObservableCollection<SearchCategoryItem> GetCategories(bool isPoe2)
    {
        (string Id, string Poe1, string Poe2)[] subCategories =
        [
            ("weapon.bow", "Bow", "Bow"), ("weapon.claw", "Claw", "Claw"), ("weapon.basedagger", "Dagger", "Dagger"),
            ("weapon.runedagger", "Rune Dagger", null), ("weapon.oneaxe", "One Hand Axe", "One Hand Axe"),
            ("weapon.onemace", "One Hand Mace", "One Hand Mace"), ("weapon.onesword", "One Hand Sword", "One Hand Sword"),
            ("weapon.rapier", "Thrusting One Hand Sword", null), ("weapon.sceptre", "Sceptre", "Sceptre"),
            ("weapon.staff", "Staff", "Staff"), ("weapon.warstaff", "Warstaff", "Warstaff"),
            ("weapon.twoaxe", "Two Hand Axe", "Two Hand Axe"), ("weapon.twomace", "Two Hand Mace", "Two Hand Mace"),
            ("weapon.twosword", "Two Hand Sword", "Two Hand Sword"), ("weapon.wand", "Wand", "Wand"),
            ("weapon.rod", "FishingRod", null), ("weapon.crossbow", null, "Crossbow"), ("weapon.flail", null, "Flail"),
            ("weapon.spear", null, "Spear"), ("weapon.talisman", null, "Talisman"), ("weapon.trap", null, "TrapTool"),
            ("armour.chest", "Body Armour", "Body Armour"), ("armour.boots", "Boots", "Boots"),
            ("armour.gloves", "Gloves", "Gloves"), ("armour.helmet", "Helmet", "Helmet"),
            ("armour.shield", "Shield", "Shield"), ("armour.buckler", null, "Buckler"), ("armour.focus", null, "Focus"),
            ("armour.quiver", "Quiver", "Quiver"),
            ("accessory.amulet", "Amulet", "Amulet"), ("accessory.belt", "Belt", "Belt"),
            ("accessory.ring", "Ring", "Ring"), ("accessory.trinket", "Trinket", null),
            ("map.waystone", null, "Map"), ("map.tablet", null, "TowerAugmentation"), ("map.barya", null, "ItemisedSanctum"),
            ("map.fragment", "MapFragment", "MapFragment"),
            ("heistmission.contract", "HeistContract", null), ("heistmission.blueprint", "HeistBlueprint", null),
            ("sanctum.relic", "Relic", "Relic"), ("sanctum.research", "ItemisedSanctum", null),
            ("currency.piece", "UniqueFragment", null), ("card", "DivinationCard", null), ("memoryline", "MemoryLine", null),
            ("sentinel", "SentinelDrone", null)
        ];

        var list = new AsyncObservableCollection<SearchCategoryItem>
        {
            new() { Id = string.Empty, Label = Resources.Resources.General005_Any }
        };
        var added = new HashSet<string>(StringComparer.Ordinal);
        foreach (var category in _dm.Items ?? [])
        {
            if (string.IsNullOrEmpty(category.Id) || string.IsNullOrEmpty(category.Label) || !added.Add(category.Id))
            {
                continue;
            }
            list.Add(new() { Id = category.Id, Label = category.Label });
            foreach (var sub in subCategories)
            {
                if (sub.Id.Length > category.Id.Length && sub.Id[category.Id.Length] is '.'
                    && sub.Id.StartsWith(category.Id, StringComparison.Ordinal))
                {
                    AddSubCategory(list, added, sub.Id, isPoe2 ? sub.Poe2 : sub.Poe1, isSub: true);
                }
            }
        }

        // categories without a category in the items file
        foreach (var sub in subCategories)
        {
            AddSubCategory(list, added, sub.Id, isPoe2 ? sub.Poe2 : sub.Poe1, isSub: false);
        }
        return list;
    }

    // named with the item class in the language used, ignored without name
    private void AddSubCategory(AsyncObservableCollection<SearchCategoryItem> list, HashSet<string> added,
        string id, string itemClass, bool isSub)
    {
        if (itemClass is null || added.Contains(id))
        {
            return;
        }
        var label = _dm.ItemClasses?.FirstOrDefault(x => x.IdOrigin == itemClass)?.Name;
        if (string.IsNullOrEmpty(label))
        {
            return;
        }
        added.Add(id);
        list.Add(new() { Id = id, Label = label, IsSub = isSub });
    }

    private int GetCategoryIndex(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId))
        {
            return 0;
        }
        for (int i = 1; i < Categories.Count; i++)
        {
            if (Categories[i].Id == categoryId)
            {
                return i;
            }
        }
        return 0;
    }

    // rarity of the trade site -> index of the rarity list (RarityViewModel), -1 : none
    private int GetRarityIndex(string rarity)
    {
        if (string.IsNullOrEmpty(rarity))
        {
            return -1;
        }
        var list = _vm.Form.Rarity.ComboBox;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].AsSpan().ToRarityApi() == rarity)
            {
                return i;
            }
        }
        return -1;
    }

    // entry of the trade filters with this id, and the label of its group (pseudo, explicit...)
    private bool TryGetFilterEntry(string id, out FilterResultEntrie entry, out string kind)
    {
        foreach (var result in _dm.Filter?.Result ?? [])
        {
            foreach (var current in result.Entries ?? [])
            {
                if (current.ID == id)
                {
                    entry = current;
                    kind = result.Label;
                    return true;
                }
            }
        }
        entry = null;
        kind = null;
        return false;
    }

    // sections of the filters, as on the trade site : their min / max in their order, all in MinMaxList
    private AsyncObservableCollection<FilterSectionViewModel> GetFilterSections(bool isPoe2)
    {
        var models = MinMaxModel.CreateDictionary();
        if (isPoe2)
        {
            models[StatPanel.MapTier].Text = Resources.Resources.General143_WaystoneTier;
        }

        List<MinMaxViewModel> Get(params StatPanel[] ids)
        {
            var list = new List<MinMaxViewModel>(ids.Length);
            foreach (var id in ids)
            {
                var vm = new MinMaxViewModel(id, models[id]);
                MinMaxList.Add(vm);
                list.Add(vm);
            }
            return list;
        }

        if (isPoe2)
        {
            return
            [
                new(Resources.Resources.Main278_TypeFilters, Get(StatPanel.CommonItemLevel, StatPanel.CommonQuality),
                    isExpanded: true, showRarity: true),
                new(Resources.Resources.Main280_EquipmentFilters, Get(StatPanel.WeaponDamage, StatPanel.WeaponAttacksPerSecond,
                    StatPanel.WeaponCriticalChance, StatPanel.DamageTotal, StatPanel.DamagePhysical, StatPanel.DamageElemental,
                    StatPanel.DefenseArmour, StatPanel.DefenseEnergy, StatPanel.DefenseEvasion, StatPanel.DefenseRunicWard,
                    StatPanel.CommonSocketRune)),
                new(Resources.Resources.Main283_Requirements, Get(StatPanel.CommonRequiresLevel)),
                new(Resources.Resources.Main285_EndgameFilters, Get(StatPanel.MapTier, StatPanel.WaystonePackSize,
                    StatPanel.WaystoneMonsterEffectiveness, StatPanel.WaystoneRarity, StatPanel.WaystoneMonsterRarity)),
                new(Resources.Resources.Main286_Miscellaneous, Get(StatPanel.AreaLevel, StatPanel.CommonSocketGem), showOptions: true)
            ];
        }
        return
        [
            new(Resources.Resources.Main278_TypeFilters, [], isExpanded: true, showRarity: true),
            new(Resources.Resources.Main279_WeaponFilters, Get(StatPanel.WeaponDamage, StatPanel.WeaponAttacksPerSecond,
                StatPanel.WeaponCriticalChance, StatPanel.DamageTotal, StatPanel.DamagePhysical, StatPanel.DamageElemental)),
            new(Resources.Resources.Main281_ArmourFilters, Get(StatPanel.DefenseArmour, StatPanel.DefenseEnergy,
                StatPanel.DefenseEvasion, StatPanel.DefenseWard)),
            new(Resources.Resources.Main282_SocketFilters, Get(StatPanel.CommonSocket, StatPanel.CommonLink)),
            new(Resources.Resources.Main283_Requirements, Get(StatPanel.CommonRequiresLevel)),
            new(Resources.Resources.Main284_MapFilters, Get(StatPanel.MapTier, StatPanel.MapPackSize,
                StatPanel.MapQuantity, StatPanel.MapRarity, StatPanel.AreaLevel)),
            new(Resources.Resources.Main286_Miscellaneous, Get(StatPanel.CommonQuality, StatPanel.CommonItemLevel,
                StatPanel.CommonIntangibility), showOptions: true)
        ];
    }

    internal async void LaunchCustomSearchAsync()
    {
        if (!_vm.Form.Tab.CustomSearchSelected || !HasCriteria())
        {
            return;
        }

        try
        {
            _view.ClearFocus();
            HasResults = true;
            ShowCriteria = false;

            _vm.Result.InitData();
            _vm.Result.DetailList.Clear();

            var json = _vm.GetSerialized(_vm.Form.Market[_vm.Form.MarketIndex], customSearch: true);
            var maxFetch = (int)_dm.Config.Options.SearchFetchDetail;

            var priceInfo = new PricingInfo(json, _vm.Form.League[_vm.Form.LeagueIndex]
                , _vm.Form.Market[_vm.Form.MarketIndex], maxFetch, _vm.Form.SameUser);

            var result = await _vm.Result.UpdateWithApi(priceInfo); // result bar not used in custom search
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error013_XCustomSearch, MessageStatus.Error);
        }
    }

    // a search needs a text, an unidentified unique, a category, a min / max or a stat filter
    private bool HasCriteria() => !string.IsNullOrEmpty(Search.SearchQuery) || SelectedUnid is not null
        || CategoryIndex > 0 || MinMaxList.Any(minMax => minMax.Min.Length > 0 || minMax.Max.Length > 0)
        || StatGroups.Any(group => group.Enabled && group.Filters.Any(stat => stat.Enabled));
}
