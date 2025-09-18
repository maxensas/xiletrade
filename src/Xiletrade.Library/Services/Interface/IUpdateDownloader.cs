using System.Collections.Generic;
using System.Threading.Tasks;
using Xiletrade.Library.Models.GitHub.Contract;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.Library.Services.Interface;

public interface IUpdateDownloader
{
    public string DownloadPath { get; }
    public string InstallationPath { get; }
    public List<string> ListUpdaterFiles { get; }

    public Task<string> DownloadAndExtractUpdateAsync(GitHubRelease release, DownloadStatusViewModel status);
    public string ExtractUpdate(GitHubRelease release);
}
