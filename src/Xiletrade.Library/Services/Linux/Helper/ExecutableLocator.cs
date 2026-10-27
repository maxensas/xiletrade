using System;
using System.IO;

namespace Xiletrade.Library.Services.Linux.Helper;

/// <summary>
/// Finds an executable in the directories of the PATH environment variable.
/// </summary>
internal static class ExecutableLocator
{
    /// <param name="name">File name, without directory.</param>
    /// <param name="pathVariable">Value of PATH, by default the one of the process.</param>
    /// <returns>The full path, or null when the executable is not found.</returns>
    internal static string Find(string name, string pathVariable = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('/'))
        {
            return null;
        }

        pathVariable ??= Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
        {
            return null;
        }

        foreach (var directory in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(directory, name);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch (ArgumentException)
            {
                // invalid path characters in PATH entry : ignore it
            }
        }
        return null;
    }
}
