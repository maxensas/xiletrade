using System.Linq;
using System.Text;
using System.Windows.Input;
using Xiletrade.Library.Models.Collections;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;

namespace Xiletrade.Library.ViewModels;

public sealed class EditorViewModel : BaseViewModel
{
    private AsyncObservableCollection<ConfigMods> dangerousMods = new();
    private AsyncObservableCollection<ConfigMods> rareMods = new();
    private AsyncObservableCollection<ModOption> parser = new();
    private AsyncObservableCollection<ModFilterViewModel> filter = new();
    private string configlocation;
    private string parserlocation;
    private string filterlocation;
    private string searchField;
    private readonly DelegateCommand saveChanges;
    private readonly DelegateCommand initVm;
    private readonly DelegateCommand searchFilter;

    public AsyncObservableCollection<ConfigMods> DangerousMods { get => dangerousMods; set => SetProperty(ref dangerousMods, value); }
    public AsyncObservableCollection<ConfigMods> RareMods { get => rareMods; set => SetProperty(ref rareMods, value); }
    public AsyncObservableCollection<ModOption> Parser { get => parser; set => SetProperty(ref parser, value); }
    public AsyncObservableCollection<ModFilterViewModel> Filter { get => filter; set => SetProperty(ref filter, value); }
    public string Configlocation { get => configlocation; set => SetProperty(ref configlocation, value); }
    public string ParserLocation { get => parserlocation; set => SetProperty(ref parserlocation, value); }
    public string Filterlocation { get => filterlocation; set => SetProperty(ref filterlocation, value); }
    public string SearchField { get => searchField; set => SetProperty(ref searchField, value); }
    public ICommand SaveChanges => saveChanges;
    public ICommand InitVm => initVm;
    public ICommand SearchFilter => searchFilter;

    public EditorViewModel()
    {
        saveChanges = new(OnSaveChanges, CanSaveChanges);
        initVm = new(OnInitVm, CanInitVm);
        searchFilter = new(OnSearchFilter, CanSearchFilter);

        string dataPath = System.IO.Path.GetFullPath("Data\\");
        StringBuilder sb = new(dataPath);
        sb.Append("Lang\\")
          .Append(Strings.Culture[DataManager.Config.Options.Language])
          .Append("\\");

        Configlocation = dataPath + Strings.File.Config;
        ParserLocation = sb.ToString() + Strings.File.ParsingRules;
        Filterlocation = sb.ToString() + Strings.File.Filters;

        OnInitVm(null);
    }

    private bool CanSaveChanges(object commandParameter)
    {
        return true;
    }

    private void OnSaveChanges(object commandParameter)
    {
        DataManager.Parser.Mods = Parser.Where(x => x.Replace is "equals" or "contains" && x.Old.Length > 0 && x.New.Length > 0).ToArray();
        string fileToSave = Json.Serialize<ParserData>(DataManager.Parser);
        DataManager.Save_File(fileToSave, ParserLocation);

        DataManager.Config.DangerousMods = DangerousMods.Where(x => x.ID.Length > 0 && x.ID.Contains("stat_")).ToArray();
        DataManager.Config.RareMods = RareMods.Where(x => x.ID.Length > 0 && x.ID.Contains("stat_")).ToArray();
        fileToSave = Json.Serialize<ConfigData>(DataManager.Config);
        DataManager.Save_File(fileToSave, Configlocation);
    }

    private bool CanInitVm(object commandParameter)
    {
        return true;
    }

    private void OnInitVm(object commandParameter)
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
        foreach (var modOption in DataManager.Config.DangerousMods)
        {
            ConfigMods mod = new()
            {
                ID = modOption.ID,
                Text = modOption.Text
            };
            DangerousMods.Add(mod);
        }

        RareMods.Clear();
        foreach (var modOption in DataManager.Config.RareMods)
        {
            ConfigMods mod = new()
            {
                ID = modOption.ID,
                Text = modOption.Text
            };
            RareMods.Add(mod);
        }
    }
    private bool CanSearchFilter(object commandParameter)
    {
        return true;
    }

    private void OnSearchFilter(object commandParameter)
    {
        Filter.Clear();
        if (SearchField.Length > 0)
        {
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
                        ModFilterViewModel newEntrie = new()
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
}
