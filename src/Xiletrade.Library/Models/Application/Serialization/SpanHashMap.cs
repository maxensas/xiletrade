using System;
using System.Buffers;
using System.Text;
using System.Text.Unicode;

namespace Xiletrade.Library.Models.Application.Serialization;

/// <summary>
/// Set of unique strings, searched by their UTF-8 bytes : a string already stored is found without being decoded nor allocated.
/// </summary>
/// <remarks>Not thread safe, see <see cref="StringCache"/>.</remarks>
public sealed class SpanHashMap
{
    private struct Entry
    {
        public int HashCode;   // hash of the UTF-8 bytes
        public string Value;   // stored string
        public int Next;       // index of next entry in chain (-1 if none)
    }

    private Entry[] _entries;
    private int[] _buckets;
    private int _count;

    private const int InitialCapacity = 16;
    private const int StackLimit = 256;

    public int Count => _count;

    public SpanHashMap(int capacity = InitialCapacity)
    {
        capacity = Math.Max(capacity, InitialCapacity);
        _buckets = new int[capacity];
        _buckets.AsSpan().Fill(-1);
        _entries = new Entry[capacity];
        _count = 0;
    }

    private static int GetHashCode(ReadOnlySpan<byte> utf8)
    {
        var hash = new HashCode();
        hash.AddBytes(utf8); // vectorized, seed is random by process (the map lives in the process only)
        return hash.ToHashCode() & 0x7FFFFFFF; // positive
    }

    /// <summary>
    /// Returns the stored string equal to <paramref name="utf8"/>, or stores a new one (the only allocation).
    /// </summary>
    public string GetOrAdd(ReadOnlySpan<byte> utf8)
    {
        int hashCode = GetHashCode(utf8);
        int bucket = hashCode % _buckets.Length;

        for (int i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
        {
            ref var entry = ref _entries[i];
            if (entry.HashCode == hashCode && Utf8Equals(utf8, entry.Value))
            {
                return entry.Value;
            }
        }

        if (_count == _entries.Length)
        {
            Resize();
            bucket = hashCode % _buckets.Length;
        }

        string value = Encoding.UTF8.GetString(utf8);
        int index = _count++;
        _entries[index].HashCode = hashCode;
        _entries[index].Value = value;
        _entries[index].Next = _buckets[bucket];
        _buckets[bucket] = index;

        return value;
    }

    // Compares UTF-8 bytes with the chars of a stored string (no copy, no allocation).
    private static bool Utf8Equals(ReadOnlySpan<byte> utf8, ReadOnlySpan<char> value)
    {
        // A non ascii char always takes more UTF-8 bytes than UTF-16 chars :
        // with the same length, the two are equal only if both are ascii.
        if (utf8.Length == value.Length)
        {
            return Ascii.Equals(utf8, value);
        }
        if (value.Length > utf8.Length)
        {
            return false;
        }

        char[] rented = null;
        Span<char> chars = utf8.Length <= StackLimit ? stackalloc char[StackLimit] : (rented = ArrayPool<char>.Shared.Rent(utf8.Length));
        try
        {
            var status = Utf8.ToUtf16(utf8, chars, out _, out int written);
            return status is OperationStatus.Done && chars[..written].SequenceEqual(value);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    private void Resize()
    {
        int newSize = _entries.Length * 2;
        var newBuckets = new int[newSize];
        newBuckets.AsSpan().Fill(-1);

        var newEntries = new Entry[newSize];
        Array.Copy(_entries, newEntries, _count);

        for (int i = 0; i < _count; i++)
        {
            int bucket = newEntries[i].HashCode % newSize;
            newEntries[i].Next = newBuckets[bucket];
            newBuckets[bucket] = i;
        }

        _buckets = newBuckets;
        _entries = newEntries;
    }
}
