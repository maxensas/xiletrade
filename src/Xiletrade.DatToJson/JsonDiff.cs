using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Xiletrade.DatToJson;

/// <summary>
/// Compares two generations of a Xiletrade json file and writes the differences in the git unified format.
/// </summary>
/// <remarks>
/// The json files are written on a single line: to get a readable diff, every entry is seen as one line
/// (entries of <c>result[n].data</c>, <c>result[n].entries</c> with the header of their group, <c>result</c>, or <c>item_class</c>).
/// Other json files are seen as one line.
/// Entries are compared on their raw UTF-8 bytes, without allocating strings.
/// Myers algorithm (the default one of git), applied between the common first and last entries.
/// Only the changed entries are written, without context (like <c>git diff -U0</c>): the files have thousands of entries,
/// and an entry is readable on its own.
/// An entry removed at one place and added unchanged at another one (rows reordered by the game) is a moved entry:
/// it is still written as '-' and '+' (the diff stays a valid patch), its hunks are marked "moved: n",
/// and it is counted apart, neither removed nor added.
/// </remarks>
internal static class JsonDiff
{
    /// <summary>Unchanged entries shown around a change: none, the diff can be long.</summary>
    private const int Context = 0;

    /// <summary>Above this number of changed entries, the file is shown as entirely replaced (memory stays bounded).</summary>
    private const int MaxEdits = 4000;

    /// <param name="Removed">Entries removed, moved entries excluded.</param>
    /// <param name="Added">Entries added, moved entries excluded.</param>
    /// <param name="Moved">Unchanged entries found at another position.</param>
    internal readonly record struct Stats(int Removed, int Added, int Moved);

    private enum Op : byte
    {
        Equal,
        Delete,
        Insert
    }

    /// <summary>Old and New are line indexes, -1 when the line is not on that side.</summary>
    private readonly record struct Edit(Op Op, int Old, int New);

    /// <summary>
    /// Compares two generations and, when <paramref name="output"/> is given, writes their diff.
    /// </summary>
    /// <param name="path">Path shown in the diff, relative to the output folder, with '/'.</param>
    /// <param name="oldJson">Previous file, without byte order mark. Empty for a new file.</param>
    /// <param name="newJson">New file, without byte order mark.</param>
    internal static Stats Compare(ReadOnlyMemory<byte> oldJson, ReadOnlyMemory<byte> newJson, Stream? output, string path, bool newFile)
    {
        var a = new EntryLines(oldJson);
        var b = new EntryLines(newJson);
        var edits = GetEdits(a, b);

        int removed = 0, added = 0;
        foreach (var edit in edits)
        {
            if (edit.Op is Op.Delete)
            {
                removed++;
            }
            else if (edit.Op is Op.Insert)
            {
                added++;
            }
        }
        if (removed + added is 0)
        {
            return default;
        }

        var moved = FindMoved(a, b, edits, out var moves);
        if (output is not null)
        {
            WriteUnified(output, path, newFile, a, b, edits, moved);
        }
        return new Stats(removed - moves, added - moves, moves);
    }

    /// <summary>
    /// Pairs every removed entry with an identical added entry, if any.
    /// </summary>
    /// <param name="moves">Number of pairs, so of moved entries.</param>
    /// <returns>For each edit, true when it is one side of a moved entry. Empty when nothing moved.</returns>
    private static bool[] FindMoved(EntryLines a, EntryLines b, List<Edit> edits, out int moves)
    {
        moves = 0;
        // Removed entries by hash: edit indexes, in file order.
        Dictionary<ulong, List<int>>? removed = null;
        for (var i = 0; i < edits.Count; i++)
        {
            if (edits[i].Op is Op.Delete)
            {
                removed ??= [];
                var hash = a.HashAt(edits[i].Old);
                if (!removed.TryGetValue(hash, out var list))
                {
                    removed[hash] = list = [];
                }
                list.Add(i);
            }
        }
        if (removed is null)
        {
            return [];
        }

        bool[]? moved = null;
        for (var i = 0; i < edits.Count; i++)
        {
            if (edits[i].Op is not Op.Insert || !removed.TryGetValue(b.HashAt(edits[i].New), out var candidates))
            {
                continue;
            }
            for (var c = 0; c < candidates.Count; c++)
            {
                var deleted = candidates[c];
                if (EntryLines.Same(a, edits[deleted].Old, b, edits[i].New))
                {
                    moved ??= new bool[edits.Count];
                    moved[deleted] = moved[i] = true;
                    candidates.RemoveAt(c);
                    moves++;
                    break;
                }
            }
        }
        return moved ?? [];
    }

