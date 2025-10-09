using System;
using System.Linq;
using System.Text;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain.Parser;

public sealed record InfoDescription
{
    internal bool IsPoeItem { get; }
    internal string[] Item { get; }

    public InfoDescription(ReadOnlySpan<char> itemText)
    {
        Item = NormalizeItemText(itemText).Split([Strings.ItemInfoDelimiter], StringSplitOptions.None);
        if (Item[0].AsSpan().StartWith(Resources.Resources.General004_Rarity)) // Fix if item class is not provided
        {
            Item[0] = $"{Resources.Resources.General126_ItemClassPrefix}{" "}{Strings.NullClass}{Strings.CRLF}{Item[0]}";
        }

        IsPoeItem = Item.Length > 1 && Item[0].AsSpan().StartWith(Resources.Resources.General126_ItemClassPrefix);

        if (Item[^1].AsSpan().Contain("~b/o") || Item[^1].AsSpan().Contain("~price"))
        {
            Item = [.. Item.Where((source, index) => index != Item.Length - 1)]; // clipDataWhitoutPrice
        }
    }

    /// <remarks>
    /// Focus on Readability : use sb instead of Span. 
    /// </remarks>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string NormalizeItemText(ReadOnlySpan<char> input)
    {
        StringBuilder sbItemText = new(input.ToString());
        // some "\r" are missing while copying directly from the game, not from website copy
        sbItemText.Replace(Strings.CRLF, Strings.LF).Replace(Strings.LF, Strings.CRLF).Replace("()", string.Empty);
        return sbItemText.ToString().ArrangeItemInfoDesc().Trim();
    }
}
