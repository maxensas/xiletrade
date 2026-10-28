using System;

namespace Xiletrade.Library.Models.Application.Serialization;

/// <summary>
/// Class used to handle a <see cref="SpanHashMap"/> cache : one instance by string value, found from its UTF-8 bytes.
/// </summary>
public abstract class StringCache
{
    private SpanHashMap _cache = new();
    private readonly object _lock = new();

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _cache.Count;
            }
        }
    }

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

    public void ResetCache()
    {
        lock (_lock)
        {
            _cache = new();
        }
    }
}