    private static List<Edit> GetEdits(EntryLines a, EntryLines b)
    {
        // Common first and last entries: most game updates only change a few entries.
        var prefix = 0;
        while (prefix < a.Count && prefix < b.Count && EntryLines.Same(a, prefix, b, prefix))
        {
            prefix++;
        }
        var suffix = 0;
        while (suffix < a.Count - prefix && suffix < b.Count - prefix
            && EntryLines.Same(a, a.Count - 1 - suffix, b, b.Count - 1 - suffix))
        {
            suffix++;
        }

        int n = a.Count - prefix - suffix, m = b.Count - prefix - suffix;
        var edits = new List<Edit>(a.Count + m);
        for (var i = 0; i < prefix; i++)
        {
            edits.Add(new Edit(Op.Equal, i, i));
        }

        var middle = Myers(a, b, prefix, n, m);
        if (middle is null)
        {
            // Too many changes: everything between the common entries is replaced.
            for (var i = 0; i < n; i++)
            {
                edits.Add(new Edit(Op.Delete, prefix + i, -1));
            }
            for (var j = 0; j < m; j++)
            {
                edits.Add(new Edit(Op.Insert, -1, prefix + j));
            }
        }
        else
        {
            edits.AddRange(middle);
        }

        for (var t = 0; t < suffix; t++)
        {
            edits.Add(new Edit(Op.Equal, a.Count - suffix + t, b.Count - suffix + t));
        }
        return edits;
    }

    /// <summary>
    /// Shortest edit script between a[start..start+n] and b[start..start+m].
    /// </summary>
    /// <returns>Null when more than <see cref="MaxEdits"/> edits are needed.</returns>
    private static List<Edit>? Myers(EntryLines a, EntryLines b, int start, int n, int m)
    {
        if (n + m is 0)
        {
            return [];
        }

        // v[offset + k]: furthest x reached on diagonal k. trace[d]: copy of v for k in [-d-1, d+1], before step d.
        var max = n + m;
        var offset = max + 1;
        var v = new int[2 * max + 3];
        List<int[]> trace = [];

        for (var d = 0; d <= max; d++)
        {
            if (d > MaxEdits)
            {
                return null;
            }
            var snapshot = new int[2 * d + 3];
            Array.Copy(v, offset - d - 1, snapshot, 0, snapshot.Length);
            trace.Add(snapshot);

            for (var k = -d; k <= d; k += 2)
            {
                var x = k == -d || (k != d && v[offset + k - 1] < v[offset + k + 1])
                    ? v[offset + k + 1]
                    : v[offset + k - 1] + 1;
                var y = x - k;
                while (x < n && y < m && EntryLines.Same(a, start + x, b, start + y))
                {
                    x++;
                    y++;
                }
                v[offset + k] = x;
                if (x >= n && y >= m)
                {
                    return Backtrack(trace, start, n, m);
                }
            }
        }
        return null;
    }

    private static List<Edit> Backtrack(List<int[]> trace, int start, int n, int m)
    {
        var edits = new List<Edit>(n + m);
        int x = n, y = m;
        for (var d = trace.Count - 1; d >= 0; d--)
        {
            var snapshot = trace[d];
            var k = x - y;
            var previousK = k == -d || (k != d && snapshot[k - 1 + d + 1] < snapshot[k + 1 + d + 1]) ? k + 1 : k - 1;
            var previousX = snapshot[previousK + d + 1];
            var previousY = previousX - previousK;

            while (x > previousX && y > previousY)
            {
                edits.Add(new Edit(Op.Equal, start + x - 1, start + y - 1));
                x--;
                y--;
            }
            if (d > 0)
            {
                edits.Add(x == previousX
                    ? new Edit(Op.Insert, -1, start + y - 1)
                    : new Edit(Op.Delete, start + x - 1, -1));
            }
            x = previousX;
            y = previousY;
        }
        edits.Reverse();
        return edits;
    }

