using Xiletrade.Library.Services;

namespace Xiletrade.Library.Models;

internal sealed class StringListFormat
{
    private int language = -1;

    private string _bulk;
    private string _shop;
    private string _shopAccount;
    internal string Bulk
    {
        get
        {
            InitCheck();
            return _bulk;
        }
    }
    internal string Shop
    {
        get
        {
            InitCheck();
            return _shop;
        }
    }
    internal string ShopAccount
    {
        get
        {
            InitCheck();
            return _shopAccount;
        }
    }

    private void InitCheck()
    {
        if (DataManager.Config.Options.Language != language)
        {
            _bulk = "{0,5} {1,-1} {2,5} {3}   " + Resources.Resources.Main014_ListStock + ": {4,-8} " + Resources.Resources.Main013_ListName + ": {5}";
            _shop = Resources.Resources.Main014_ListStock + " : {0,-8} {1,20} {2,-4} ⇐ {3,4} {4}";
            _shopAccount = Resources.Resources.Main206_tabItemShop + "  : {0} ({1})";
            language = DataManager.Config.Options.Language;
        }
    }
}
