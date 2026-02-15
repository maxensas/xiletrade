using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Services;

public partial class LocalizationService : ObservableObject
{
    private static IServiceProvider _serviceProvider;

    private readonly ResourceManager _rm = Resources.Resources.ResourceManager;

    private CultureInfo _currentCulture;

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            if (_currentCulture != value)
            {
                _currentCulture = value;
                OnPropertyChanged(string.Empty);
            }
        }
    }

    public LocalizationService(IServiceProvider service)
    {
        _serviceProvider = service;
    }

    public string this[string key] => _rm.GetString(key, CurrentCulture) ?? $"!{key}!";

    internal void RefreshCurrentCulture(int culture = -1, bool init = false)
    {
        if (init)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InstalledUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
            CurrentCulture = CultureInfo.InstalledUICulture;
            return;
        }
        
        var dm = _serviceProvider.GetRequiredService<DataManagerService>();
        var indexCulture = culture < 0 ? dm.Config.Options.Language : culture;
        CultureInfo cultureRefresh = CultureInfo.CreateSpecificCulture(Strings.Culture[indexCulture]);
        Thread.CurrentThread.CurrentCulture = cultureRefresh;
        Thread.CurrentThread.CurrentUICulture = cultureRefresh;
        CurrentCulture = cultureRefresh;
    }
}