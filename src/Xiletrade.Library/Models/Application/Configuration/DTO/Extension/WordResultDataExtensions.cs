using System;

namespace Xiletrade.Library.Models.Application.Configuration.DTO.Extension;

internal static class WordResultDataExtensions
{
    public static bool MatchNameEn(this WordResultData[] words, 
        ReadOnlySpan<char> nameEn, ReadOnlySpan<char> itemName)
    {
        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i];
            if (word.NameEn.AsSpan().SequenceEqual(nameEn))
                return word.Name.AsSpan().SequenceEqual(itemName);
        }
        return false;
    }

    public static WordResultData FindWordByName(this WordResultData[] words,
        ReadOnlySpan<char> itemName)
    {
        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i];
            if (word.Name.AsSpan().SequenceEqual(itemName))
                return word;
        }
        return null;
    }

    public static WordResultData FindWordByNameEn(this WordResultData[] words,
        ReadOnlySpan<char> EnglishItemName)
    {
        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i];
            if (word.NameEn.AsSpan().SequenceEqual(EnglishItemName))
                return word;
        }
        return null;
    }
}

