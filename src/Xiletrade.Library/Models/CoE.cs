using System;
using System.Text;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

/// <summary>
/// Generate Craft Of Exile url link
/// </summary>
internal sealed class CoE
{
    internal string Link { get; private set; }

    internal CoE(string ClipboardText)
    {
        StringBuilder url = new(Strings.UrlCraftOfExile);
        Link = url.Append(Uri.EscapeDataString(ClipboardText)).ToString();
    }
}
