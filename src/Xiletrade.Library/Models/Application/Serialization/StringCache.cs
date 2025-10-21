using System;

namespace Xiletrade.Library.Models.Application.Serialization;

/// <summary>
/// Class used to handle a SpanHashMap<string> cache
/// </summary>
public abstract class StringCache
{
    private SpanHashMap<string> _cache = new();
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

    public string Intern(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return string.Empty;

        lock (_lock)
        {
            if (_cache.TryGetValue(span, out var existing))
            {
                return existing;
            }

            string key = span.ToString();
            return _cache.AddOrGetExisting(key, key);
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