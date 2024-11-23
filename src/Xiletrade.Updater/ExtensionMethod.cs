using System.IO.Compression;

namespace Xiletrade.Updater;

public static class ExtensionMethod
{
    public static bool IsDirectory(this ZipArchiveEntry entry)
    {
        return string.IsNullOrEmpty(entry.Name) && (entry.FullName.EndsWith("/") || entry.FullName.EndsWith(@"\"));
    }
}
