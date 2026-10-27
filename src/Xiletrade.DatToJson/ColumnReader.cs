namespace Xiletrade.DatToJson;

/// <summary>
/// Reads a column value as a 64-bit fingerprint, so values can be compared between two game versions
/// without allocating strings.
/// </summary>
/// <remarks>
/// <see cref="Empty"/> means "no information": empty string, zero, or null reference.
/// Such values are ignored when scoring, because many unrelated columns share them.
/// </remarks>
internal static class ColumnReader
{
    internal const ulong Empty = 0;

    private const ulong FnvOffsetBasis = 0xCBF29CE484222325;
    private const ulong FnvPrime = 0x100000001B3;

    /// <summary>
    /// Reads the value of a column at an arbitrary offset.
    /// </summary>
    /// <param name="referencedKeys">
    /// For a foreign row: the key fingerprints of the referenced table, indexed by row.
    /// The reference is compared through the referenced key, because row indexes change when rows are inserted.
    /// </param>
    /// <returns>False when the bytes at this offset can not be a value of this type.</returns>
    internal static bool TryRead(DatFile dat, int row, int offset, ColumnType type, ulong[]? referencedKeys, out ulong value)
    {
        switch (type)
        {
            case ColumnType.String:
                if (dat.TryReadString(row, offset, out var text))
                {
                    value = Hash(text);
                    return true;
                }
                value = Empty;
                return false;

            case ColumnType.Int32:
                // Any 4 bytes are a valid integer: the score alone decides.
                value = (uint)dat.ReadInt32(row, offset);
                return true;

            case ColumnType.ForeignRow:
                value = Empty;
                if (!dat.TryReadForeignRow(row, offset, out var rowIndex))
                {
                    return false;
                }
                if (rowIndex is DatFile.NullRow)
                {
                    return true;
                }
                if (referencedKeys is null || rowIndex >= (ulong)referencedKeys.Length)
                {
                    return false;
                }
                value = referencedKeys[(int)rowIndex];
                return true;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    /// <summary>
    /// FNV-1a 64 bits, one UTF-16 code unit at a time. Stable across runs, so it can be stored in a snapshot.
    /// </summary>
    internal static ulong Hash(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
        {
            return Empty;
        }

        var hash = FnvOffsetBasis;
        foreach (var c in text)
        {
            hash ^= c;
            hash *= FnvPrime;
        }
        // Keep Empty reserved for missing values.
        return hash is Empty ? 1 : hash;
    }
}