    /// <param name="moved">For each edit, true when it is one side of a moved entry, or empty.</param>
    private static void WriteUnified(Stream output, string path, bool newFile, EntryLines a, EntryLines b, List<Edit> edits, bool[] moved)
    {
        WriteText(output, $"diff --git a/{path} b/{path}\n");
        WriteText(output, newFile ? "new file mode 100644\n--- /dev/null\n" : $"--- a/{path}\n");
        WriteText(output, $"+++ b/{path}\n");

        var index = 0;
        while (index < edits.Count)
        {
            // Next change, then the end of its hunk: changes separated by 2 * Context equal lines or less are grouped, like git
            // (without context, only consecutive changes).
            while (index < edits.Count && edits[index].Op is Op.Equal)
            {
                index++;
            }
            if (index == edits.Count)
            {
                break;
            }

            var hunkStart = Math.Max(0, index - Context);
            var hunkEnd = index;
            var lastChange = index;
            while (hunkEnd < edits.Count && hunkEnd - lastChange <= 2 * Context + 1)
            {
                if (edits[hunkEnd].Op is not Op.Equal)
                {
                    lastChange = hunkEnd;
                }
                hunkEnd++;
            }
            hunkEnd = Math.Min(edits.Count, lastChange + Context + 1);

            WriteHunk(output, a, b, edits, moved, hunkStart, hunkEnd);
            index = hunkEnd;
        }
    }

    private static void WriteHunk(Stream output, EntryLines a, EntryLines b, List<Edit> edits, bool[] moved, int start, int end)
    {
        int oldStart = -1, newStart = -1, oldCount = 0, newCount = 0, movedCount = 0;
        for (var i = start; i < end; i++)
        {
            var edit = edits[i];
            if (moved.Length > 0 && moved[i])
            {
                movedCount++;
            }
            if (edit.Op is not Op.Insert)
            {
                oldCount++;
                if (oldStart < 0)
                {
                    oldStart = edit.Old;
                }
            }
            if (edit.Op is not Op.Delete)
            {
                newCount++;
                if (newStart < 0)
                {
                    newStart = edit.New;
                }
            }
        }

        // Git numbers lines from 1; for an empty side, it gives the line before the hunk.
        oldStart = oldCount > 0 ? oldStart + 1 : start > 0 ? edits[start - 1].Old + 1 : 0;
        newStart = newCount > 0 ? newStart + 1 : start > 0 ? edits[start - 1].New + 1 : 0;
        WriteHunkHeader(output, oldStart, oldCount, newStart, newCount, movedCount);

        for (var i = start; i < end; i++)
        {
            var edit = edits[i];
            switch (edit.Op)
            {
                case Op.Equal:
                    WriteLine(output, (byte)' ', a[edit.Old]);
                    break;
                case Op.Delete:
                    WriteLine(output, (byte)'-', a[edit.Old]);
                    break;
                default:
                    WriteLine(output, (byte)'+', b[edit.New]);
                    break;
            }
        }
    }

    /// <summary>
    /// "@@ -start,count +start,count @@", followed by "moved: n" when the hunk has moved entries, written without allocation.
    /// </summary>
    /// <remarks>Git ignores the text after the second "@@" (it shows the function name there).</remarks>
    private static void WriteHunkHeader(Stream output, int oldStart, int oldCount, int newStart, int newCount, int movedCount)
    {
        Span<byte> buffer = stackalloc byte[96];
        "@@ -"u8.CopyTo(buffer);
        var written = 4;
        AppendRange(buffer, ref written, oldStart, oldCount);
        " +"u8.CopyTo(buffer[written..]);
        written += 2;
        AppendRange(buffer, ref written, newStart, newCount);
        " @@"u8.CopyTo(buffer[written..]);
        written += 3;
        if (movedCount > 0)
        {
            " moved: "u8.CopyTo(buffer[written..]);
            written += 8;
            movedCount.TryFormat(buffer[written..], out var length, default, CultureInfo.InvariantCulture);
            written += length;
        }
        buffer[written++] = (byte)'\n';
        output.Write(buffer[..written]);
    }

    /// <summary>Git omits the count when it is 1.</summary>
    private static void AppendRange(Span<byte> buffer, ref int written, int start, int count)
    {
        start.TryFormat(buffer[written..], out var length, default, CultureInfo.InvariantCulture);
        written += length;
        if (count != 1)
        {
            buffer[written++] = (byte)',';
            count.TryFormat(buffer[written..], out length, default, CultureInfo.InvariantCulture);
            written += length;
        }
    }

    private static void WriteLine(Stream output, byte prefix, ReadOnlySpan<byte> line)
    {
        output.WriteByte(prefix);
        output.Write(line);
        output.WriteByte((byte)'\n');
    }

    private static void WriteText(Stream output, string text)
    {
        Span<byte> buffer = stackalloc byte[512];
        var length = Encoding.UTF8.GetMaxByteCount(text.Length);
        if (length <= buffer.Length)
        {
            output.Write(buffer[..Encoding.UTF8.GetBytes(text, buffer)]);
        }
        else
        {
            output.Write(Encoding.UTF8.GetBytes(text));
        }
    }

