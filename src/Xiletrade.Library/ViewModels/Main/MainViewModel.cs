using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xiletrade.Library.Models;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Logic;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;
using Xiletrade.Library.ViewModels.Command;
using Xiletrade.Library.ViewModels.Main.Form;
using Xiletrade.Library.ViewModels.Main.Result;

namespace Xiletrade.Library.ViewModels.Main;

public sealed partial class MainViewModel : ViewModelBase
{
    private static IServiceProvider _serviceProvider;

    [ObservableProperty]
    private FormViewModel form;

    [ObservableProperty]
    private ResultViewModel result;

    [ObservableProperty]
    private NinjaViewModel ninja;

    [ObservableProperty]
    private string notifyName;

    internal ItemBaseName CurrentItem { get; set; }
    internal MainLogic Logic { get; private set; }

    public MainCommand Commands { get; private set; }
    public TrayMenuCommand TrayCommands { get; private set; }

    public List<MouseGestureCom> GestureList { get; set; } = new();

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Logic = new(this, _serviceProvider);
        Commands = new(this, _serviceProvider);
        TrayCommands = new(this, _serviceProvider);
        NotifyName = "Xiletrade " + Common.GetFileVersion();
        GestureList.Add(new (Commands.WheelIncrementCommand, ModifierKey.None, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelIncrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelIncrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Up));
        GestureList.Add(new (Commands.WheelDecrementCommand, ModifierKey.None, MouseWheelDirection.Down));
        GestureList.Add(new (Commands.WheelDecrementTenthCommand, ModifierKey.Control, MouseWheelDirection.Down));
        GestureList.Add(new (Commands.WheelDecrementHundredthCommand, ModifierKey.Shift, MouseWheelDirection.Down));
    }

    internal void InitViewModels(bool useBulk = false)
    {
        Form = new(_serviceProvider, useBulk);
        Result = new(_serviceProvider);
        Ninja = new(_serviceProvider);
    }

    internal void SearchCurrency(string str)
    {
        var exVm = str is "get" ? Form.Bulk.Get :
            str is "pay" ? Form.Bulk.Pay :
            str is "shop" ? Form.Shop.Exchange : null;
        if (exVm is not null)
        {
            if (exVm.Search.Length >= 1)
            {
                SelectExchangeCurrency(str + "/contains", exVm.Search);
            }
            else
            {
                exVm.CategoryIndex = 0;
                exVm.CurrencyIndex = 0;
            }
        }
    }

    internal void SelectExchangeCurrency(string args, string currency, string tier = null)
    {
        var arg = args.Split('/');
        if (arg[0] is not "pay" and not "get" and not "shop")
        {
            return;
        }
        IEnumerable<(string, string, string Text)> cur;
        if (arg.Length > 1 && arg[1] is "contains") // contains requests to improve
        {
            var curKeys = currency.ToLowerInvariant().Split(' ');
            if (curKeys.Length >= 3)
            {
                cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep &&
                (Entrie.Text.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal))
                && (Entrie.Text.ToLowerInvariant().Contains(curKeys[1], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[1], StringComparison.Ordinal))
                && (Entrie.Text.ToLowerInvariant().Contains(curKeys[2], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[2], StringComparison.Ordinal))
                select (result.Id, Entrie.Id, Entrie.Text);
            }
            else if (curKeys.Length is 2)
            {
                cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep &&
                (Entrie.Text.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal))
                && (Entrie.Text.ToLowerInvariant().Contains(curKeys[1], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[1], StringComparison.Ordinal))
                select (result.Id, Entrie.Id, Entrie.Text);
            }
            else
            {
                cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep &&
                (Entrie.Text.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal)
                || Entrie.Id.ToLowerInvariant().Contains(curKeys[0], StringComparison.Ordinal))
                select (result.Id, Entrie.Id, Entrie.Text);
            }
        }
        else
        {
            cur =
                from result in DataManager.Currencies
                from Entrie in result.Entries
                where Entrie.Id is not Strings.sep && Entrie.Text == currency
                select (result.Id, Entrie.Id, Entrie.Text);
        }

        if (cur.Any())
        {
            string curClass = cur.First().Item1;
            string curId = cur.First().Item2;
            string curText = cur.First().Text;

            string selectedCurrency = string.Empty, selectedTier = string.Empty;
            string selectedCategory = Strings.GetCategory(curClass, curId);

            if (selectedCategory.Length > 0)
            {
                selectedCurrency = curText;

                if (selectedCategory == Resources.Resources.Main055_Divination)
                {
                    var tmpDiv = DataManager.DivTiers.FirstOrDefault(x => x.Tag == curId);
                    selectedTier = tmpDiv != null ? "T" + tmpDiv.Tier : Resources.Resources.Main016_TierNothing;
                }
                if (selectedCategory == Resources.Resources.Main056_Maps
                    || selectedCategory == Resources.Resources.Main179_UniqueMaps
                    || selectedCategory == Resources.Resources.Main217_BlightedMaps)
                {
                    if (tier?.Length > 0)
                    {
                        selectedTier = "T" + tier;
                    }
                    else
                    {
                        var match = RegexUtil.DecimalNoPlusPattern().Matches(curText);
                        if (match.Count == 1)
                        {
                            selectedTier = "T" + match[0].Value.ToString();
                        }
                    }
                }
            }

            var bulk = arg[0] is "pay" ? Form.Bulk.Pay
                : arg[0] is "get" ? Form.Bulk.Get
                : arg[0] is "shop" ? Form.Shop.Exchange
                : null;

            int idxCat = bulk.Category.IndexOf(selectedCategory);
            if (idxCat > -1)
            {
                bulk.CategoryIndex = idxCat;
            }

            int idxTier = bulk.Tier.IndexOf(selectedTier);
            if (idxTier > -1 && selectedTier.Length > 0)
            {
                bulk.TierIndex = idxTier;
            }

            // FIXES : 'bulk.Currency' ObservableCollection need to be loaded in View. 
            System.Threading.Tasks.Task.Run(async () =>
            {
                int watchdog = 0;
                // 2 seconds max
                while (bulk.Currency.Count is 0 && watchdog < 10)
                {
                    bulk.CategoryIndex = -1;
                    await System.Threading.Tasks.Task.Delay(100);
                    bulk.CategoryIndex = idxCat;
                    await System.Threading.Tasks.Task.Delay(100);
                    watchdog++;
                }

                int idxCur = bulk.Currency.IndexOf(selectedCurrency);
                if (idxCur > -1)
                {
                    bulk.CurrencyIndex = idxCur;
                }
            });
        }
    }
}
