using System;
using System.Reflection;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.Library.Services;

public sealed class AutoUpdaterService(INavigationService navigation, 
    IMessageAdapterService message, INetService net) : IAutoUpdaterService
{
    private readonly INavigationService _navigation = navigation;
    private readonly IMessageAdapterService _message = message;
    private readonly INetService _net = net;

    private const string ASSETNAME = "Xiletrade_win-x64.7z";

    public async Task CheckUpdateAsync(bool manualCheck = false)
    {
        try
        {
            var release = await CheckForUpdateAsync(manualCheck);
            if (release is not null)
            {
                _navigation.ShowUpdateView(release);
            }
        }
        catch (Exception ex)
        {
            _message.Show(ex.GetFormated(),"Failed to check for Xiletrade updates", MessageStatus.Exclamation);
        }
    }

    private async Task<GitHubRelease> CheckForUpdateAsync(bool manualCheck)
    {
        var release = await _net.GetFromJsonAsync<GitHubRelease>(Strings.GitHubApiLatestRelease, Client.GitHub);
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
