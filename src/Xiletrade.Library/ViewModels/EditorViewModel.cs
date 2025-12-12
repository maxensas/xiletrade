using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Collection;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.ViewModels.Main;

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

    [ObservableProperty]
    private double viewScale;

    [ObservableProperty]
    private string poeSessId;

    public EditorViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dm = _serviceProvider.GetRequiredService<DataManagerService>();
        ViewScale = _dm.Config.Options.Scale;
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
    private async Task SaveChanges(object commandParameter)
    {
        _dm.Parser.Mods = [.. Parser.Where(x => x.Replace is "equals" or "contains" && x.Old.Length > 0 && x.New.Length > 0)];
        var fileToSave = _dm.Json.Serialize<ParserData>(_dm.Parser);
        await _dm.SaveFileAsync(fileToSave, ParserLocation);

        _dm.Config.DangerousMapMods = [.. DangerousMods.Where(x => x.Id.Length > 0 && x.Id.Contain("stat_"))];
        _dm.Config.RareItemMods = [.. RareMods.Where(x => x.Id.Length > 0 && x.Id.Contain("stat_"))];
        fileToSave = _dm.Json.Serialize<ConfigData>(_dm.Config);
        await _dm.SaveFileAsync(fileToSave, ConfigLocation);
    }

    [RelayCommand]
    private void AddPoeId(object commandParameter)
    {
        var messageService = _serviceProvider.GetRequiredService<IMessageAdapterService>();
        if (RegexUtil.MD5().IsMatch(PoeSessId))
        {
            _serviceProvider.GetRequiredService<ITokenService>().TryInitToken(PoeSessId, useCustom: true);
            _serviceProvider.GetRequiredService<XiletradeService>().RefreshAuthenticationState();

            var validity = _serviceProvider.GetRequiredService<ITokenService>().CustomToken is not null;
            if (validity)
            {
                messageService.Show("You entered a valid token.\n\n You need to restart the application to take effect.", "Token validation", MessageStatus.Information);
                return;
            }
        }
        messageService.Show("You entered an invalid token.", "Token validation", MessageStatus.Error);
        PoeSessId = string.Empty;
    }

    [RelayCommand]
    private void RemovePoeId(object commandParameter)
    {
        var messageService = _serviceProvider.GetRequiredService<IMessageAdapterService>();
        var tokenService = _serviceProvider.GetRequiredService<ITokenService>();

        var auth = tokenService.CacheToken is not null ? "\n0Auth token removed." : string.Empty;
        var poeid = tokenService.CustomToken is not null ? "\nPOESESSID token removed." : string.Empty;
        tokenService.ClearTokens();

        messageService.Show(string.Format("All tokens are now removed from your device.\n{0}{1}", auth, poeid)
            , "Token removal", MessageStatus.Information);

        PoeSessId = string.Empty;
    }

    [RelayCommand]
    private async Task Test(object commandParameter)
    {
        var messageService = _serviceProvider.GetRequiredService<IMessageAdapterService>();
        var mvm = _serviceProvider.GetRequiredService<MainViewModel>();
        if (!mvm.Authenticated)
        {
            messageService.Show("You are not authenticated", "Test command", MessageStatus.Information);
            return;
        }
        
        try
        {
            var service = _serviceProvider.GetRequiredService<NetService>();
            var idCur = "/" + GetPreviousHourUnixTimestamp();
            var sResult = await service.SendHTTP(Strings.CurrencyExchangeApi + idCur, Client.Xiletrade);
            //var sResult = await service.SendHTTP(Strings.ApiLeague, Client.Xiletrade);
            messageService.Show(string.Format("Request sent with success \r\n\r\n Response lenght: {0}"
                , sResult.Length), "Test command", MessageStatus.Information);
        }
        catch (Exception ex)
        {
            messageService.Show(string.Format("{0} Error:  {1}\r\n\r\n{2}\r\n\r\n"
                , ex.Source, ex.Message, ex.StackTrace), "Test error", MessageStatus.Error);
        }
    }

    private static long GetPreviousHourUnixTimestamp()
    {
        var nowLocal = DateTime.Now;

        var startOfCurrentHour = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day,
            nowLocal.Hour, 0, 0, nowLocal.Kind);

        var previousHour = startOfCurrentHour.AddHours(-1);
        var previousHourUtc = previousHour.ToUniversalTime();
        return new DateTimeOffset(previousHourUtc).ToUnixTimeSeconds();
    }

    [RelayCommand]
    private void InitVm(object commandParameter)
    {
        Parser.Clear();
        foreach (var modOption in _dm.Parser.Mods)
        {
            Parser.Add(new()
            {
                Id = modOption.Id,
                New = modOption.New,
                Old = modOption.Old,
                Replace = modOption.Replace,
                Stat = modOption.Stat
            });
        }
        SearchField = string.Empty;
        Filter.Clear();
        DangerousMods.Clear();

        foreach (var modOption in _dm.Config.DangerousMapMods)
        {
            DangerousMods.Add(new()
            {
                Id = modOption.Id,
                Text = modOption.Text
            });
        }

        RareMods.Clear();
        foreach (var modOption in _dm.Config.RareItemMods)
        {
            RareMods.Add(new()
            {
                Id = modOption.Id,
                Text = modOption.Text
            });
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

        var entrieMatches = _dm.Filter.Result.SelectMany(result => result.Entries)
            .Where(e => e.Text.Contain(SearchField));
        if (entrieMatches is not null)
        {
            int nb = 0;
            foreach (var match in entrieMatches)
            {
                Filter.Add(new()
                {
                    Num = nb,
                    Id = match.ID,
                    Type = match.Type,
                    Text = match.Text
                });
                nb++;
            }
        }
    }

    [RelayCommand]
    private void ShowDuplicates(object commandParameter)
    {
        Duplicate.Clear();

        var filter = _dm.Filter.Result.SelectMany(result => result.Entries);
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
            var duplicate = filter.Where(result => !result.ID.Equal(entrie.ID)
                  && result.Text.Equal(entrie.Text) && result.Type.Equal(entrie.Type));
            if (!duplicate.Any())
            {
                continue;
            }

            int nb = 0;
            Duplicate.Add(new()
            {
                Num = nb,
                Id = entrie.ID,
                Type = entrie.Type,
                Text = entrie.Text
            });

            foreach (var match in duplicate)
            {
                nb++;
                Duplicate.Add(new()
                {
                    Num = nb,
                    Id = match.ID,
                    Type = match.Type,
                    Text = match.Text
                });
            }
        }


    }
}