    /// <summary>
    /// The entries of a json file, as slices of its UTF-8 bytes, with a hash for fast comparison.
    /// </summary>
    private sealed class EntryLines
    {
        private readonly ReadOnlyMemory<byte> _json;
        private readonly Range[] _ranges;
        private readonly ulong[] _hashes;

        internal EntryLines(ReadOnlyMemory<byte> json)
        {
            _json = json;
            _ranges = Split(json);
            _hashes = new ulong[_ranges.Length];
            for (var i = 0; i < _ranges.Length; i++)
            {
                _hashes[i] = Hash(json.Span[_ranges[i]]);
            }
        }

        internal int Count => _ranges.Length;

        internal ReadOnlySpan<byte> this[int index] => _json.Span[_ranges[index]];

        internal ulong HashAt(int index) => _hashes[index];

        internal static bool Same(EntryLines a, int i, EntryLines b, int j)
            => a._hashes[i] == b._hashes[j] && a[i].SequenceEqual(b[j]);

        private static Range[] Split(ReadOnlyMemory<byte> json)
        {
            if (json.IsEmpty)
            {
                return [];
            }
            try
            {
                using var document = JsonDocument.Parse(json);
                List<Range> ranges = [];
                if (AddEntries(document.RootElement, json.Span, ranges))
                {
                    return [.. ranges];
                }
            }
            catch (JsonException)
            {
                // Not a json file of the expected shape: compared as a whole.
            }
            return [Range.All];
        }

        /// <summary>
        /// One line per entry, as raw bytes of the file:
        /// <list type="bullet">
        /// <item><c>item_class</c>: each item class;</item>
        /// <item><c>result</c>, for each element: each entry of its <c>data</c> (bases, mods, words, gems), or its header then each
        /// entry of its <c>entries</c> (filters, items, currency: the header is the group up to its entries, e.g. <c>{"label":"Pseudo","entries":[</c>),
        /// otherwise the element itself (leagues).</item>
        /// </list>
        /// </summary>
        /// <returns>False when the file does not have one of these shapes.</returns>
        private static bool AddEntries(JsonElement root, ReadOnlySpan<byte> json, List<Range> ranges)
        {
            if (root.ValueKind is not JsonValueKind.Object)
            {
                return false;
            }
            if (root.TryGetProperty("item_class"u8, out var itemClasses) && itemClasses.ValueKind is JsonValueKind.Array)
            {
                return AddItems(itemClasses, json, ranges);
            }
            if (!root.TryGetProperty("result"u8, out var result) || result.ValueKind is not JsonValueKind.Array)
            {
                return false;
            }

            foreach (var element in result.EnumerateArray())
            {
                if (element.ValueKind is JsonValueKind.Object
                    && element.TryGetProperty("data"u8, out var data) && data.ValueKind is JsonValueKind.Array)
                {
                    if (!AddItems(data, json, ranges))
                    {
                        return false;
                    }
                }
                else if (element.ValueKind is JsonValueKind.Object
                    && element.TryGetProperty("entries"u8, out var entries) && entries.ValueKind is JsonValueKind.Array)
                {
                    if (!TryGetPosition(element, json, out var groupStart, out _)
                        || !TryGetPosition(entries, json, out var entriesStart, out _)
                        || entriesStart < groupStart)
                    {
                        return false;
                    }
                    ranges.Add(new Range(groupStart, entriesStart + 1));
                    if (!AddItems(entries, json, ranges))
                    {
                        return false;
                    }
                }
                else if (!AddItem(element, json, ranges))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool AddItems(JsonElement array, ReadOnlySpan<byte> json, List<Range> ranges)
        {
            foreach (var item in array.EnumerateArray())
            {
                if (!AddItem(item, json, ranges))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool AddItem(JsonElement item, ReadOnlySpan<byte> json, List<Range> ranges)
        {
            if (!TryGetPosition(item, json, out var start, out var length))
            {
                return false;
            }
            ranges.Add(new Range(start, start + length));
            return true;
        }

        /// <summary>Position of the raw bytes of a value inside the parsed buffer: only its position is kept.</summary>
        private static bool TryGetPosition(JsonElement element, ReadOnlySpan<byte> json, out int start, out int length)
        {
            var raw = JsonMarshal.GetRawUtf8Value(element);
            length = raw.Length;
            return json.Overlaps(raw, out start);
        }

        /// <summary>FNV-1a 64 bits.</summary>
        private static ulong Hash(ReadOnlySpan<byte> bytes)
        {
            var hash = 0xCBF29CE484222325;
            foreach (var b in bytes)
            {
                hash ^= b;
                hash *= 0x100000001B3;
            }
            return hash;
        }
    }
}
