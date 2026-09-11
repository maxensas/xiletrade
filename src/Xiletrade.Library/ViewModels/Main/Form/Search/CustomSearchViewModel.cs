using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Models.Poe.Domain;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main.Form.Panel;

namespace Xiletrade.Library.ViewModels.Main.Form.Search;

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

        var searchList = dm.Items.SelectMany(cat => cat.Entries)
            .Select(x => ( Value: x.Text ?? x.Type, IsUnique: x.Text is not null)).ToList();

        search = new(serviceProvider, searchList);

        //stat = new(serviceProvider, dm.Filter.EnumerateTextEntries());

        var vm = _serviceProvider.GetRequiredService<MainViewModel>();
        vm.Result.Rate.ShowMin = false;
        vm.Result.Quick.RightString = Resources.Resources.Main245_SearchPreset;
        vm.Result.Quick.LeftString = string.Empty;

        // to finish

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

        minMaxList = GetMinMaxList(MinMaxModel.CreateDictionary(), isPoe2);
    }

    private readonly StatPanel[] _exclude = [StatPanel.CommonMemoryStrand, StatPanel.MapMoreCurrency
        , StatPanel.MapMoreDivCard, StatPanel.MapMoreScarab, StatPanel.MapPackSize, StatPanel.MapQuantity
        , StatPanel.MapRarity, StatPanel.MapMoreMap, StatPanel.MapMonsterRare, StatPanel.MapMonsterMagic
        , StatPanel.GoldFound, StatPanel.DeadSulphur
        , StatPanel.SanctumAureus, StatPanel.SanctumInspiration, StatPanel.SanctumMaxResolve
        , StatPanel.SanctumResolve, StatPanel.TotalLife, StatPanel.TotalElemResistance, StatPanel.TotalGlobalEs, StatPanel.TotalAttribute
        , StatPanel.WaystoneRevives, StatPanel.WaystoneRarity, StatPanel.WaystonePackSize
        , StatPanel.WaystoneMonsterRarity, StatPanel.WaystoneMonsterEffectiveness, StatPanel.WaystoneDrop
        , StatPanel.HeistRevealedWings, StatPanel.HeistRevealedEscapeRoutes, StatPanel.HeistRevealedRewardRooms
        , StatPanel.HeistTotalWings, StatPanel.HeistTotalEscapeRoutes, StatPanel.HeistTotalRewardRooms
        , StatPanel.HeistLockpicking, StatPanel.HeistDemolition, StatPanel.HeistCounterThaumaturgy
        , StatPanel.HeistTrapDisarmament, StatPanel.HeistAgility, StatPanel.HeistEngineering
        , StatPanel.HeistBruteForce, StatPanel.HeistPerception, StatPanel.HeistDeception
    ];

    private readonly StatPanel[] _statPoe1 = [StatPanel.CommonSocket, StatPanel.CommonLink, StatPanel.DefenseWard];

    private readonly StatPanel[] _statPoe2 = [StatPanel.CommonSocketRune, StatPanel.CommonSocketGem, StatPanel.DefenseRunicWard];

    partial void OnUnidUniquesIndexChanged(int value)
    {
        if (value < 0)
        {
            return;
        }
        _serviceProvider.GetRequiredService<INavigationService>().ClearKeyboardFocus();
        var vm = _serviceProvider.GetRequiredService<MainViewModel>();
        Search.SearchQuery = string.Empty;

        if (UnidUniquesIndex is 0)
        {
            return;
        }

        if (UnidUniquesIndex > 0)
        {
            vm.Form.IdentifiedIndex = 1;
            vm.Form.CorruptedIndex = 0;
            vm.Form.Rarity.Index = 4;

            vm.LaunchCustomSearch();
        }
    }

    [RelayCommand]
    private void ResetCustomSearch(object commandParameter)
    {
        var vm = _serviceProvider.GetRequiredService<MainViewModel>();
        Search.SearchQuery = string.Empty;

        foreach (var minMax in MinMaxList)
        {
            minMax.Selected = false;
            minMax.Min = string.Empty;
            minMax.Max = string.Empty;
        }

        UnidUniquesIndex = vm.Form.Rarity.Index = vm.Form.CorruptedIndex
            = vm.Form.IdentifiedIndex = vm.Form.MirroredIndex = vm.Form.FracturedIndex
            = vm.Form.SplitIndex = vm.Form.CraftedIndex = vm.Form.MutatedIndex = 0;
    }

    [RelayCommand]
    private void CustomSearch(object commandParameter)
    {
        UnidUniquesIndex = 0;
        _serviceProvider.GetRequiredService<MainViewModel>().LaunchCustomSearch();
    }

    [RelayCommand]
    private void StatSearch(object commandParameter)
    {

    }

    private AsyncObservableCollection<MinMaxViewModel> GetMinMaxList(Dictionary<StatPanel, MinMaxModel> minMaxDic, bool isPoe2)
    {
        AsyncObservableCollection<MinMaxViewModel> minMaxList = new();

        var forbiddenStats = isPoe2 ? _statPoe1 : _statPoe2;

        foreach (var model in minMaxDic)
        {
            if (_exclude.Contains(model.Key))
                continue;

            if (forbiddenStats.Contains(model.Key))
                continue;

            minMaxList.Add(new(model.Key, model.Value));
        }

        return minMaxList;
    }
}
