namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class Api
    {
        internal const string League = "https://api.pathofexile.com/league?realm=pc?type=main";
        internal const string PoePrice = "https://www.poeprices.info/api?l=";

        internal static string NinjaLeague { get => IsPoe2 ? _ninjaLeague2 : _ninjaLeague1; }
        internal static string NinjaItem { get => IsPoe2 ? _ninjaItem2 : _ninjaItem1; }
        internal static string NinjaExchangeOverview { get => IsPoe2 ? _ninjaExchangeOverview2 : _ninjaExchangeOverview1; }
        internal static string NinjaExchangeDetails { get => IsPoe2 ? _ninjaExchangeDetails2 : _ninjaExchangeDetails1; }
        internal static string CurrencyExchange { get => IsPoe2 ? _currencyExchange2 : _currencyExchange1; }

        internal static string Trade { get => IsPoe2 ? _trade2[Gateway] : _trade1[Gateway]; }
        internal static string Fetch { get => IsPoe2 ? _fetch2[Gateway] : _fetch1[Gateway]; }
        internal static string Exchange { get => IsPoe2 ? _exchange2[Gateway] : _exchange1[Gateway]; }
        internal static string Whisper { get => IsPoe2 ? _whisper2[Gateway] : _whisper1[Gateway]; }

        internal static string GetUpdate(int idxLang) => IsPoe2 ? _update2[idxLang] : _update1[idxLang];

        // private members
        private static readonly string[] _trade1 = ["https://www.pathofexile.com/api/trade/search/", "https://poe.kakaogames.com/api/trade/search/", "https://fr.pathofexile.com/api/trade/search/", "https://es.pathofexile.com/api/trade/search/", "https://de.pathofexile.com/api/trade/search/", "https://br.pathofexile.com/api/trade/search/", "https://ru.pathofexile.com/api/trade/search/", "https://th.pathofexile.com/api/trade/search/", "https://pathofexile.tw/api/trade/search/", "https://poe.game.qq.com/api/trade/search/", "https://jp.pathofexile.com/api/trade/search/"];
        private static readonly string[] _update1 = ["https://www.pathofexile.com/api/trade/data/", "https://poe.kakaogames.com/api/trade/data/", "https://fr.pathofexile.com/api/trade/data/", "https://es.pathofexile.com/api/trade/data/", "https://de.pathofexile.com/api/trade/data/", "https://br.pathofexile.com/api/trade/data/", "https://ru.pathofexile.com/api/trade/data/", "https://th.pathofexile.com/api/trade/data/", "https://pathofexile.tw/api/trade/data/", "https://poe.game.qq.com/api/trade/data/", "https://jp.pathofexile.com/api/trade/data/"];
        private static readonly string[] _fetch1 = ["https://www.pathofexile.com/api/trade/fetch/", "https://poe.kakaogames.com/api/trade/fetch/", "https://fr.pathofexile.com/api/trade/fetch/", "https://es.pathofexile.com/api/trade/fetch/", "https://de.pathofexile.com/api/trade/fetch/", "https://br.pathofexile.com/api/trade/fetch/", "https://ru.pathofexile.com/api/trade/fetch/", "https://th.pathofexile.com/api/trade/fetch/", "https://pathofexile.tw/api/trade/fetch/", "https://poe.game.qq.com/api/trade/fetch/", "https://jp.pathofexile.com/api/trade/fetch/"];
        private static readonly string[] _exchange1 = ["https://www.pathofexile.com/api/trade/exchange/", "https://poe.kakaogames.com/api/trade/exchange/", "https://fr.pathofexile.com/api/trade/exchange/", "https://es.pathofexile.com/api/trade/exchange/", "https://de.pathofexile.com/api/trade/exchange/", "https://br.pathofexile.com/api/trade/exchange/", "https://ru.pathofexile.com/api/trade/exchange/", "https://th.pathofexile.com/api/trade/exchange/", "https://pathofexile.tw/api/trade/exchange/", "https://poe.game.qq.com/api/trade/exchange/", "https://jp.pathofexile.com/api/trade/exchange/"];
        private static readonly string[] _whisper1 = ["https://www.pathofexile.com/api/trade/whisper", "https://poe.kakaogames.com/api/trade/whisper", "https://fr.pathofexile.com/api/trade/whisper", "https://es.pathofexile.com/api/trade/whisper", "https://de.pathofexile.com/api/trade/whisper", "https://br.pathofexile.com/api/trade/whisper", "https://ru.pathofexile.com/api/trade/whisper", "https://th.pathofexile.com/api/trade/whisper", "https://pathofexile.tw/api/trade/whisper", "https://poe.game.qq.com/api/trade/whisper", "https://jp.pathofexile.com/api/trade/whisper"];
        private static readonly string _ninjaLeague1 = "https://poe.ninja/poe1/api/data/index-state";
        private static readonly string _ninjaItem1 = "https://poe.ninja/poe1/api/economy/stash/current/item/overview?league=";
        private static readonly string _ninjaExchangeOverview1 = "https://poe.ninja/poe1/api/economy/exchange/current/overview?league=";
        private static readonly string _ninjaExchangeDetails1 = "https://poe.ninja/poe1/api/economy/exchange/current/details?league=";
        private static readonly string _currencyExchange1 = "https://api.pathofexile.com/currency-exchange";
        //private static readonly string _currencyExchangeCdn1 = "https://web.poecdn.com/api/currency-exchange";

        private static readonly string[] _trade2 = ["https://www.pathofexile.com/api/trade2/search/", "https://poe.kakaogames.com/api/trade2/search/", "https://fr.pathofexile.com/api/trade2/search/", "https://es.pathofexile.com/api/trade2/search/", "https://de.pathofexile.com/api/trade2/search/", "https://br.pathofexile.com/api/trade2/search/", "https://ru.pathofexile.com/api/trade2/search/", "https://th.pathofexile.com/api/trade2/search/", "https://pathofexile.tw/api/trade2/search/", "https://poe.game.qq.com/api/trade2/search/", "https://jp.pathofexile.com/api/trade2/search/"];
        private static readonly string[] _update2 = ["https://www.pathofexile.com/api/trade2/data/", "https://poe.kakaogames.com/api/trade2/data/", "https://fr.pathofexile.com/api/trade2/data/", "https://es.pathofexile.com/api/trade2/data/", "https://de.pathofexile.com/api/trade2/data/", "https://br.pathofexile.com/api/trade2/data/", "https://ru.pathofexile.com/api/trade2/data/", "https://th.pathofexile.com/api/trade2/data/", "https://pathofexile.tw/api/trade2/data/", "https://poe.game.qq.com/api/trade2/data/", "https://jp.pathofexile.com/api/trade2/data/"];
        private static readonly string[] _fetch2 = ["https://www.pathofexile.com/api/trade2/fetch/", "https://poe.kakaogames.com/api/trade2/fetch/", "https://fr.pathofexile.com/api/trade2/fetch/", "https://es.pathofexile.com/api/trade2/fetch/", "https://de.pathofexile.com/api/trade2/fetch/", "https://br.pathofexile.com/api/trade2/fetch/", "https://ru.pathofexile.com/api/trade2/fetch/", "https://th.pathofexile.com/api/trade2/fetch/", "https://pathofexile.tw/api/trade2/fetch/", "https://poe.game.qq.com/api/trade2/fetch/", "https://jp.pathofexile.com/api/trade2/fetch/"];
        private static readonly string[] _exchange2 = ["https://www.pathofexile.com/api/trade2/exchange/", "https://poe.kakaogames.com/api/trade2/exchange/", "https://fr.pathofexile.com/api/trade2/exchange/", "https://es.pathofexile.com/api/trade2/exchange/", "https://de.pathofexile.com/api/trade2/exchange/", "https://br.pathofexile.com/api/trade2/exchange/", "https://ru.pathofexile.com/api/trade2/exchange/", "https://th.pathofexile.com/api/trade2/exchange/", "https://pathofexile.tw/api/trade2/exchange/", "https://poe.game.qq.com/api/trade2/exchange/", "https://jp.pathofexile.com/api/trade2/exchange/"];
        private static readonly string[] _whisper2 = ["https://www.pathofexile.com/api/trade2/whisper", "https://poe.kakaogames.com/api/trade2/whisper", "https://fr.pathofexile.com/api/trade2/whisper", "https://es.pathofexile.com/api/trade2/whisper", "https://de.pathofexile.com/api/trade2/whisper", "https://br.pathofexile.com/api/trade2/whisper", "https://ru.pathofexile.com/api/trade2/whisper", "https://th.pathofexile.com/api/trade2/whisper", "https://pathofexile.tw/api/trade2/whisper", "https://poe.game.qq.com/api/trade2/whisper", "https://jp.pathofexile.com/api/trade2/whisper"];
        private static readonly string _ninjaLeague2 = "https://poe.ninja/poe2/api/data/index-state";
        private static readonly string _ninjaItem2 = "https://poe.ninja/poe2/api/economy/stash/current/item/overview?league=";
        private static readonly string _ninjaExchangeOverview2 = "https://poe.ninja/poe2/api/economy/exchange/current/overview?league=";
        private static readonly string _ninjaExchangeDetails2 = "https://poe.ninja/poe2/api/economy/exchange/current/details?league=";
        private static readonly string _currencyExchange2 = "https://api.pathofexile.com/currency-exchange/poe2";
        //private static readonly string _currencyExchangeCdn2 = "https://web.poecdn.com/api/currency-exchange/poe2";
    }
}
