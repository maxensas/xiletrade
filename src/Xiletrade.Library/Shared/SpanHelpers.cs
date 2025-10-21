using System;
using System.Linq;

namespace Xiletrade.Library.Shared;

public static class SpanHelpers
{
    /// <summary>
    /// Replaces all occurrences of <paramref name="pattern"/> with <paramref name="replacement"/> in <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source text.</param>
    /// <param name="pattern">The string to search for.</param>
    /// <param name="replacement">The replacement string.</param>
    /// <returns>A new string with the replacements made.</returns>
    public static string ReplaceSpan(ReadOnlySpan<char> source, ReadOnlySpan<char> pattern, ReadOnlySpan<char> replacement)
    {
        if (pattern.Length is 0)
            throw new ArgumentException("The pattern cannot be empty.", nameof(pattern));

        // Count the number of pattern occurrences in source to size the buffer
        int count = CountOccurrences(source, pattern);
        if (count is 0)
            return source.ToString(); // nothing to replace

        // Calculate the size of the result after replacement
        int length = source.Length + count * (replacement.Length - pattern.Length);

        var buffer = new char[length];
        Span<char> dest = buffer.AsSpan();

        int srcPos = 0;
        int dstPos = 0;

        while (srcPos < source.Length)
        {
            if (srcPos + pattern.Length <= source.Length &&
                source.Slice(srcPos, pattern.Length).SequenceEqual(pattern))
            {
                // Copy replacement
                replacement.CopyTo(dest[dstPos..]);
                dstPos += replacement.Length;
                srcPos += pattern.Length;
            }
            else
            {
                dest[dstPos++] = source[srcPos++];
            }
        }

        return new string(buffer, 0, dstPos);
    }

    private static int CountOccurrences(ReadOnlySpan<char> source, ReadOnlySpan<char> pattern)
    {
        int count = 0;
        int pos = 0;

        while (pos <= source.Length - pattern.Length)
        {
            if (source.Slice(pos, pattern.Length).SequenceEqual(pattern))
            {
                count++;
                pos += pattern.Length;
            }
            else
            {
                pos++;
            }
        }

        return count;
    }
}
