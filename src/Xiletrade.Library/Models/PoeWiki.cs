using Xiletrade.Library.Models.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal class PoeWiki
{
    internal string Link { get; private set; }

    internal PoeWiki(DataManagerService dm, ItemData item) // Poe Wiki only well done in english and russian.
    {
        string name = dm.Config.Options.Language is 0 or 6 ? item.Name : item.NameEn;
        string type = dm.Config.Options.Language is 0 or 6 ? item.Type : item.TypeEn;
        string url = dm.Config.Options.Language is 6 ? Strings.UrlPoeWikiRu : Strings.UrlPoeWiki;
        url += (item.Flag.Unique && name.Length > 0 ? name : type).Replace(' ', '_');

        Link = url;
    }
}
