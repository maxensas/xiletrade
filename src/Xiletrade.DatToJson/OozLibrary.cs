using System.Reflection;
using System.Runtime.InteropServices;

namespace Xiletrade.DatToJson;

/// <summary>
/// Provides the native decompressor of the game bundles.
/// </summary>
/// <remarks>
/// libGGPK3 loads a native library named "oo2core" (Oodle, proprietary) to decompress bundles.
/// This tool ships ooz instead, an open source implementation of the same decompressors (GPL v3),
/// built from the Native folder: requests of libGGPK3 for "oo2core" are redirected to it.
/// </remarks>
internal static class OozLibrary
{
    private const string OodleLibraryName = "oo2core";

    private static int _registered;

    internal static string FileName
        => OperatingSystem.IsWindows() ? "ooz.dll"
        : OperatingSystem.IsMacOS() ? "libooz.dylib"
        : "libooz.so";

    internal static string FilePath => Path.Combine(AppContext.BaseDirectory, FileName);

    /// <summary>
    /// Redirects libGGPK3 to ooz. Must be called before the first bundle is read.
    /// </summary>
    /// <returns>False when the library was not built or not copied next to the executable.</returns>
    internal static bool TryRegister()
    {
        if (!File.Exists(FilePath))
        {
            return false;
        }
        // A resolver can only be set once per assembly.
        if (Interlocked.Exchange(ref _registered, 1) is 0)
        {
            NativeLibrary.SetDllImportResolver(typeof(LibBundle3.Oodle).Assembly, Resolve);
        }
        return true;
    }

    private static nint Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        => libraryName == OodleLibraryName ? NativeLibrary.Load(FilePath) : nint.Zero;
}
