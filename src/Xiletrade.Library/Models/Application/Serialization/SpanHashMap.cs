using System;
using System.Linq;

namespace Xiletrade.Library.Models.Application.Serialization;

/// <summary>
/// Class used to store unique strings using hash of Span.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class SpanHashMap<TValue>
{
    private struct Entry
    {
        public int HashCode;   // hash code of the key
        public string Key;     // stored key
        public TValue Value;
        public int Next;       // index of next entry in chain (-1 if none)
    }

    private Entry[] _entries;
    private int[] _buckets;
    private int _count;
    private int _freeList;

    private const int InitialCapacity = 16;

    public int Count => _count;

    public SpanHashMap(int capacity = InitialCapacity)
    {
        capacity = Math.Max(capacity, InitialCapacity);
        _buckets = new int[capacity];
        for (int i = 0; i < _buckets.Length; i++)
            _buckets[i] = -1;

        _entries = new Entry[capacity];
        _count = 0;
        _freeList = -1;
    }

    private static int GetHashCode(ReadOnlySpan<char> span)
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < span.Length; i++)
                hash = hash * 31 + span[i];
            return hash & 0x7FFFFFFF; // positive
        }
    }

    private static int GetHashCode(string s)
    {
        return GetHashCode(s.AsSpan());
    }

    public bool TryGetValue(ReadOnlySpan<char> keySpan, out TValue value)
    {
        int hashCode = GetHashCode(keySpan);
        int bucket = hashCode % _buckets.Length;

        for (int i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
        {
            ref var entry = ref _entries[i];
            if (entry.HashCode == hashCode && entry.Key.AsSpan().SequenceEqual(keySpan))
            {
                value = entry.Value;
                return true;
            }
        }

        value = default!;
        return false;
    }

    public bool ContainsKey(ReadOnlySpan<char> keySpan)
    {
        return TryGetValue(keySpan, out _);
    }

    public TValue AddOrGetExisting(string key, TValue value)
    {
        int hashCode = GetHashCode(key);
        int bucket = hashCode % _buckets.Length;

        for (int i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
        {
            ref var entry = ref _entries[i];
            if (entry.HashCode == hashCode && entry.Key == key)
            {
                return entry.Value;
            }
        }

        int index;
        if (_freeList >= 0)
        {
            index = _freeList;
            _freeList = _entries[index].Next;
        }
        else
        {
            if (_count == _entries.Length)
            {
                Resize();
                bucket = hashCode % _buckets.Length;
            }
            index = _count;
            _count++;
        }

        _entries[index].HashCode = hashCode;
        _entries[index].Key = key;
        _entries[index].Value = value;
        _entries[index].Next = _buckets[bucket];
        _buckets[bucket] = index;

        return value;
    }

    private void Resize()
    {
        int newSize = _entries.Length * 2;
        var newBuckets = new int[newSize];
        for (int i = 0; i < newBuckets.Length; i++)
            newBuckets[i] = -1;

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