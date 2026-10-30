using System;

namespace Xiletrade.Library.Models.Application.Serialization;

/// <summary>
/// Unique strings of the data files loaded together, found from their UTF-8 bytes (see <see cref="SpanHashMap"/>).
/// </summary>
/// <remarks>Thread safe : the files are deserialized in parallel.</remarks>
public sealed class StringCache
{
    private readonly SpanHashMap _cache = new();
    private readonly object _lock = new();

    /// <summary>
    /// Returns the cached string of these UTF-8 bytes (unescaped json text), a string is allocated only the first time.
    /// </summary>
    public string Intern(ReadOnlySpan<byte> utf8)
    {
        if (utf8.IsEmpty)
            return string.Empty;

        lock (_lock)
        {
            return _cache.GetOrAdd(utf8);
        }
    }
}
