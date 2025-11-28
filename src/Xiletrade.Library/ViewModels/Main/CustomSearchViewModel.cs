using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared.Collection;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class CustomSearchViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private AsyncObservableCollection<UniqueUnidentified> unidUniques;

    [ObservableProperty]
    private int unidUniquesIndex;

    [ObservableProperty]
    private string search;

    public CustomSearchViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var vm = _serviceProvider.GetRequiredService<MainViewModel>();
        vm.Result.Rate.ShowMin = false;
        vm.Result.Quick.RightString = Resources.Resources.Main245_SearchPreset;
        vm.Result.Quick.LeftString = string.Empty;

        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
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
}
