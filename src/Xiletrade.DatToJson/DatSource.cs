using LibBundledGGPK3;
using LibGGPK3;

namespace Xiletrade.DatToJson;

/// <summary>
/// Gives the raw bytes of dat files. Calibration and export only depend on this,
/// so the game files can come from a local installation or from the patch CDN.
/// </summary>
internal interface IDatSource : IDisposable
{
    /// <summary>Where the files come from, for the console and the reports.</summary>
    string Description { get; }

    /// <param name="path">Path inside the game index, with forward slashes (e.g. "data/mods.datc64").</param>
    /// <returns>Null when the file does not exist.</returns>
    byte[]? Read(string path);

    /// <summary>Checks that a file exists without reading it.</summary>
    bool Exists(string path);
}

/// <summary>
/// Reads dat files from a local Content.ggpk.
/// </summary>
internal sealed class GgpkDatSource : IDatSource
{
    private readonly BundledGGPK _ggpk;

    internal GgpkDatSource(string ggpkPath)
    {
        // Paths are not parsed: files are found by the hash of their path, which is much faster.
        _ggpk = new BundledGGPK(ggpkPath, parsePathsInIndex: false);
        Description = ggpkPath;
    }

    public string Description { get; }

    public byte[]? Read(string path)
        => _ggpk.Index.TryGetFile(path, out var file) ? file.Read().ToArray() : null;

    public bool Exists(string path) => _ggpk.Index.TryGetFile(path, out _);

    public void Dispose() => _ggpk.Dispose();
}

/// <summary>
/// Reads dat files from the patch CDN of the game, without any installation.
/// </summary>
/// <remarks>
/// The index (_.index.bin, a few MB) is downloaded on each run. A bundle is only downloaded when a file it contains
/// is read, then kept in the cache folder of this game version: a second run on the same version downloads nothing more.
/// </remarks>
internal sealed class CdnDatSource : IDatSource
{
    private const string IndexFile = "_.index.bin";

    private readonly ServerBundleFactory _factory;
    private readonly LibBundle3.Index _index;

    /// <param name="cdnUrl">Versioned CDN url given by the patch server: "https://&lt;cdn host&gt;/&lt;version&gt;/".</param>
    /// <param name="cacheDirectory">Root of the download cache, a sub folder is used for each game version.</param>
    internal CdnDatSource(string cdnUrl, string cacheDirectory)
    {
        CdnUrl = cdnUrl.EndsWith('/') ? cdnUrl : cdnUrl + "/";
        Version = GetVersion(CdnUrl);
        Description = $"patch CDN, version {Version} ({CdnUrl})";

        var directory = Path.Combine(cacheDirectory, Version);
        Directory.CreateDirectory(directory);
        _factory = new ServerBundleFactory(directory, CdnUrl);
        // Always downloaded again: an interrupted download would otherwise leave a broken index in the cache.
        _factory.DownloadIndex();
        _index = new LibBundle3.Index(Path.Combine(directory, IndexFile), parsePaths: false, _factory);
    }

    internal string CdnUrl { get; }

    /// <summary>Game version, last segment of the CDN url.</summary>
    internal string Version { get; }

    public string Description { get; }

    /// <summary>
    /// Asks the patch server of the game for the CDN url of its current version.
    /// </summary>
    internal static Task<string> GetCurrentCdnUrlAsync(int game)
        => PatchClient.GetPatchCdnUrlAsync(game is 2 ? PatchClient.ServerEndPoints.US2 : PatchClient.ServerEndPoints.US);

    public byte[]? Read(string path)
        => _index.TryGetFile(path, out var file) ? file.Read().ToArray() : null;

    public bool Exists(string path) => _index.TryGetFile(path, out _);

    public void Dispose()
    {
        _index.Dispose();
        _factory.Dispose();
    }

    private static string GetVersion(string cdnUrl)
    {
        var trimmed = cdnUrl.AsSpan().TrimEnd('/');
        var version = trimmed[(trimmed.LastIndexOf('/') + 1)..];
        if (version.IsEmpty)
        {
            throw new InvalidDataException($"No game version at the end of the CDN url: {cdnUrl}");
        }
        return new string(version);
    }
}
