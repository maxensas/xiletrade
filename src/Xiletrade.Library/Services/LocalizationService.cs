using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Resources;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Services;

public partial class LocalizationService : ObservableObject
{
    private static IServiceProvider _serviceProvider;

    private readonly ResourceManager _rm = Resources.Resources.ResourceManager;
    
    [ObservableProperty]
    private CultureInfo currentCulture = CultureInfo.CurrentUICulture;

    partial void OnCurrentCultureChanged(CultureInfo value) => OnPropertyChanged(string.Empty); // "Item[]"

    public LocalizationService(IServiceProvider service)
    {
        _serviceProvider = service;
    }

    public string this[string key] => _rm.GetString(key, CurrentCulture) ?? $"!{key}!";

    internal void RefreshCurrentCulture(int culture = -1, bool init = false)
    {
        if (init)
        {
            var installed = CultureInfo.InstalledUICulture;
            CultureInfo.CurrentCulture = installed;
            CultureInfo.CurrentUICulture = installed;
            CurrentCulture = installed;
            return;
        }
        
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var indexCulture = culture < 0 ? dm.Config.Options.Language : culture;
        var cultureRefresh = CultureInfo.CreateSpecificCulture(Strings.Culture[indexCulture]);
        CultureInfo.CurrentCulture = cultureRefresh;
        CultureInfo.CurrentUICulture = cultureRefresh;
        CurrentCulture = cultureRefresh;
    }
}