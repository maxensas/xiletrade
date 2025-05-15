using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels;

public sealed partial class EditorViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;
    private readonly DataManagerService _dm;

    [ObservableProperty]
    private AsyncObservableCollection<ConfigMods> dangerousMods = new();

    [ObservableProperty]
    private AsyncObservableCollection<ConfigMods> rareMods = new();

    [ObservableProperty]
    private AsyncObservableCollection<ModOption> parser = new();

    [ObservableProperty]
    private AsyncObservableCollection<EditorModViewModel> filter = new();

    [ObservableProperty]
    private AsyncObservableCollection<EditorModViewModel> duplicate = new();

    [ObservableProperty]
    private string configLocation;

    [ObservableProperty]
    private string parserLocation;

    [ObservableProperty]
    private string filterLocation;

    [ObservableProperty]
    private string searchField;

    public EditorViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
        string dataPath = System.IO.Path.GetFullPath("Data\\");
        
        StringBuilder sb = new(dataPath);
        sb.Append("Lang\\")
          .Append(Strings.Culture[_dm.Config.Options.Language])
          .Append("\\");

        ConfigLocation = dataPath + Strings.File.Config;
        ParserLocation = sb.ToString() + Strings.File.ParsingRules;
        FilterLocation = sb.ToString() + Strings.File.Filters;

        InitVm(null);
    }

    [RelayCommand]
    private void SaveChanges(object commandParameter)
    {
        _dm.Parser.Mods = Parser.Where(x => x.Replace is "equals" or "contains" && x.Old.Length > 0 && x.New.Length > 0).ToArray();
        string fileToSave = Json.Serialize<ParserData>(_dm.Parser);
        _dm.Save_File(fileToSave, ParserLocation);

        _dm.Config.DangerousMapMods = DangerousMods.Where(x => x.Id.Length > 0 && x.Id.Contain("stat_")).ToArray();
        _dm.Config.RareItemMods = RareMods.Where(x => x.Id.Length > 0 && x.Id.Contain("stat_")).ToArray();
        fileToSave = Json.Serialize<ConfigData>(_dm.Config);
        _dm.Save_File(fileToSave, ConfigLocation);
    }

    [RelayCommand]
    private void InitVm(object commandParameter)
    {
        Parser.Clear();
        foreach (var modOption in _dm.Parser.Mods)
        {
            ModOption mod = new()
            {
                Id = modOption.Id,
                New = modOption.New,
                Old = modOption.Old,
                Replace = modOption.Replace,
                Stat = modOption.Stat
            };
            Parser.Add(mod);
        }
        SearchField = string.Empty;
        Filter.Clear();

        //if (DataManager.Config.DangerousMods.FirstOrDefault(x => x.Text == ifilter.Text && x.ID.IndexOf(inherit + "/", StringComparison.Ordinal) > -1) != null)
        DangerousMods.Clear();
        foreach (var modOption in _dm.Config.DangerousMapMods)
        {
            ConfigMods mod = new()
            {
                Id = modOption.Id,
                Text = modOption.Text
            };
            DangerousMods.Add(mod);
        }

        RareMods.Clear();
        foreach (var modOption in _dm.Config.RareItemMods)
        {
            ConfigMods mod = new()
            {
                Id = modOption.Id,
                Text = modOption.Text
            };
            RareMods.Add(mod);
        }
    }

    [RelayCommand]
    private void SearchFilter(object commandParameter)
    {
        Filter.Clear();
        var search = SearchField.Length > 0;
        if (!search)
        {
            return;
        }

        var entriesMerge =
                from result in _dm.Filter.Result
                from Entrie in result.Entries
                select Entrie;
        if (entriesMerge.Any())
        {
            var entrieMatches =
                from result in entriesMerge
                where result.Text.Contain(SearchField)
                select result;
            if (entrieMatches.Any())
            {
                int nb = 0;
                foreach (var match in entrieMatches)
                {
                    EditorModViewModel newEntrie = new()
                    {
                        Num = nb,
                        Id = match.ID,
                        Type = match.Type,
                        Text = match.Text
                    };
                    Filter.Add(newEntrie);
                    nb++;
                }
            }
        }
    }

    [RelayCommand]
    private void ShowDuplicates(object commandParameter)
    {
        Duplicate.Clear();

        var filter =
                from result in _dm.Filter.Result
                from Entrie in result.Entries
                select Entrie;
        if (!filter.Any())
        {
            return;
        }
        foreach (var entrie in filter)
        {
            if (Duplicate.Where(x => x.Id == entrie.ID).Any())
            {
                continue;
            }
            var duplicate =
            from result in filter
            where !result.ID.Equal(entrie.ID)
            && result.Text.Equal(entrie.Text)
            && result.Type.Equal(entrie.Type)
            select result;
            if (!duplicate.Any())
            {
                continue;
            }

            int nb = 0;
            EditorModViewModel indexEntrie = new()
            {
                Num = nb,
                Id = entrie.ID,
                Type = entrie.Type,
                Text = entrie.Text
            };
            Duplicate.Add(indexEntrie);

            foreach (var match in duplicate)
            {
                nb++;
                EditorModViewModel duplicateEntrie = new()
                {
                    Num = nb,
                    Id = match.ID,
                    Type = match.Type,
                    Text = match.Text
                };
                Duplicate.Add(duplicateEntrie);
            }
        }
    }
}
