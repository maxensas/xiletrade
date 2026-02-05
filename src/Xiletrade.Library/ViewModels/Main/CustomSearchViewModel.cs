using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class CustomSearchViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private SuggestionSearchViewModel search;

    [ObservableProperty]
    private SuggestionSearchViewModel stat;

    [ObservableProperty]
    private AsyncObservableCollection<UniqueUnidentified> unidUniques;

    [ObservableProperty]
    private AsyncObservableCollection<MinMaxViewModel> minMaxList = new();

    [ObservableProperty]
    private int unidUniquesIndex;

    public CustomSearchViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();

        // lot of strings - TO UPDATE
        var searchList = dm.Words.Select(x => x.Name).Concat(dm.Bases.Select(x => x.Name));
        search = new(serviceProvider, searchList);

        stat = new(serviceProvider, dm.Filter.EnumerateTextEntries());

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
                    var word = dm.Words.FindWordByNameEn(unid.Name);
                    if (word is not null)
                    {
                        unid.Name = word.Name;
                    }
                }
                if (unid.Type?.Length > 0)
                {
                    var word = dm.Words.FindWordByNameEn(unid.Name);
                    if (word is not null)
                    {
                        unid.Type = word.Name;
                    }
                }
            }
        }
        unidUniques = [header, .. list];
        unidUniquesIndex = 0;

        FillMinMaxList(MinMaxModel.GetNewMinMaxList(), isPoe2);
    }

    private readonly StatPanel[] _exclude = [StatPanel.CommonMemoryStrand, StatPanel.MapMoreCurrency
        , StatPanel.MapMoreDivCard, StatPanel.MapMoreScarab, StatPanel.MapPackSize, StatPanel.MapQuantity
        , StatPanel.MapRarity, StatPanel.MapMoreMap, StatPanel.MapMonsterRare, StatPanel.MapMonsterMagic
        , StatPanel.SanctumAureus, StatPanel.SanctumInspiration, StatPanel.SanctumMaxResolve
        , StatPanel.SanctumResolve, StatPanel.TotalLife, StatPanel.TotalResistance, StatPanel.TotalGlobalEs];

    private readonly StatPanel[] _statPoe1 = [StatPanel.CommonSocket, StatPanel.CommonLink, StatPanel.DefenseWard];

    private readonly StatPanel[] _statPoe2 = [StatPanel.CommonSocketRune, StatPanel.CommonSocketGem];

    private void FillMinMaxList(IEnumerable<MinMaxModel> minMaxList, bool isPoe2)
    {
        minMaxList.GetModel(StatPanel.CommonItemLevel).Text = Resources.Resources.General032_ItemLv;

        var shortList = minMaxList.Where(x => !_exclude.Contains(x.Id) 
        && (isPoe2 ? !_statPoe1.Contains(x.Id) : !_statPoe2.Contains(x.Id)));

        foreach (var minMax in shortList)
        {
            MinMaxList.Add(new(minMax));
        }
    }
}
