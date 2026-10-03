using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

public sealed class AutoUpdaterService : IAutoUpdaterService
{
    private readonly IViewManager _view;
    private readonly IMessageAdapterService _message;
    private readonly INetService _net;

    public AutoUpdaterService(ILogger<AutoUpdaterService> logger, IViewManager view,
        IMessageAdapterService message, INetService net)
    {
        _view = view;
        _message = message;
        _net = net;

#if DEBUG
        logger.LogInformation("Service launched");
#endif
    }

    private const string ASSETNAME = "Xiletrade_win-x64.7z";

    public async Task CheckForUpdateAsync(bool manualCheck = false)
    {
        try
        {
            var release = await GetAvailableUpdateAsync(manualCheck);
            if (release is not null)
            {
                _view.ShowUpdateView(release);
            }
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(), Resources.Resources.Error020_XUpdateCheck, MessageStatus.Exclamation);
        }
    }

    /// <summary>
    /// Gets the latest compatible update if a newer version is available.
    /// </summary>
    private async Task<GitHubRelease> GetAvailableUpdateAsync(bool manualCheck)
    {
        var release = await _net.GetFromJsonAsync<GitHubRelease>(Strings.Github.ApiLatestRelease, Client.GitHub);
        if (release is null)
        {
            if (manualCheck)
            {
                _message.Show($@"{Resources.Resources.Update006_Error}",
                    $@"{Resources.Resources.Update009_TitleError}", MessageStatus.Error);
            }
            return null;
        }

        bool findAsset = false;
        foreach (var rel in release.Assets)
        {
            if (rel.Name is ASSETNAME)
            {
                findAsset = true;
                break;
            }
        }

        if (!findAsset)
        {
            if (manualCheck)
            {
                _message.Show($@"{Resources.Resources.Update006_Error}",
                    $@"{Resources.Resources.Update009_TitleError}", MessageStatus.Error);
            }
            return null;
        }
        
        var latestVersionStr = release.TagName.StartsWith('v') ? release.TagName[1..] : release.TagName;
        if (Version.TryParse(latestVersionStr, out var latestVersion))
        {
            var currentVersion = Assembly.GetEntryAssembly().GetName().Version ?? new Version(1, 0);
            if (latestVersion > currentVersion)
            {
                return release;
            }
            if (manualCheck)
            {
                _message.Show($@"{Resources.Resources.Update005_NoUpdate}",
                    $@"{Resources.Resources.Update008_TitleNoUpdate}", MessageStatus.Information);
            }
            return null;
        }

        if (manualCheck)
        {
            _message.Show($@"{Resources.Resources.Update006_Error}",
                $@"{Resources.Resources.Update009_TitleError}", MessageStatus.Error);
        }
        return null;
    }
}
