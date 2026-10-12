using System.Collections.Generic;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.ViewModels.Update;

namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Provides operations for downloading and extracting application updates.
/// </summary>
public interface IUpdateDownloader
{
    string DownloadPath { get; }
    string InstallationPath { get; }
    List<string> ListUpdaterFiles { get; }

    Task<string> DownloadAndExtractUpdateAsync(GitHubRelease release, DownloadStatusViewModel status);
    string ExtractUpdate(GitHubRelease release);
}
