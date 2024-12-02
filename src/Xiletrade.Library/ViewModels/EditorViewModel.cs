using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Text;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.ViewModels;

public sealed partial class EditorViewModel : ViewModelBase
{
    [ObservableProperty]
    private AsyncObservableCollection<ConfigMods> dangerousMods = new();

    [ObservableProperty]
    private AsyncObservableCollection<ConfigMods> rareMods = new();

    [ObservableProperty]
    private AsyncObservableCollection<ModOption> parser = new();

    [ObservableProperty]
    private AsyncObservableCollection<EditorModViewModel> filter = new();

    [ObservableProperty]
    private string configLocation;

    [ObservableProperty]
    private string parserLocation;

    [ObservableProperty]
    private string filterLocation;

    [ObservableProperty]
    private string searchField;

    public EditorViewModel()
    {
        string dataPath = System.IO.Path.GetFullPath("Data\\");
        StringBuilder sb = new(dataPath);
        sb.Append("Lang\\")
          .Append(Strings.Culture[DataManager.Config.Options.Language])
          .Append("\\");

        ConfigLocation = dataPath + Strings.File.Config;
        ParserLocation = sb.ToString() + Strings.File.ParsingRules;
        FilterLocation = sb.ToString() + Strings.File.Filters;

        InitVm(null);
    }

    [RelayCommand]
    private void SaveChanges(object commandParameter)
    {
        DataManager.Parser.Mods = Parser.Where(x => x.Replace is "equals" or "contains" && x.Old.Length > 0 && x.New.Length > 0).ToArray();
        string fileToSave = Json.Serialize<ParserData>(DataManager.Parser);
        DataManager.Save_File(fileToSave, ParserLocation);

        DataManager.Config.DangerousMapMods = DangerousMods.Where(x => x.Id.Length > 0 && x.Id.Contains("stat_")).ToArray();
        DataManager.Config.RareItemMods = RareMods.Where(x => x.Id.Length > 0 && x.Id.Contains("stat_")).ToArray();
        fileToSave = Json.Serialize<ConfigData>(DataManager.Config);
        DataManager.Save_File(fileToSave, ConfigLocation);
    }

    [RelayCommand]
    private void InitVm(object commandParameter)
    {
        Parser.Clear();
        foreach (var modOption in DataManager.Parser.Mods)
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
        foreach (var modOption in DataManager.Config.DangerousMapMods)
        {
            ConfigMods mod = new()
            {
                Id = modOption.Id,
                Text = modOption.Text
            };
            DangerousMods.Add(mod);
        }

        RareMods.Clear();
        foreach (var modOption in DataManager.Config.RareItemMods)
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
                from result in DataManager.Filter.Result
                from Entrie in result.Entries
                select Entrie;
        if (entriesMerge.Any())
        {
            var entrieMatches =
                from result in entriesMerge
                where result.Text.Contains(SearchField, System.StringComparison.Ordinal)
                select result;
            if (entrieMatches.Any())
            {
                int nb = 0;
                foreach (FilterResultEntrie match in entrieMatches)
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
}
