namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class Url
    {
        internal const string Poelab = "https://www.poelab.com/";
        internal const string PaypalDonate = "https://www.paypal.me/maxensas";
        // old: https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9TEA8EMSSB846
        internal const string GithubData = "https://raw.githubusercontent.com/maxensas/xiletrade/master/Xiletrade/Data/";
        internal const string XiletradeAuth = "https://maxensas.github.io/xiletrade-oauth/poe";
        internal const string Changelog = "https://github.com/maxensas/xiletrade/releases";
        internal const string PoeRegex = "https://poe.re";
        internal const string PoeRegexKr = "https://poeregexkr.web.app";
        internal const string PoeRegexJp = "https://ochi3.github.io/poe-jp-regex";
        internal const string PoeRegexRu = "https://path-of-regex-ru.vercel.app";

        internal static string PoeWiki { get => IsPoe2 ? _poeWiki2 : _poeWiki1; }
        internal static string PoeWikiRu { get => IsPoe2 ? _poeWiki2 : _poeWikiRu; }
        internal static string Poedb { get => IsPoe2 ? _poedb2 : _poedb1; }
        internal static string PoedbHost { get => IsPoe2 ? _poedbHost2 : _poedbHost1; }
        internal static string CraftOfExile { get => IsPoe2 ? _craftOfExile2 : _craftOfExile1; }
        internal static string PoeNinja { get => IsPoe2 ? _poeNinja2 : _poeNinja1; }
        internal static string Trade { get => IsPoe2 ? _trade2[Gateway] : _trade1[Gateway]; }
        internal static string Exchange { get => IsPoe2 ? _exchange2[Gateway] : _exchange1[Gateway]; }

        //poe1
        private static readonly string _poeWiki1 = "https://www.poewiki.net/wiki/";
        private static readonly string _poedb1 = "https://poedb.tw/us/Modifiers";
        private static readonly string _poedbHost1 = "https://poedb.tw/";
        private static readonly string _craftOfExile1 = "https://craftofexile.com/?game=poe1&eimport=$";
        private static readonly string _poeNinja1 = "https://poe.ninja/economy/";
        private static readonly string[] _trade1 = ["https://www.pathofexile.com/trade/search/", "https://poe.kakaogames.com/trade/search/", "https://fr.pathofexile.com/trade/search/", "https://es.pathofexile.com/trade/search/", "https://de.pathofexile.com/trade/search/", "https://br.pathofexile.com/trade/search/", "https://ru.pathofexile.com/trade/search/", "https://th.pathofexile.com/trade/search/", "https://pathofexile.tw/trade/search/", "https://poe.game.qq.com/trade/search/", "https://jp.pathofexile.com/trade/search/"];
        private static readonly string[] _exchange1 = ["https://www.pathofexile.com/trade/exchange/", "https://poe.kakaogames.com/trade/exchange/", "https://fr.pathofexile.com/trade/exchange/", "https://es.pathofexile.com/trade/exchange/", "https://de.pathofexile.com/trade/exchange/", "https://br.pathofexile.com/trade/exchange/", "https://ru.pathofexile.com/trade/exchange/", "https://th.pathofexile.com/trade/exchange/", "https://pathofexile.tw/trade/exchange/", "https://poe.game.qq.com/trade/exchange/", "https://jp.pathofexile.com/trade/exchange/"];
        private static readonly string _poeWikiRu = "https://pathofexile-ru.gamepedia.com/";

        //poe2
        private static readonly string _poeWiki2 = "https://www.poe2wiki.net/wiki/";
        private static readonly string _poedb2 = "https://poe2db.tw/us/Modifiers";
        private static readonly string _poedbHost2 = "https://poe2db.tw/";
        private static readonly string _craftOfExile2 = "https://craftofexile.com/?game=poe2&eimport=$";
        private static readonly string _poeNinja2 = "https://poe.ninja/poe2/economy/";
        private static readonly string[] _trade2 = ["https://www.pathofexile.com/trade2/search/", "https://poe.kakaogames.com/trade2/search/", "https://fr.pathofexile.com/trade2/search/", "https://es.pathofexile.com/trade2/search/", "https://de.pathofexile.com/trade2/search/", "https://br.pathofexile.com/trade2/search/", "https://ru.pathofexile.com/trade2/search/", "https://th.pathofexile.com/trade2/search/", "https://pathofexile.tw/trade2/search/", "https://poe.game.qq.com/trade2/search/", "https://jp.pathofexile.com/trade2/search/"];
        private static readonly string[] _exchange2 = ["https://www.pathofexile.com/trade2/exchange/", "https://poe.kakaogames.com/trade2/exchange/", "https://fr.pathofexile.com/trade2/exchange/", "https://es.pathofexile.com/trade2/exchange/", "https://de.pathofexile.com/trade2/exchange/", "https://br.pathofexile.com/trade2/exchange/", "https://ru.pathofexile.com/trade2/exchange/", "https://th.pathofexile.com/trade2/exchange/", "https://pathofexile.tw/trade2/exchange/", "https://poe.game.qq.com/trade2/exchange/", "https://jp.pathofexile.com/trade2/exchange/"];
    }
}
