using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models;

internal class PoeWiki
{
    internal string Link { get; private set; }

    internal PoeWiki(ItemBase item, string rarity) // Poe Wiki only well done in english and russian.
    {
        var currentItem = item;
        string name = DataManager.Config.Options.Language is 0 or 6 ? currentItem.Name : currentItem.NameEn;
        string type = DataManager.Config.Options.Language is 0 or 6 ? currentItem.Type : currentItem.TypeEn;
        string url = DataManager.Config.Options.Language is 6 ? Strings.UrlPoeWikiRu : Strings.UrlPoeWiki;
        url += (rarity == Resources.Resources.General006_Unique && name.Length > 0 ? name : type).Replace(' ', '_');

        Link = url;
    }
}
