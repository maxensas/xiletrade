using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared;

namespace Xiletrade.UI.WPF.Services;

public sealed class AutoUpdaterService : IAutoUpdaterService
{
    private static IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;
    private const string ASSETNAME = "Xiletrade_win-x64.7z";

    public AutoUpdaterService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;        
        _httpClient = _serviceProvider.GetRequiredService<NetService>().GetClient(Client.GitHub);
    }

    public Task CheckUpdate(bool manualCheck = false)
    {
        return Task.Run(async () =>
        {
            var release = await CheckForUpdateAsync(manualCheck);
            if (release is not null)
            {
                _serviceProvider.GetRequiredService<INavigationService>().ShowUpdateView(release);
            }
        });
    }

    private async Task<GitHubRelease> CheckForUpdateAsync(bool manualCheck)
    {
        var ms = _serviceProvider.GetRequiredService<IMessageAdapterService>();
        var release = await _httpClient.GetFromJsonAsync<GitHubRelease>(Strings.GitHubApiLatestRelease);
        if (release is null)
        {
            if (manualCheck)
            {
                ms.Show($@"{Library.Resources.Resources.Update006_Error}",
                    $@"{Library.Resources.Resources.Update009_TitleError}", MessageStatus.Error);
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
                ms.Show($@"{Library.Resources.Resources.Update006_Error}",
                    $@"{Library.Resources.Resources.Update009_TitleError}", MessageStatus.Error);
            }
            return null;
        }

        var latestVersionStr = release.TagName.StartsWith('v') ? release.TagName[1..] : release.TagName;
        if (Version.TryParse(latestVersionStr, out var latestVersion))
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0);
            if (latestVersion > currentVersion)
            {
                return release;
            }
            if (manualCheck)
            {
                ms.Show($@"{Library.Resources.Resources.Update005_NoUpdate}",
                    $@"{Library.Resources.Resources.Update008_TitleNoUpdate}", MessageStatus.Information);
            }
            return null;
        }
        
        if (manualCheck)
        {
            ms.Show($@"{Library.Resources.Resources.Update006_Error}",
                $@"{Library.Resources.Resources.Update009_TitleError}", MessageStatus.Error);
        }
        return null;
    }
}
