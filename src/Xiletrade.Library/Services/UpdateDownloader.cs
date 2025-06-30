using Microsoft.Extensions.DependencyInjection;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Enums;
using Xiletrade.Library.Models.Serializable;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.ViewModels;

namespace Xiletrade.Library.Services;

internal class UpdateDownloader : IUpdateDownloader
{
    private static IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;

    public string DownloadPath { get; } = Environment.CurrentDirectory;
    public string InstallationPath { get; } = string.Empty;
    public List<string> ListUpdaterFiles { get; } = new() { "Update.exe"/*, "av_libglesv2.dll", "libHarfBuzzSharp.dll", "libSkiaSharp.dll"*/ };

    public UpdateDownloader(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _httpClient = _serviceProvider.GetRequiredService<NetService>().GetClient(Client.GitHub);
        //var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // Application folder should be used in archive : 'Xiletrade' atm
        var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
        if (currentDirectory.Parent is not null) InstallationPath = currentDirectory.Parent.FullName;
    }

    public async Task<string> DownloadAndExtractUpdateAsync(GitHubRelease release, DownloadStatusViewModel status)
    {
        var zipAsset = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".7z", StringComparison.OrdinalIgnoreCase));
        if (zipAsset is null)
            return null;

        //string assemblyName = AppDomain.CurrentDomain.FriendlyName;

        Directory.CreateDirectory(DownloadPath);
        var archiveFile = Path.Combine(DownloadPath, zipAsset.Name);

        using var response = await _httpClient.GetAsync(zipAsset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;
        status.IsIndeterminate = contentLength is null;

        var totalBytes = contentLength ?? -1L;
        var totalRead = 0L;
        var buffer = new byte[8192];
        var isMoreToRead = true;

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(archiveFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        do
        {
            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
            if (read is 0)
            {
                isMoreToRead = false;
                continue;
            }

            await fileStream.WriteAsync(buffer, 0, read);
            totalRead += read;

            if (totalBytes is not -1)
            {
                status.DownloadProgress = (int)((totalRead * 100L) / totalBytes);
            }

        } while (isMoreToRead);
        contentStream.Dispose();
        fileStream.Dispose();

        status.DownloadProgress = 100;
        status.IsIndeterminate = false;

        var verified = await VerifySha256Async(archiveFile, release);
        if (!verified)
            throw new InvalidOperationException("SHA256 verification failed. Update aborted.");

        ExtractSevenArchive(archiveFile);
        return archiveFile;
    }

    public string ExtractUpdate(GitHubRelease release)
    {
        var zipAsset = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".7z", StringComparison.OrdinalIgnoreCase));
        if (zipAsset is null)
            return null;

        Directory.CreateDirectory(DownloadPath);
        var archiveFile = Path.Combine(DownloadPath, zipAsset.Name);
        ExtractSevenArchive(archiveFile);
        return archiveFile;
    }

    private void ExtractSevenArchive(string zipFile, string extractPath = null)
    {
        var archive = SevenZipArchive.Open(zipFile);
        IReader reader = archive.ExtractAllEntries();
        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory)
            {
                continue;
            }
            if (ListUpdaterFiles.Contains(Path.GetFileName(reader.Entry.ToString())))
            {
                reader.WriteEntryToDirectory(Path.GetDirectoryName(zipFile), new ExtractionOptions()
                {
                    ExtractFullPath = false,
                    Overwrite = true
                });
                break;
            }
        }
        archive.Dispose();
    }

    private static async Task<bool> VerifySha256Async(string zipPath, GitHubRelease release)
    {
        var zipFileName = Path.GetFileName(zipPath);

        var hashAsset = release.Assets.FirstOrDefault(a =>
            a.Name.EndsWith(".7z") && a.Digest.StartsWith("sha256"));
        if (hashAsset is null)
            return false;

        var digest = hashAsset.Digest.Split(':');
        if (digest.Length is not 2)
            return false;
        var githubHash = digest[1];

        using var sha256 = SHA256.Create();
        await using var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read);
        var hashBytes = await sha256.ComputeHashAsync(fs);
        var actualHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return actualHash.Equals(githubHash, StringComparison.InvariantCultureIgnoreCase);
    }
}
