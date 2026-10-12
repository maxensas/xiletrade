using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Resources;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Services;

/// <summary>
/// Provides access to localized resources and manages the current UI culture.
/// </summary>
/// <param name="dm"></param>
public partial class LocalizationService : ObservableObject
{
    private readonly DataManagerService _dm;
    private readonly ResourceManager _rm;

    public LocalizationService(ILogger<LocalizationService> logger, DataManagerService dm)
    {
        _dm = dm;
        _rm = Resources.Resources.ResourceManager;

#if DEBUG
        logger?.LogInformation("Service launched");
#endif
    }

    [ObservableProperty]
    private CultureInfo currentCulture = CultureInfo.CurrentUICulture;

    partial void OnCurrentCultureChanged(CultureInfo value) => OnPropertyChanged(string.Empty); // "Item[]"

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
        
        var indexCulture = culture < 0 ? _dm.Config.Options.Language : culture;
        var cultureRefresh = CultureInfo.CreateSpecificCulture(Strings.Culture[indexCulture]);
        CultureInfo.CurrentCulture = cultureRefresh;
        CultureInfo.CurrentUICulture = cultureRefresh;
        CurrentCulture = cultureRefresh;
    }
}