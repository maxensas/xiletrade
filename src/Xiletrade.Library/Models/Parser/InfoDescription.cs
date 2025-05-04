using System;
using System.Linq;
using System.Text;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Parser;

internal sealed record InfoDescription
{
    internal bool IsPoeItem { get; }
    internal string[] Item { get; }

    //TODO use Span/Slice
    internal InfoDescription(string itemText)
    {
        StringBuilder sbItemText = new(itemText);
        // some "\r" are missing while copying directly from the game, not from website copy
        sbItemText.Replace(Strings.CRLF, Strings.LF).Replace(Strings.LF, Strings.CRLF).Replace("()", string.Empty);
        Item = sbItemText.ToString().ArrangeItemInfoDesc().Trim().Split([Strings.ItemInfoDelimiter], StringSplitOptions.None);
        if (Item[0].StartWith(Resources.Resources.General004_Rarity)) // Fix until GGG's update
        {
            Item[0] = Resources.Resources.General126_ItemClassPrefix + " " + Strings.NullClass + Strings.CRLF + Item[0];
        }

        IsPoeItem = Item.Length > 1 &&
        Item[0].StartWith(Resources.Resources.General126_ItemClassPrefix);

        if (Item[^1].Contain("~b/o")
        || Item[^1].Contain("~price"))
        {
            Item = Item.Where((source, index) => index != Item.Length - 1).ToArray(); // clipDataWhitoutPrice
        }
    }
}
