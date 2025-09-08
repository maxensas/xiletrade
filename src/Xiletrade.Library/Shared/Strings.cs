using System.Collections.Generic;

namespace Xiletrade.Library.Shared;

/// <summary> Static class containing ALL and ONLY global constants strings.</summary>
/// <remarks> Does not respect intentionally naming conventions for global constants.</remarks>
public static class Strings
{
    // public members
    public const string UrlGithubVersion = "https://raw.githubusercontent.com/maxensas/xiletrade/master/version_win.xml";
    public const string GitHubApiLatestRelease = "https://api.github.com/repos/maxensas/xiletrade/releases/latest";
    
    public static class WindowName
    {
        //public const string Main = "XileTrade";
        public const string Config = "Configuration";
        public const string Editor = "Editor";
        //public const string Start = "StartWindow";
        public const string Whisper = "WhisperListWindow";
        public const string Popup = "PopupWindow";
        public const string Regex = "RegexWindow";
        public static readonly List<string> XiletradeWindowList = new(){ Config, Editor, Whisper, Popup, Regex };
    }

    // private members
    private static readonly string _poeCaption1 = "Path of Exile";
    private static readonly string[] _tradeUrl1 = ["https://www.pathofexile.com/trade/search/", "https://poe.game.daum.net/trade/search/", "https://fr.pathofexile.com/trade/search/", "https://es.pathofexile.com/trade/search/", "https://de.pathofexile.com/trade/search/", "https://br.pathofexile.com/trade/search/", "https://ru.pathofexile.com/trade/search/", "https://th.pathofexile.com/trade/search/", "https://pathofexile.tw/trade/search/", "https://poe.game.qq.com/trade/search/", "https://jp.pathofexile.com/trade/search/"];
    private static readonly string[] _tradeApi1 = ["https://www.pathofexile.com/api/trade/search/", "https://poe.game.daum.net/api/trade/search/", "https://fr.pathofexile.com/api/trade/search/", "https://es.pathofexile.com/api/trade/search/", "https://de.pathofexile.com/api/trade/search/", "https://br.pathofexile.com/api/trade/search/", "https://ru.pathofexile.com/api/trade/search/", "https://th.pathofexile.com/api/trade/search/", "https://pathofexile.tw/api/trade/search/", "https://poe.game.qq.com/api/trade/search/", "https://jp.pathofexile.com/api/trade/search/"];
    private static readonly string[] _updateApi1 = ["https://www.pathofexile.com/api/trade/data/", "https://poe.game.daum.net/api/trade/data/", "https://fr.pathofexile.com/api/trade/data/", "https://es.pathofexile.com/api/trade/data/", "https://de.pathofexile.com/api/trade/data/", "https://br.pathofexile.com/api/trade/data/", "https://ru.pathofexile.com/api/trade/data/", "https://th.pathofexile.com/api/trade/data/", "https://pathofexile.tw/api/trade/data/", "https://poe.game.qq.com/api/trade/data/", "https://jp.pathofexile.com/api/trade/data/"];
    private static readonly string[] _fetchApi1 = ["https://www.pathofexile.com/api/trade/fetch/", "https://poe.game.daum.net/api/trade/fetch/", "https://fr.pathofexile.com/api/trade/fetch/", "https://es.pathofexile.com/api/trade/fetch/", "https://de.pathofexile.com/api/trade/fetch/", "https://br.pathofexile.com/api/trade/fetch/", "https://ru.pathofexile.com/api/trade/fetch/", "https://th.pathofexile.com/api/trade/fetch/", "https://pathofexile.tw/api/trade/fetch/", "https://poe.game.qq.com/api/trade/fetch/", "https://jp.pathofexile.com/api/trade/fetch/"];
    private static readonly string[] _exchangeUrl1 = ["https://www.pathofexile.com/trade/exchange/", "https://poe.game.daum.net/trade/exchange/", "https://fr.pathofexile.com/trade/exchange/", "https://es.pathofexile.com/trade/exchange/", "https://de.pathofexile.com/trade/exchange/", "https://br.pathofexile.com/trade/exchange/", "https://ru.pathofexile.com/trade/exchange/", "https://th.pathofexile.com/trade/exchange/", "https://pathofexile.tw/trade/exchange/", "https://poe.game.qq.com/trade/exchange/", "https://jp.pathofexile.com/trade/exchange/"];
    private static readonly string[] _exchangeApi1 = ["https://www.pathofexile.com/api/trade/exchange/", "https://poe.game.daum.net/api/trade/exchange/", "https://fr.pathofexile.com/api/trade/exchange/", "https://es.pathofexile.com/api/trade/exchange/", "https://de.pathofexile.com/api/trade/exchange/", "https://br.pathofexile.com/api/trade/exchange/", "https://ru.pathofexile.com/api/trade/exchange/", "https://th.pathofexile.com/api/trade/exchange/", "https://pathofexile.tw/api/trade/exchange/", "https://poe.game.qq.com/api/trade/exchange/", "https://jp.pathofexile.com/api/trade/exchange/"];
    private static readonly string _urlPoeWiki1 = "https://www.poewiki.net/wiki/";
    private static readonly string _urlPoedb1 = "https://poedb.tw/us/Modifiers";
    private static readonly string _urlPoedbHost1 = "https://poedb.tw/";
    private static readonly string _urlCraftOfExile1 = "https://craftofexile.com/?game=poe1&eimport=$";

    private static readonly string _poeCaption2 = "Path of Exile 2";
    private static readonly string[] _tradeUrl2 = ["https://www.pathofexile.com/trade2/search/", "https://poe.game.daum.net/trade2/search/", "https://fr.pathofexile.com/trade2/search/", "https://es.pathofexile.com/trade2/search/", "https://de.pathofexile.com/trade2/search/", "https://br.pathofexile.com/trade2/search/", "https://ru.pathofexile.com/trade2/search/", "https://th.pathofexile.com/trade2/search/", "https://pathofexile.tw/trade2/search/", "https://poe.game.qq.com/trade2/search/", "https://jp.pathofexile.com/trade2/search/"];
    private static readonly string[] _tradeApi2 = ["https://www.pathofexile.com/api/trade2/search/", "https://poe.game.daum.net/api/trade2/search/", "https://fr.pathofexile.com/api/trade2/search/", "https://es.pathofexile.com/api/trade2/search/", "https://de.pathofexile.com/api/trade2/search/", "https://br.pathofexile.com/api/trade2/search/", "https://ru.pathofexile.com/api/trade2/search/", "https://th.pathofexile.com/api/trade2/search/", "https://pathofexile.tw/api/trade2/search/", "https://poe.game.qq.com/api/trade2/search/", "https://jp.pathofexile.com/api/trade2/search/"];
    private static readonly string[] _updateApi2 = ["https://www.pathofexile.com/api/trade2/data/", "https://poe.game.daum.net/api/trade2/data/", "https://fr.pathofexile.com/api/trade2/data/", "https://es.pathofexile.com/api/trade2/data/", "https://de.pathofexile.com/api/trade2/data/", "https://br.pathofexile.com/api/trade2/data/", "https://ru.pathofexile.com/api/trade2/data/", "https://th.pathofexile.com/api/trade2/data/", "https://pathofexile.tw/api/trade2/data/", "https://poe.game.qq.com/api/trade2/data/", "https://jp.pathofexile.com/api/trade2/data/"];
    private static readonly string[] _fetchApi2 = ["https://www.pathofexile.com/api/trade2/fetch/", "https://poe.game.daum.net/api/trade2/fetch/", "https://fr.pathofexile.com/api/trade2/fetch/", "https://es.pathofexile.com/api/trade2/fetch/", "https://de.pathofexile.com/api/trade2/fetch/", "https://br.pathofexile.com/api/trade2/fetch/", "https://ru.pathofexile.com/api/trade2/fetch/", "https://th.pathofexile.com/api/trade2/fetch/", "https://pathofexile.tw/api/trade2/fetch/", "https://poe.game.qq.com/api/trade2/fetch/", "https://jp.pathofexile.com/api/trade2/fetch/"];
    private static readonly string[] _exchangeUrl2 = ["https://www.pathofexile.com/trade2/exchange/", "https://poe.game.daum.net/trade2/exchange/", "https://fr.pathofexile.com/trade2/exchange/", "https://es.pathofexile.com/trade2/exchange/", "https://de.pathofexile.com/trade2/exchange/", "https://br.pathofexile.com/trade2/exchange/", "https://ru.pathofexile.com/trade2/exchange/", "https://th.pathofexile.com/trade2/exchange/", "https://pathofexile.tw/trade2/exchange/", "https://poe.game.qq.com/trade2/exchange/", "https://jp.pathofexile.com/trade2/exchange/"];
    private static readonly string[] _exchangeApi2 = ["https://www.pathofexile.com/api/trade2/exchange/", "https://poe.game.daum.net/api/trade2/exchange/", "https://fr.pathofexile.com/api/trade2/exchange/", "https://es.pathofexile.com/api/trade2/exchange/", "https://de.pathofexile.com/api/trade2/exchange/", "https://br.pathofexile.com/api/trade2/exchange/", "https://ru.pathofexile.com/api/trade2/exchange/", "https://th.pathofexile.com/api/trade2/exchange/", "https://pathofexile.tw/api/trade2/exchange/", "https://poe.game.qq.com/api/trade2/exchange/", "https://jp.pathofexile.com/api/trade2/exchange/"];
    private static readonly string _urlPoeWiki2 = "https://www.poe2wiki.net/wiki/";
    private static readonly string _urlPoedb2 = "https://poe2db.tw/us/Modifiers";
    private static readonly string _urlPoedbHost2 = "https://poe2db.tw/";
    private static readonly string _urlCraftOfExile2 = "https://craftofexile.com/?game=poe2&eimport=$";

    private static readonly string _urlPoeWikiRu = "https://pathofexile-ru.gamepedia.com/";

    // initialized with data service
    private static bool IsPoe2 { get; set; }
    private static int Gateway { get; set; }

    // internal members
    /// <summary>Carriage Return + Line Feed</summary>
    internal const string CRLF = "\r\n";
    /// <summary>Line Feed</summary>
    internal const string LF = "\n";
    /// <summary>Delimiter used for POE item info descriptions.</summary>
    internal const string ItemInfoDelimiter = "--------";
    /// <summary> Delimiter used for POE item info descriptions + Carriage Return + Line Feed</summary>
    internal const string ItemInfoDelimiterCRLF = "--------\r\n";
    internal const string DetailListFormat1 = "{0,5} {1,-12} {2,3} {3,-23} {4} {6}";
    internal const string DetailListFormat2 = "{0,5} {1,-12} {2,3} {3,-8} {4}{5}{6,2}      {8}";
    /*internal const string DetailListFormat1 = "{0,5} {1,-12} {2,3} {3,-23} {4}{5}: {6}";
    internal const string DetailListFormat2 = "{0,5} {1,-12} {2,3} {3,-8} {4}{5}{6,2} {7,8}: {8}";*/
    internal const string PoeClass = "POEWindowClass";
    internal const string Info = " [Xiletrade POE Helper]";
    internal const string TrueOption = "_TRUE_";
    internal const string Prophecy = "Prophecy";
    internal const string Blight = "Blight";
    internal const string Ravaged = "Ravaged";
    internal const string Maps = "Maps";
    internal const string Delve = "Delve";
    internal const string Captured = "Captured";
    internal const string UnscalableValue = "Unscalable Value";
    internal const string ChaosOrb = "Chaos Orb";
    internal const string Online = "Online";
    internal const string Offline = "Offline";
    internal const string monster = "monster";
    internal const string scarab = "scarab";
    internal const string shard = "shard";
    internal const string splinter = "splinter";
    internal const string sep = "sep";
    internal const string tierPrefix = "tier-";
    internal const string any = "any";
    internal const string afk = "afk";
    internal const string contains = "contains";
    internal const string equals = "equals";
    internal const string NullClass = "NullClass";

    internal const string ApiPoePrice = "https://www.poeprices.info/api?l=";
    internal const string ApiNinjaItem = "https://poe.ninja/api/data/itemoverview?league=";
    internal const string ApiNinjaCur = "https://poe.ninja/api/data/currencyoverview?league=";
    internal const string ApiNinjaLeague = "https://poe.ninja/api/data/index-state";
    internal const string UrlPoelab = "https://www.poelab.com/";
    internal const string UrlPoeNinja = "https://poe.ninja/economy/";    
    internal const string UrlPoeRegex = "https://poe.re/";
    internal const string UrlPaypalDonate = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9TEA8EMSSB846";
    internal const string UrlGithubData = "https://raw.githubusercontent.com/maxensas/xiletrade/master/Xiletrade/Data/";

    internal static readonly string[] Culture = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW", "zh-CN", "ja-JP"];

    // parameters
    internal static string PoeCaption { get => IsPoe2 ? _poeCaption2 : _poeCaption1; }
    internal static string UrlPoeWiki { get => IsPoe2 ? _urlPoeWiki2 : _urlPoeWiki1; }
    internal static string UrlPoeWikiRu { get => IsPoe2 ? _urlPoeWiki2 : _urlPoeWikiRu; }
    internal static string UrlPoedb { get => IsPoe2 ? _urlPoedb2 : _urlPoedb1; }
    internal static string UrlPoedbHost { get => IsPoe2 ? _urlPoedbHost2 : _urlPoedbHost1; }
    internal static string UrlCraftOfExile { get => IsPoe2 ? _urlCraftOfExile2 : _urlCraftOfExile1; }
    internal static string TradeUrl { get => IsPoe2 ? _tradeUrl2[Gateway] : _tradeUrl1[Gateway]; }
    internal static string TradeApi { get => IsPoe2 ? _tradeApi2[Gateway] : _tradeApi1[Gateway]; }
    internal static string FetchApi { get => IsPoe2 ? _fetchApi2[Gateway] : _fetchApi1[Gateway]; }
    internal static string ExchangeUrl { get => IsPoe2 ? _exchangeUrl2[Gateway] : _exchangeUrl1[Gateway]; }
    internal static string ExchangeApi { get => IsPoe2 ? _exchangeApi2[Gateway] : _exchangeApi1[Gateway]; }
    
    // methods
    internal static void Initialize(bool isPoe2, int gateway)
    {
        IsPoe2 = isPoe2;
        Gateway = gateway;
    }

    internal static string GetUpdateApi(int idxLang) => IsPoe2 ? _updateApi2[idxLang] : _updateApi1[idxLang];
    
    /// <summary>
    /// Get Poe1 or Poe2 category.
    /// </summary>
    /// <param name="curClass"></param>
    /// <param name="curId"></param>
    /// <returns></returns>
    internal static string GetCategory(string curClass, string curId) => IsPoe2 ? 
        CurrencyTypePoe2.GetPoe2Category(curClass, curId) : CurrencyTypePoe1.GetPoe1Category(curClass, curId);

    // nested class
    internal static class Status
    {
        internal const string Available = "available"; // instant buy out & person trade
        internal const string Online = "online"; // person trade only
        internal const string Securable = "securable"; // instant buy out only
        internal const string Any = "any";
    }

    internal static class Emoji
    {
        internal const string VeryHappy = "emoji_vhappy";
        internal const string Happy = "emoji_happy";
        internal const string Neutral = "emoji_neutral";
        internal const string Crying = "emoji_crying";
        internal const string Angry = "emoji_angry";

        internal static string GetNinjaTag(double ratio)
        {
            return ratio >= 1.2 ? VeryHappy 
                : ratio >= 1 ? Happy 
                : ratio >= 0.90 ? Neutral 
                : ratio >= 0.80 ? Crying 
                : Angry;
        }
    }

    internal static class File
    {
        internal const string Config = "Config.json";
        internal const string DefaultConfig = "DefaultConfig.json";
        internal const string Divination = "Divination.json";
        internal const string ParsingRules = "ParsingRules.json";
        internal const string Monsters = "Monsters.json";
        internal const string Gems = "Gems.json";
        internal const string DustLevel = "DustLevel.json";

        internal const string _currency1 = "Currency.json";
        internal const string _filters1 = "Filters.json";
        internal const string _leagues1 = "Leagues.json";
        internal const string _bases1 = "Bases.json";
        internal const string _mods1 = "Mods.json";
        internal const string _words1 = "Words.json";

        internal const string _currency2 = "CurrencyTwo.json";
        internal const string _filters2 = "FiltersTwo.json";
        internal const string _leagues2 = "LeaguesTwo.json";
        internal const string _bases2 = "BasesTwo.json";
        internal const string _mods2 = "ModsTwo.json";
        internal const string _words2 = "WordsTwo.json";

        internal static string Currency { get => IsPoe2 ? _currency2 : _currency1; }
        internal static string Filters { get => IsPoe2 ? _filters2 : _filters1; }
        internal static string Leagues { get => IsPoe2 ? _leagues2 : _leagues1; }
        internal static string Bases { get => IsPoe2 ? _bases2 : _bases1; }
        internal static string Mods { get => IsPoe2 ? _mods2 : _mods1; }
        internal static string Words { get => IsPoe2 ? _words2 : _words1; }
    }

    internal static class Feature
    {
        internal const string hideout = "hideout";
        internal const string oos = "oos";
        internal const string exitchar = "exitchar";
        internal const string invlast = "invlast";
        internal const string replylast = "replylast";
        internal const string tradelast = "tradelast";
        internal const string whoislast = "whoislast";
        internal const string tradechan = "tradechan";
        internal const string globalchan = "globalchan";
        internal const string invite = "invite";
        internal const string kick = "kick";
        internal const string leave = "leave";
        internal const string afk = "afk";
        internal const string autoreply = "autoreply";
        internal const string dnd = "dnd";
        internal const string chat1 = "chat1";
        internal const string chat2 = "chat2";
        internal const string chat3 = "chat3";
        internal static readonly List<string> Unregisterable = new() { hideout, oos, exitchar, invlast, replylast, tradelast,
            whoislast, tradechan, globalchan, invite, kick, leave, afk, autoreply, dnd, chat1, chat2, chat3, close};
        internal const string whispertrade = "whispertrade";
        internal const string bulk = "bulk";
        internal const string close = "close";
        internal const string config = "config";
        internal const string run = "run";
        internal const string syndicate = "syndicate";
        internal const string incursion = "incursion";
        internal const string tcp = "tcp";
        internal const string wiki = "wiki";
        internal const string ninja = "ninja";
        internal const string lab = "lab";
        internal const string poedb = "poedb";
        internal const string link1 = "link1";
        internal const string link2 = "link2";
        internal const string chatkey = "chatkey";
        internal const string regex = "regex";
        internal const string coe = "coe";
    }

    internal static class BulkStrings
    {
        internal const string Delimiter = "------------------------------------------------";
        internal static readonly string[] DivinationCardTier = ["T1", "T2", "T3", "T4"];
        internal static readonly string[] MapTierPoe1 = ["T1","T2","T3","T4","T5",
            "T6","T7","T8","T9","T10","T11","T12","T13","T14","T15","T16","T17"];
        internal static readonly string[] MapTierPoe2 = ["T1","T2","T3","T4","T5",
            "T6","T7","T8","T9","T10","T11","T12","T13","T14","T15","T16"];
    }

    internal static class Chat
    {
        internal const string hideout = "/hideout";
        internal const string exit = "/exit";
        internal const string leave = "/leave";
        internal const string invite = "/invite";
        internal const string tradewith = "/tradewith";
        internal const string whois = "/whois";
        internal const string trade = "/trade";
        internal const string global = "/global";
        internal const string kick = "/kick";
        internal const string afk = "/afk";
        internal const string autoreply = "/autoreply";
        internal const string dnd = "/dnd";
    }

    internal static class Label
    {
        internal const string Pseudo = "Pseudo";
        internal const string Explicit = "Explicit";
        internal const string Implicit = "Implicit";
        internal const string Fractured = "Fractured";
        internal const string Enchant = "Enchant";
        internal const string Crafted = "Crafted";
        internal const string Veiled = "Veiled";
        internal const string Monster = "Monster";
        internal const string Delve = "Delve";
        internal const string Ultimatum = "Ultimatum";
        internal const string Necropolis = "Necropolis";
    }

    internal static class ModTag
    {
        internal const string Attack = "Attack";
        internal const string Physical = "Physical";
        internal const string Caster = "Caster";
        internal const string Speed = "Speed";
        internal const string Critical = "Critical";
        internal const string Fire = "Fire";
        internal const string Cold = "Cold";
        internal const string Lightning = "Lightning";
        internal const string Chaos = "Chaos";
        internal const string Life = "Life";
        internal const string Defences = "Defences";
    }

    internal static class ModKind
    {
        internal const string DangerousMod = "DangerousMod";
        internal const string RareMod = "RareMod";
    }

    internal static class TierKind
    {
        internal const string EnchantAndCraft = "R";
        internal const string Implicit = "I";
        internal const string Prefix = "P";
        internal const string Suffix = "S";
        internal const string Unique = "U";
    }

    internal static class Color
    {
        internal const string Red = "Red";
        internal const string YellowGreen = "YellowGreen";
        internal const string LimeGreen = "LimeGreen";
        internal const string Yellow = "Yellow";
        internal const string DeepSkyBlue = "DeepSkyBlue";
        internal const string DarkRed = "DarkRed";
        internal const string LightGray = "LightGray";
        internal const string Azure = "Azure";
        internal const string Gray = "Gray";
        internal const string Peru = "Peru";
        internal const string Gold = "Gold";
        internal const string Green = "Green";
        internal const string Teal = "Teal";
        internal const string Moccasin = "Moccasin";
    }

    internal static class ItemLabel
    {
        internal const string Implicit = "(implicit)";
        internal const string Scourge = "(scourge)";
        internal const string Fractured = "(fractured)";
        internal const string Enchant = "(enchant)";
        internal const string Crafted = "(crafted)";
        internal const string Augmented = "(augmented)";
        internal const string Rune = "(rune)";
        internal const string Desecrated = "(desecrated)";
    }

    internal static class Gem
    {
        internal const string Anomalous = "Anomalous";
        internal const string Divergent = "Divergent";
        internal const string Phantasmal = "Phantasmal";
    }

    internal static class Reward
    {
        internal const string DoubleCurrency = "DoubleCurrency";
        internal const string DoubleDivCards = "DoubleDivCards";
        internal const string MirrorRare = "MirrorRare";
        internal const string ExchangeUnique = "ExchangeUnique";
        internal const string FoilUnique = "FoilUnique"; // string can be modified
    }

    internal static class Words
    {
        internal const string ToMaxLife = "to maximum life";
        internal const string ToMaxEs = "to maximum energy shield";
        internal const string ToAllResist = "to all Elemental Resistances";
        internal const string ToStrength = "to strength";
        internal const string Resistance = "resistance";
        internal const string Fire = "fire";
        internal const string Cold = "cold";
        internal const string Lightning = "lightning";
        internal const string Chaos = "chaos";
        internal const string IndexableSupport = "indexable_support";
        internal const string Sanctum = "sanctum";
        internal const string Logbook = "logbook";
        internal const string Explicit = "explicit";
    }

    internal static class Unique
    {
        internal const string StringOfServitude = "String of Servitude";
        internal const string ForbiddenShako = "Forbidden Shako";
        internal const string LioneyesVision = "Lioneye's Vision";
        internal const string Bitterdream = "Bitterdream";
        internal const string TheHungryLoop = "The Hungry Loop";
        internal const string GorgonsGaze = "Gorgon's Gaze";
        internal const string EshsMirror = "Esh's Mirror";
        internal const string BonesOfUllr = "Bones of Ullr";
        internal const string CinderswallowUrn = "Cinderswallow Urn";
        internal const string DivinationDistillate = "Divination Distillate";
        internal const string TheBlueDream = "The Blue Dream";
        internal const string TheDancingDervish = "The Dancing Dervish";
        internal const string ReplicaTrypanon = "Replica Trypanon";
        internal const string UulNetolsKiss = "Uul-Netol's Kiss";
    }

    internal static class UniqueTwo
    {
        internal const string TheUnbornLich = "The Unborn Lich";
        internal const string GripofKulemak = "Grip of Kulemak";
        internal const string NazirsJudgement = "Nazir's Judgement";
        internal const string HrimnorsHymn = "Hrimnor's Hymn";
        internal const string TheHammerofFaith = "The Hammer of Faith";
    }

    internal static class Resource
    {
        internal const string Enchant = "General011_Enchant";
        internal const string Crafted = "General012_Crafted";
        internal const string Implicit = "General013_Implicit";
        internal const string Pseudo = "General014_Pseudo";
        internal const string Explicit = "General015_Explicit";
        internal const string Fractured = "General016_Fractured";
        internal const string CorruptImp = "General017_CorruptImp";
        internal const string Monster = "General018_Monster";
        internal const string Scourge = "General099_Scourge";
        internal const string Desecrated = "General158_Desecrated";
    }

    internal static class CurrencyTypePoe1
    {
        internal const string Cards = "Cards";
        internal const string Currency = "Currency";
        internal const string Delve = "Delve"; // Fossils and Resonators
        internal const string Fragments = "Fragments";
        internal const string Incubators = "Incubators";
        internal const string Scarabs = "Scarabs";
        internal const string Essences = "Essences";
        internal const string Oils = "Oils";
        internal const string Catalysts = "Catalysts";
        internal const string DeliriumOrbs = "DeliriumOrbs";
        internal const string Expedition = "Expedition";
        internal const string Prophecies = "Prophecies";
        internal const string Splinters = "Splinters";
        internal const string MapsUnique = "MapsUnique";
        internal const string MapsBlighted = "MapsBlighted";

        internal const string MapsBlightRavaged = "MapsUberBlighted";
        internal const string MapsTier = "MapsTier";

        internal const string Maps = "Maps";
        internal const string MapsSpecial = "MapsSpecial"; // boss
        internal const string Beasts = "Beasts";
        internal const string Heist = "Heist";

        internal const string TaintedCurrency = "TaintedCurrency";
        internal const string EldritchCurrency = "EldritchCurrency";
        internal const string ScoutingReport = "ScoutingReport";
        internal const string Sentinel = "Sentinel";
        internal const string Exotic = "ExoticCurrency";
        internal const string Ancestor = "Ancestor";
        internal const string Sanctum = "Sanctum";
        internal const string Crucible = "Crucible";

        internal const string AllflameEmbers = "AllflameEmbers";
        internal const string Runegrafts = "Runegrafts";

        internal static string GetPoe1Category(string curClass, string curId)
        {
            return curClass is Currency ?
                    dicMainCur.TryGetValue(curId, out string curVal2) ? Resources.Resources.Main044_MainCur :
                    dicExoticCur.TryGetValue(curId, out string curVal4) ? Resources.Resources.Main207_ExoticCurrency : Resources.Resources.Main045_OtherCur :
                    curClass is Fragments ? dicStones.TryGetValue(curId, out string curVal3) ? Resources.Resources.Main047_Stones
                    : curId.Contain(scarab) ? Resources.Resources.Main052_Scarabs : Resources.Resources.Main046_MapFrag :
                    curClass is ScoutingReport ? Resources.Resources.Main198_ScoutingReports :
                    curClass is Expedition ? Resources.Resources.Main186_Expedition :
                    curClass is DeliriumOrbs ? Resources.Resources.Main048_Delirium :
                    curClass is Catalysts ? Resources.Resources.Main049_Catalysts :
                    curClass is Oils ? Resources.Resources.Main050_Oils :
                    curClass is Incubators ? Resources.Resources.Main051_Incubators :
                    curClass is Delve ? Resources.Resources.Main053_Fossils :
                    curClass is Essences ? Resources.Resources.Main054_Essences :
                    curClass is Ancestor ? Resources.Resources.Main211_AncestorCurrency :
                    curClass is Sanctum ? Resources.Resources.Main212_Sanctum :
                    curClass is Sentinel ? Resources.Resources.Main200_SentinelCurrency :
                    curClass is Cards ? Resources.Resources.Main055_Divination :
                    curClass is MapsUnique ? Resources.Resources.Main179_UniqueMaps :
                    curClass is Maps ? Resources.Resources.Main056_Maps :
                    curClass is MapsBlighted ? Resources.Resources.Main217_BlightedMaps :
                    curClass is MapsSpecial ? Resources.Resources.Main216_BossMaps :
                    curClass is Beasts ? Resources.Resources.Main219_Beasts :
                    curClass is Heist ? Resources.Resources.Main218_Heist :
                    curClass is Runegrafts ? Resources.Resources.General132_Rune :
                    curClass is AllflameEmbers ? Resources.Resources.ItemClass_allflame :
                    string.Empty;
        }
    }

    internal static class CurrencyTypePoe2
    {
        internal const string Currency = "Currency";
        internal const string Fragments = "Fragments";
        internal const string Runes = "Runes";
        internal const string Essences = "Essences";
        internal const string Relics = "Relics";
        internal const string Ultimatum = "Ultimatum";
        internal const string BreachCatalyst = "BreachCatalyst";
        internal const string Expedition = "Expedition";
        internal const string Ritual = "Ritual";
        internal const string Delirium = "Delirium";
        internal const string Waystones = "Waystones";
        internal const string VaultKeys = "VaultKeys";
        internal const string Talismans = "Talismans";
        internal const string Abyss = "Abyss";
        internal const string UncutGems = "UncutGems";
        internal const string LineageSupportGems = "LineageSupportGems";

        internal static string GetPoe2Category(string curClass, string curId)
        {
            return curClass is Currency ?
                    dicMainCur.TryGetValue(curId, out string curVal20) ? Resources.Resources.Main044_MainCur : Resources.Resources.Main045_OtherCur :
                    curClass is Fragments ? Resources.Resources.Main046_MapFrag :
                    curClass is Runes ? Resources.Resources.General132_Rune :
                    curClass is Essences ? Resources.Resources.Main054_Essences :
                    curClass is Relics ? Resources.Resources.ItemClass_sanctumRelic :
                    curClass is Ultimatum ? Resources.Resources.General069_Ultimatum :
                    curClass is BreachCatalyst ? Resources.Resources.Main049_Catalysts :
                    curClass is Expedition ? Resources.Resources.Main186_Expedition :
                    curClass is Ritual ? Resources.Resources.ItemClass_omen :
                    curClass is Delirium ? Resources.Resources.Main236_Delirium :
                    curClass is Waystones ? Resources.Resources.ItemClass_maps :
                    curClass is Talismans ? Resources.Resources.Main229_Talismans :
                    curClass is VaultKeys ? Resources.Resources.Main230_VaultKeys :
                    curClass is Abyss ? Resources.Resources.Main235_AbyssalBones :
                    curClass is UncutGems ? Resources.Resources.Main237_UncutGems :
                    curClass is LineageSupportGems ? Resources.Resources.Main238_LineageGems :
                    string.Empty;
        }
    }

    internal static class Inherit
    {
        internal const string Jewels = "Jewels";
        internal const string Armours = "Armours";
        internal const string Weapons = "Weapons";
        internal const string Quivers = "Quivers";
        internal const string Amulets = "Amulets";
        internal const string Rings = "Rings";
        internal const string Belts = "Belts";
        internal const string UniqueFragments = "UniqueFragments";
        internal const string Labyrinth = "Labyrinth";
        internal const string Gems = "Gems";
        internal const string Currency = "Currency";
        internal const string Delve = "Delve";
        internal const string Maps = "Maps";
        internal const string Area = "Area";
        internal const string Sentinel = "Sentinel";
        internal const string Sanctum = "Sanctum";
        internal const string Charms = "AnimalCharms";
        internal const string Tinctures = "Tinctures";
        internal const string NecropolisPack = "NecropolisPack";
        internal const string Expedition = "Expedition";
        internal const string Waystones = "Waystones";
    }

    internal static class Net
    {
        internal static readonly string UserAgent = "OAuth Xiletrade/" + Common.GetFileVersion() + " (contact: xiletrade@gmail.com)";
        internal const string TencetUrl = "poe.game.qq.com";
        internal const string XrateLimitPolicy = "X-Rate-Limit-Policy";
        internal const string TradeSearchRequestLimit = "trade-search-request-limit";
        internal const string TradeFetchRequestLimit = "trade-fetch-request-limit";
        internal const string TradeExchangeRequestLimit = "trade-exchange-request-limit";
        internal const string XrateLimit = "X-Rate-Limit-";
        internal const string State = "-State";
        internal const string RetryAfter = "Retry-After";
    }

    internal static class Stat
    {
        internal static class Influence
        {
            internal const string Shaper = "pseudo_has_shaper_influence"; // Has Shaper Influence
            internal const string Elder = "pseudo_has_elder_influence"; // Has Elder Influence
            internal const string Crusader = "pseudo_has_crusader_influence"; // Has Crusader Influence
            internal const string Redeemer = "pseudo_has_redeemer_influence"; // Has Redeemer Influence
            internal const string Hunter = "pseudo_has_hunter_influence"; // Has Hunter Influence
            internal const string Warlord = "pseudo_has_warlord_influence"; // Has Warlord Influence
            internal const string Count = "pseudo_has_influence_count"; // Has # Influences
        }

        internal static class Temple
        {
            internal const string Room01 = "pseudo_temple_apex"; // Apex of Atzoatl
            internal const string Room02 = "pseudo_temple_breeding_room_3"; // Hall of War
            internal const string Room03 = "pseudo_temple_workshop_3"; // Factory
            internal const string Room04 = "pseudo_temple_explosives_room_3"; // Shrine of Unmaking
            internal const string Room05 = "pseudo_temple_breach_room_3"; // House of the Others
            internal const string Room06 = "pseudo_temple_currency_vault_3"; // Wealth of the Vaal
            internal const string Room07 = "pseudo_temple_weapon_room_3"; // Hall of Champions
            internal const string Room08 = "pseudo_temple_armour_room_3"; // Chamber of Iron
            internal const string Room09 = "pseudo_temple_trinket_room_3"; // Glittering Halls
            internal const string Room10 = "pseudo_temple_cartography_room_3"; // Atlas of Worlds
            internal const string Room11 = "pseudo_temple_gem_room_3"; // Doryani's Institute
            internal const string Room12 = "pseudo_temple_torment_3"; // Sadist's Den
            internal const string Room13 = "pseudo_temple_strongbox_3"; // Court of Sealed Death
            internal const string Room14 = "pseudo_temple_legion_3"; // Hall of Legends
            internal const string Room15 = "pseudo_temple_sacrifice_room_3"; // Apex of Ascension
            internal const string Room16 = "pseudo_temple_chests_3"; // Museum of Artefacts
            internal const string Room17 = "pseudo_temple_corruption_room_3"; // Locus of Corruption
            internal const string Room18 = "pseudo_temple_empowering_room_3"; // Temple Nexus
            internal const string Room19 = "pseudo_temple_storm_room_3"; // Storm of Corruption
            internal const string Room20 = "pseudo_temple_poison_room_3"; // Toxic Grove
            internal const string Room21 = "pseudo_temple_trap_room_3"; // Defense Research Lab
            internal const string Room22 = "pseudo_temple_healing_room_3"; // Sanctum of Immortality
            internal const string Room23 = "pseudo_temple_boss_fire_3"; // Crucible of Flame
            internal const string Room24 = "pseudo_temple_boss_lightning_3"; // Conduit of Lightning
            internal const string Room25 = "pseudo_temple_boss_minions_3"; // Hybridisation Chamber
            internal const string Room26 = "pseudo_temple_queens_chambers_3"; // Throne of Atziri

            internal static readonly string[] RoomList = [ Room01, Room02, Room03, Room04, Room05, Room06, Room07, Room08, Room09, Room10,
            Room11, Room12, Room13, Room14, Room15, Room16, Room17, Room18, Room19, Room20, Room21, Room22, Room23, Room24, Room25, Room26];
        }

        internal static class Lake
        {
            internal const string Tablet01 = "pseudo.lake_50846"; // Reflection of Paradise (Difficulty #)
            internal const string Tablet02 = "pseudo.lake_36591"; // Reflection of Kalandra (Difficulty #)
            internal const string Tablet03 = "pseudo.lake_60034"; // Reflection of the Sun (Difficulty #)
            internal const string Tablet04 = "pseudo.lake_40794"; // Reflection of Angling (Difficulty #)
        }

        internal static class Pseudo
        {
            internal const string TotalResistance = "pseudo.pseudo_total_resistance"; // +#% total Resistance
            internal const string TotalLife = "pseudo.pseudo_total_life"; // +# total maximum Life
            internal const string TotalEs = "pseudo.pseudo_total_energy_shield"; // # to maximum Energy Shield
            internal const string MoreScarab = "pseudo.pseudo_map_more_scarab_drops"; // More Scarabs: #%
            internal const string MoreCurrency = "pseudo.pseudo_map_more_currency_drops"; // More Currency: #%
            internal const string MoreDivCard = "pseudo.pseudo_map_more_card_drops"; // More Divination Cards: #%

            internal const string EmmptyPrefix = "pseudo.pseudo_number_of_empty_prefix_mods"; // # Empty Prefix Modifiers
            internal const string EmptySuffix = "pseudo.pseudo_number_of_empty_suffix_mods"; // # Empty Suffix Modifiers
        }

        internal static class Aura
        {
            //auras
            internal const string Hatred = "stat_1920370417";
            internal const string Grace = "stat_1803598623";
            internal const string Determination = "stat_2721871046";
            internal const string Pride = "stat_3484910620";
            internal const string Anger = "stat_2963485753";
            internal const string Zealotry = "stat_4216444167";
            internal const string Malevolence = "stat_3266567165";
            internal const string Wrath = "stat_1761642973";
            internal const string Discipline = "stat_1692887998";
            internal const string HeraldIce = "stat_3059700363";
            internal const string HeraldAsh = "stat_3819451758";
            internal const string HeraldPurity = "stat_1542765265";
            internal const string HeraldAgony = "stat_1284151528";
            internal const string HeraldThunder = "stat_3959101898";
            internal const string ArcticArmour = "stat_2605040931";
            internal const string PurityFire = "stat_1135152940";
            internal const string PurityLightning = "stat_1450978702";
            internal const string PurityIce = "stat_2665518524";
            internal const string DelveCorrupted = "delve_corrupted_implicit";

            internal static readonly List<string> lSkipMods = new()
            {
                Hatred, Grace, Determination, Pride, Anger, Zealotry, Malevolence, Wrath, Discipline, HeraldIce, HeraldAsh,
                HeraldPurity, HeraldAgony, HeraldThunder, ArcticArmour, PurityFire, PurityLightning, PurityIce, DelveCorrupted
            };
        }

        internal static class Generic
        {
            internal const string UseRemaining = "stat_1479533453"; // # use remaining : enchant.stat_290368246 / explicit.stat_1479533453
            internal const string PassiveSkill = "stat_3086156145"; // Adds # Passive Skills
            internal const string PassiveJewel = "stat_4079888060"; // # Added Passive Skills are Jewel Sockets
            internal const string GrantNothing = "stat_1085446536"; // Adds # Small Passive Skills which grant nothing
            internal const string Crafted = "stat_1859333175"; // Can have up to 3 Crafted Modifiers
            internal const string IncPhys = "stat_1509134228"; // #% increased Physical Damage

            internal const string LogbookBoss = "stat_3159649981"; // Area contains an Expedition Boss (#)
            internal const string LogbookArea = "stat_1160596338"; // Area contains an additional Underground Area
            internal const string LogbookTwice = "stat_3239978999"; // Excavated Chests have a #% chance to contain twice as many Items

            //non-local
            internal const string BlockStaff = "stat_1778298516"; // #% Chance to Block Attack Damage while wielding a Staff

            internal const string AddArmor = "stat_809229260"; // # to Armour
            internal const string AddEs = "stat_3489782002"; // # to maximum Energy Shield
            internal const string AddEva = "stat_2144192055"; // # to Evasion Rating

            //local
            internal const string Block = "stat_4253454700"; // #% Chance to Block (Shields)
            internal const string BlockStaffWeapon = "stat_1001829678"; // #% Chance to Block Attack Damage while wielding a Staff (Staves)

            internal const string AddArmorFlat = "stat_3484657501"; // # to Armour (Local)
            internal const string AddEsFlat = "stat_4052037485"; // # to maximum Energy Shield (Local)
            internal const string AddEvaFlat = "stat_53045048"; // # to Evasion Rating (Local)

            internal const string IncEs = "stat_4015621042"; // #% increased Energy Shield (Local)
            internal const string IncEva = "stat_124859000"; // #% increased Evasion Rating (Local)
            internal const string IncArmour = "stat_1062208444"; // #% increased Armour (Local)
            internal const string IncAe = "stat_2451402625"; // #% increased Armour and Evasion (Local)
            internal const string IncAes = "stat_3321629045"; // #% increased Armour and Energy Shield (Local)
            internal const string IncEes = "stat_1999113824"; // #% increased Evasion and Energy Shield (Local)
            internal const string IncArEes = "stat_3523867985"; // #% increased Armour, Evasion and Energy Shield (Local)

            internal const string AddAccuracyLocal = "stat_691932474"; // # to Accuracy Rating (Local)
            internal const string LifeLeech = "stat_55876295"; // #% of Physical Attack Damage Leeched as Life (Local)
            internal const string ManaLeech = "stat_669069897"; // #% of Physical Attack Damage Leeched as Mana (Local)
            internal const string PoisonHit = "stat_3885634897"; // #% chance to Poison on Hit (Local)
            internal const string AttackSpeed = "stat_210067635"; // #% increased Attack Speed (Local)
            internal const string IncPhysFlat = "stat_1940865751"; // Adds # to # Physical Damage (Local)
            internal const string IncLightFlat = "stat_3336890334"; // Adds # to # Lightning Damage (Local)
            internal const string IncColdFlat = "stat_1037193709"; // Adds # to # Cold Damage (Local)
            internal const string IncFireFlat = "stat_709508406"; // Adds # to # Fire Damage (Local)
            internal const string IncChaosFlat = "stat_2223678961"; // Adds # to # Chaos Damage (Local)
        }

        internal static class Option
        {
            internal const string Allocate = "enchant.stat_2954116742"; // Allocates #
            internal const string AllocateAdd = "enchant.stat_3459808765"; // Allocates # (Additional)
            internal const string AllocateFlesh = "explicit.stat_2460506030"; // Allocates # if you have matching modifier on Forbidden Flame
            internal const string AllocateFlame = "explicit.stat_1190333629"; // Allocates # if you have matching modifier on Forbidden Flesh
            internal const string Bestial = "explicit.stat_2878779644"; // Grants Level 20 Summon Bestial # Skill
            internal const string RingPassive = "explicit.stat_3642528642"; // Only affects Passives in # Ring
            internal const string SmallPassive = "enchant.stat_3948993189"; // Added Small Passive Skills grant: #
            internal const string PassivesInRadius = "explicit.stat_2422708892"; // Passives in Radius of # can be Allocated\nwithout being connected to your tree
            internal const string CompassHarvest = "enchant.stat_832377952"; // Harvests in Areas contain at least one Crop of # Plants
            internal const string CompassMaster = "enchant.stat_3187151138"; // Area contains # (Master)
            internal const string CompassStrongbox = "enchant.stat_3522828354"; // Strongboxes in Area are at least #
            internal const string CompassBreach = "enchant.stat_1542416476"; // Breaches in Areas belong to #
            internal const string MapOccupConq = "implicit.stat_2563183002"; // Map contains #'s Citadel
            internal const string MapOccupElder = "implicit.stat_3624393862"; // Map is occupied by #
            internal const string AreaInflu = "implicit.stat_1792283443"; // Area is influenced by #
        }

        //implicits
        internal const string ActionSpeed = "implicit.stat_2878959938"; // #% reduced Action Speed
        internal const string AreaInfluOrigin = "implicit.stat_2696470877"; // Area is Influenced by the Originator's Memories
        
        //explicits
        internal const string StunOnYou = "explicit.stat_1067429236"; // #% increased Stun Duration on you

        internal const string PoisonMoreDmg1 = "explicit.stat_2523146878"; // #% chance for Poisons inflicted with this Weapon to deal 100% more Damage
        internal const string PoisonMoreDmg2 = "explicit.stat_768124628"; // #% chance for Poisons inflicted with this Weapon to deal 300% more Damage

        internal const string HitBlind1 = "explicit.stat_3503466234"; // #% increased Damage with Hits and Ailments against Blinded Enemies
        internal const string HitBlind2 = "explicit.stat_3565956680"; // #% increased Damage with Hits and Ailments against Blinded Enemies

        internal const string IncExpGain = "explicit.stat_3666934677"; // #% increased Experience gain
        internal const string IncExpGainMap = "explicit.stat_57434274"; // #% increased Experience gain (Maps)

        internal const string TriggerAssassinOld = "explicit.stat_3382957283"; // Before HEIST : Trigger Level # Assassin's Mark when you Hit a Rare or Unique Enemy
        internal const string TriggerAssassinNew = "explicit.stat_3924520095"; // Heist update mod

        internal const string ImmunityIgnite1 = "explicit.stat_2361218755"; // Grants Immunity to Ignite for # seconds if used while Ignited\nRemoves all Burning when used
        internal const string ImmunityIgnite2 = "explicit.stat_2695527599"; // Grants Immunity to Ignite for 4 seconds if used while Ignited\nRemoves all Burning when used

        internal const string IncManaReserveEffOld = "explicit.stat_1269219558"; // #% increased Mana Reservation Efficiency of Skills
        internal const string IncManaReserveEffNew = "explicit.stat_4237190083"; // #% increased Mana Reservation Efficiency of Skills

        internal const string BlockAttack1 = "explicit.stat_2530372417"; // #% Chance to Block Attack Damage
        internal const string BlockAttack2 = "explicit.stat_1702195217"; // #% Chance to Block Attack Damage
        internal const string BlockSpell1 = "explicit.stat_561307714"; // #% Chance to Block Spell Damage
        internal const string BlockSpell2 = "explicit.stat_19803471"; // #% Chance to Block Spell Damage

        internal const string IncCritAgainst1 = "explicit.stat_165218607"; // Hits have #% increased Critical Strike Chance against you
        internal const string IncCritAgainst2 = "explicit.stat_4270096386"; // Hits have #% increased Critical Strike Chance against you

        internal const string FlaskIncRarity1 = "explicit.stat_1740200922"; // #% increased Rarity of Items found during Effect
        internal const string FlaskIncRarity2 = "explicit.stat_3251705960"; // #% increased Rarity of Items found during Effect

        internal const string MonsterLifeOld = "explicit.stat_95249895";
        internal const string MonsterLifeNew = "explicit.stat_2710898947";

        internal const string TimelessJewel = "explicit.pseudo_timeless_jewel";

        internal const string Conflux = "explicit.stat_1190121450";

        internal const string Rampage = "explicit.stat_2397408229"; // Rampage

        internal const string SupressNew = "explicit.stat_3680664274"; // #% chance to Suppress Spell Damage
        internal const string SupressOld = "explicit.stat_492027537"; // #% chance to Suppress Spell Damage

        internal const string BleedingAvoid = "explicit.stat_1618589784"; // #% chance to Avoid Bleeding
        internal const string BleedingCannot = "explicit.stat_1901158930"; // Bleeding cannot be inflicted on you

        internal const string SocketsUnmodifiable = "explicit.stat_3192592092"; // Sockets cannot be modified

        internal const string TheBlueDream = "explicit.stat_926444104";
        internal const string TheBlueNightmare = "explicit.stat_1224928411";

        internal const string FireTakenOld = "explicit.stat_1029319062"; // #% of Fire Damage from Hits taken as Physical Damage
        internal const string FireTakenNew = "explicit.stat_3205239847"; // #% of Fire Damage from Hits taken as Physical Damage

        internal const string CritFlaskChargeOld = "explicit.stat_2858921304"; // #% chance to gain a Flask Charge when you deal a Critical Strike
        internal const string CritFlaskChargeNew = "explicit.stat_3738001379"; // #% chance to gain a Flask Charge when you deal a Critical Strike

        internal const string SocketedPierce1 = "explicit.stat_254728692"; // Socketed Gems are Supported by Level # Pierce
        internal const string SocketedPierce2 = "explicit.indexable_support_33"; // Socketed Gems are Supported by Level # Pierce
        //internal const string SocketedPierce3 = "explicit.stat_2433615566"; // Socketed Gems are supported by Level # Pierce

        internal const string CurseVulnerability = "explicit.stat_3967845372"; // Curse Enemies with Vulnerability on Hit
        internal const string CurseVulnerabilityChance = "explicit.stat_2213584313"; // #% chance to Curse Enemies with Vulnerability on Hit

        //local
        internal const string ArmorLocal = "explicit.stat_3484657501"; // # to Armour (Local)
        internal const string EsLocal = "explicit.stat_4052037485"; // # to maximum Energy Shield (Local)
        internal const string EvaLocal = "explicit.stat_53045048"; // # to Evasion Rating (Local)
        internal const string AccuracyLocal = "explicit.stat_691932474"; // # to Accuracy Rating (Local)

        //non-local
        internal const string Armor = "explicit.stat_809229260"; // # to Armour
        internal const string Es = "explicit.stat_3489782002"; // # to maximum Energy Shield
        internal const string Eva = "explicit.stat_2144192055"; // # to Evasion Rating
        internal const string Accuracy = "explicit.stat_803737631"; // # to Accuracy Rating

        internal const string PurityIce1 = "explicit.stat_151975117"; // Grants Level # Purity of Ice Skill
        internal const string PurityFire1 = "explicit.stat_3716281760"; // Grants Level # Purity of Fire Skill
        internal const string PurityLightning1 = "explicit.stat_1141249906"; // Grants Level # Purity of Lightning Skill

        internal const string PurityIce2 = "explicit.stat_4193390599"; // Grants Level # Purity of Ice Skill
        internal const string PurityFire2 = "explicit.stat_3970432307"; // Grants Level # Purity of Fire Skill
        internal const string PurityLightning2 = "explicit.stat_3822878124"; // Grants Level # Purity of Lightning Skill

        internal const string PrecisionEfficiencyOld = "explicit.stat_1291925008"; // Precision has 100% increased Mana Reservation Efficiency
        internal const string PrecisionEfficiencyNew = "explicit.stat_3859865977"; // Precision has #% increased Mana Reservation Efficiency

        internal const string SocketedInspiration1 = "explicit.stat_749770518"; // Socketed Gems are Supported by Level # Inspiration
        internal const string SocketedInspiration2 = "explicit.indexable_support_24"; // Socketed Gems are Supported by Level # Inspiration

        internal const string PeneFireTincture = "explicit.stat_1123291426"; // Damage Penetrates #% Fire Resistance
        internal const string PeneFire = "explicit.stat_2653955271"; // Damage Penetrates #% Fire Resistance

        internal const string PeneColdTincture = "explicit.stat_1211769158"; // Damage Penetrates #% Cold Resistance
        internal const string PeneCold = "explicit.stat_3417711605"; // Damage Penetrates #% Cold Resistance

        internal const string PeneLightTincture = "explicit.stat_3301510262"; // Damage Penetrates #% Lightning Resistance
        internal const string PeneLight = "explicit.stat_818778753"; // Damage Penetrates #% Lightning Resistance

        internal const string ManaPerKillTincture = "explicit.stat_782259898"; // Gain # Mana per Enemy Killed
        internal const string ManaPerKill = "explicit.stat_1368271171"; // Gain # Mana per Enemy Killed

        internal const string AoeKillTincture = "explicit.stat_923608573"; // #% increased Area of Effect if you've Killed Recently
        internal const string AoeKill = "explicit.stat_3481736410"; // #% increased Area of Effect if you've Killed Recently

        internal const string CritFullLifeTincture = "explicit.stat_3735443206"; // +#% to Critical Strike Multiplier against Enemies that are on Full Life
        internal const string CritFullLife = "explicit.stat_2355615476"; // +#% to Critical Strike Multiplier against Enemies that are on Full Life

        internal const string PhasingKillTincture = "explicit.stat_3669845133"; // #% chance to gain Phasing for 4 seconds on Kill
        internal const string PhasingKill = "explicit.stat_2918708827"; // #% chance to gain Phasing for 4 seconds on Kill

        internal const string ConcGroundTincture = "explicit.stat_4278270018"; // #% chance to create Consecrated Ground when you Hit a Rare or Unique Enemy, lasting 8 seconds
        internal const string ConcGround = "explicit.stat_3135669941"; // #% chance to create Consecrated Ground when you Hit a Rare or Unique Enemy, lasting 8 seconds

        internal const string StrikeRangeTincture = "explicit.stat_3369332977"; // +# metre to Melee Strike Range
        internal const string StrikeRange = "explicit.stat_2264295449"; // +# metres to Melee Strike Range

        internal const string ReduceEleGorgon = "explicit.stat_983989924"; // #% reduced Elemental Damage taken while stationary
        internal const string ReduceEle = "explicit.stat_3859593448"; // #% reduced Elemental Damage taken while stationary

        internal const string ShockSpreadEsh = "explicit.stat_1640259660"; // Shocks you inflict spread to other Enemies within 1.5 metres
        internal const string ShockSpread = "explicit.stat_424549222"; // Shocks you inflict spread to other Enemies within # metre

        internal const string ZombieBones = "explicit.stat_2739830820"; // +# to Level of all Raise Zombie Gems
        internal const string Zombie = "explicit.indexable_skill_16"; // +# to Level of all Raise Zombie Gems

        internal const string SpectreBones = "explicit.stat_3235814433"; // +# to Level of all Raise Spectre Gems
        internal const string Spectre = "explicit.indexable_skill_29"; // +# to Level of all Raise Spectre Gems

        internal const string StrIntCharm = "explicit.stat_2543977012"; // +# to Strength and Intelligence
        internal const string StrInt = "explicit.stat_1535626285"; // +# to Strength and Intelligence

        internal const string BlockDmgJewCharm = "explicit.stat_1702195217"; // +#% Chance to Block Attack Damage
        internal const string BlockDmg = "explicit.stat_2530372417"; // #% Chance to Block Attack Damage

        internal const string OnslaughtWeaponCharm = "explicit.stat_665823128"; // #% chance to gain Onslaught for 4 seconds on Kill
        internal const string Onslaught = "explicit.stat_3023957681"; // #% chance to gain Onslaught for 4 seconds on Kill
        internal const string OnslaughtAmulet = "explicit.stat_2453026567"; // #% chance to gain Onslaught for 10 seconds on Kill

        internal const string CoolDownRecovery1 = "explicit.stat_1004011302"; // #% increased Cooldown Recovery Rate
        internal const string CoolDownRecovery2 = "explicit.stat_239144"; // #% increased Cooldown Recovery Rate

        //veiled
        internal const string VeiledPrefix = "veiled.mod_65000"; // Veiled
        internal const string VeiledSuffix = "veiled.mod_63099"; // of the Veil

        /*
        internal const string TotalResistance = "+#% total Elemental Resistance";

        internal const string FlatPhysicalDamage = "Adds # to # Physical Damage";
        internal const string FlatAccuracyRating = "# to Accuracy Rating";
        internal const string FlatColdDamage = "Adds # to # Cold Damage";
        internal const string FlatLightningDamage = "Adds # to # Lightning Damage";
        internal const string FlatFireDamage = "Adds # to # Fire Damage";
        internal const string FlatChaosDamage = "Adds # to # Chaos Damage";

        internal const string EnergyShield = "#% increased Energy Shield";
        internal const string EvasionRating = "#% increased Evasion Rating";
        internal const string ArmourRating = "#% increased Armour";

        internal const string FlatEnergyShield = "# to maximum Energy Shield";
        internal const string FlatEvasionRating = "# to Evasion Rating";
        internal const string FlatArmourRating = "# to Armour";

        internal const string ArmourEvasionRating = "#% increased Armour and Evasion";
        internal const string ArmourEnergyShield = "#% increased Armour and Energy Shield";
        internal const string EvasionEnergyShield = "#% increased Evasion and Energy Shield";
        internal const string ArmourEvasionEnergyShield = "#% increased Armour, Evasion and Energy Shield";
        */

        internal static readonly Dictionary<string, string> dicPseudo = new()
        {
            { "stat_4220027924", "pseudo_total_cold_resistance" }, { "stat_3372524247", "pseudo_total_fire_resistance" }, { "stat_1671376347", "pseudo_total_lightning_resistance" }, { "stat_2923486259", "pseudo_total_chaos_resistance" },
            { "stat_3299347043", "pseudo_total_life" }, { "stat_1050105434", "pseudo_total_mana" }, { "stat_3489782002", "pseudo_total_energy_shield" }, { "stat_2482852589", "pseudo_increased_energy_shield" },
            { "stat_4080418644", "pseudo_total_strength" }, { "stat_3261801346", "pseudo_total_dexterity" }, { "stat_328541901", "pseudo_total_intelligence" },
            { "stat_681332047", "pseudo_total_attack_speed" }, { "stat_2891184298", "pseudo_total_cast_speed" }, { "stat_2250533757", "pseudo_increased_movement_speed" },
            { "stat_587431675", "pseudo_global_critical_strike_chance" }, { "stat_3556824919", "pseudo_global_critical_strike_multiplier" }, { "stat_737908626", "pseudo_critical_strike_chance_for_spells" },
            { "stat_1509134228", "pseudo_increased_physical_damage" }, { "stat_2974417149", "pseudo_increased_spell_damage" }, { "stat_3141070085", "pseudo_increased_elemental_damage" },
            { "stat_2231156303", "pseudo_increased_lightning_damage" }, { "stat_3291658075", "pseudo_increased_cold_damage" }, { "stat_3962278098", "pseudo_increased_fire_damage" },
            { "stat_4208907162", "pseudo_increased_lightning_damage_with_attack_skills" }, { "stat_860668586", "pseudo_increased_cold_damage_with_attack_skills" }, { "stat_2468413380", "pseudo_increased_fire_damage_with_attack_skills" }, { "stat_387439868", "pseudo_increased_elemental_damage_with_attack_skills" },
            { "stat_960081730", "pseudo_adds_physical_damage" }, { "stat_1334060246", "pseudo_adds_lightning_damage" }, { "stat_2387423236", "pseudo_adds_cold_damage" }, { "stat_321077055", "pseudo_adds_fire_damage" }, { "stat_3531280422", "pseudo_adds_chaos_damage" },
            { "stat_3032590688", "pseudo_adds_physical_damage_to_attacks" }, { "stat_1754445556", "pseudo_adds_lightning_damage_to_attacks" }, { "stat_4067062424", "pseudo_adds_cold_damage_to_attacks" }, { "stat_1573130764", "pseudo_adds_fire_damage_to_attacks" }, { "stat_674553446", "pseudo_adds_chaos_damage_to_attacks" },
            { "stat_2435536961", "pseudo_adds_physical_damage_to_spells" }, { "stat_2831165374", "pseudo_adds_lightning_damage_to_spells" }, { "stat_2469416729", "pseudo_adds_cold_damage_to_spells" }, { "stat_1133016593", "pseudo_adds_fire_damage_to_spells" }, { "stat_2300399854", "pseudo_adds_chaos_damage_to_spells" },
            { "stat_3325883026", "pseudo_total_life_regen" }, { "stat_836936635", "pseudo_percent_life_regen" }, { "stat_789117908", "pseudo_increased_mana_regen" }
        };

        internal static readonly Dictionary<string, string> dicCorruption = new()
        {
            { "implicit.stat_2551779822", "Boots" }, { "implicit.stat_1662717006", "Rings" }, { "implicit.stat_215124030", "Quivers" }, { "implicit.stat_3825877290", "Boots" }, { "implicit.stat_3964634628", "Rings" }, { "implicit.stat_3120164895", "Quivers" }, { "implicit.stat_2885144362", "Rings" }, { "implicit.stat_1040269876", "Quivers" },
            { "implicit.stat_742529963", "Bows,Quivers" }, { "implicit.stat_1787073323", "Quivers" }, { "implicit.stat_240289863", "Belts" }, { "implicit.stat_30642521", "Amulets" }, { "implicit.stat_2181129193", "Body Armours" }, { "implicit.stat_74338099", "Thrusting One Hand Swords" }, { "implicit.stat_484879947", "Rings" }, { "implicit.stat_1436284579", "Helmets" },
            { "implicit.stat_2530372417", "Amulets" }, { "implicit.stat_2451060005", "Fishing Rods" }, { "implicit.stat_1901158930", "Rings" }, { "implicit.stat_3835551335", "Rings" }, { "implicit.stat_1519615863", "One Hand Axes,Two Hand Axes" }, { "implicit.stat_3944782785", "One Hand Axes,Two Hand Axes" }, { "implicit.stat_2749862839", "Boots" }, { "implicit.stat_1582887649", "One Hand Maces,Staves,Two Hand Maces,Sceptres,Warstaves" },
            { "implicit.stat_1826802197", "Claws,Daggers,Bows,Rune Daggers" }, { "implicit.stat_3023957681", "One Hand Swords,Thrusting One Hand Swords,One Hand Axes,One Hand Maces" }, { "implicit.stat_3814876985", "Claws,Daggers,Wands,Staves,Sceptres,Rune Daggers,Warstaves" }, { "implicit.stat_3562211447", "Daggers,Wands,Rune Daggers" }, { "implicit.stat_696707743", "Boots" }, { "implicit.stat_3511815065", "Amulets" },
            { "implicit.stat_3999401129", "Amulets,Quivers,Helmets" }, { "implicit.stat_461472247", "Rings" }, { "implicit.stat_1658498488", "Jewel" }, { "implicit.stat_2764915899", "Gloves" }, { "implicit.stat_2028847114", "Gloves" }, { "implicit.stat_1625819882", "Gloves" }, { "implicit.stat_3433724931", "Gloves" }, { "implicit.stat_3967845372", "Gloves" }, { "implicit.stat_2044547677", "Rings" },
            { "implicit.stat_4265392510", "Shields" }, { "implicit.stat_2341269061", "Shields" }, { "implicit.stat_845428765", "Boots" }, { "implicit.stat_682182849", "Boots" }, { "implicit.stat_2166444903", "Claws" }, { "implicit.stat_3848282610", "Amulets,Quivers,Helmets" }, { "implicit.stat_3802667447", "Fishing Rods" }, { "implicit.stat_3310914132", "Fishing Rods" }, { "implicit.stat_2209668839", "Rings" },
            { "implicit.stat_1169502663", "Rings" }, { "implicit.stat_3868549606", "Thrusting One Hand Swords" }, { "implicit.stat_2843100721", "Gloves,Boots,Body Armours,Shields" }, { "implicit.stat_3556824919", "Claws,Daggers,Rune Daggers" }, { "implicit.stat_2867050084", "Shields" }, { "implicit.stat_1188846263", "Boots" }, { "implicit.stat_2429546158", "Rings" }, { "implicit.stat_2148556029", "Amulets" },
            { "implicit.stat_3224664127", "Rings" }, { "implicit.stat_4184565463", "Rings" }, { "implicit.stat_721014846", "Jewel" }, { "implicit.stat_782230869", "Jewel" }, { "implicit.stat_280731498", "Belts,Jewel,Wands,One Hand Swords,One Hand Axes,One Hand Maces,Sceptres,Staves,Two Hand Swords,Two Hand Axes,Two Hand Maces,Warstaves" }, { "implicit.stat_2572042788", "Gloves" },
            { "implicit.stat_681332047", "Amulets,Wands,Rings,Gloves,Bows,Claws,Daggers,One Hand Swords,Thrusting One Hand Swords,One Hand Axes,One Hand Maces,Sceptres,Rune Daggers,Staves,Two Hand Swords,Two Hand Axes,Two Hand Maces,Warstaves" }, { "implicit.stat_1365052901", "Belts" }, { "implicit.stat_1175385867", "Helmets" }, { "implicit.stat_2891184298", "Fishing Rods,Staves,Sceptres,Warstaves,Rings,Gloves" },
            { "implicit.stat_252194507", "Belts" }, { "implicit.stat_828179689", "Helmets" }, { "implicit.stat_587431675", "Jewel" }, { "implicit.stat_2898434123", "Belts" }, { "implicit.stat_2154246560", "Body Armours,Jewel" }, { "implicit.stat_967627487", "Daggers,Rune Daggers" }, { "implicit.stat_3377888098", "Belts,Gloves,Boots,Body Armours,Helmets,Shields" }, { "implicit.stat_1310194496", "Rings" },
            { "implicit.stat_836936635", "Helmets" }, { "implicit.stat_304970526", "Belts" }, { "implicit.stat_1923210508", "Quivers" }, { "implicit.stat_883169830", "Quivers" }, { "implicit.stat_2527686725", "Helmets" }, { "implicit.stat_791835907", "Gloves" }, { "implicit.stat_1645459191", "Helmets" }, { "implicit.stat_339179093", "Helmets" }, { "implicit.stat_4043416969", "Helmets" },
            { "implicit.stat_80079005", "Amulets,Quivers,Helmets" }, { "implicit.stat_3531280422", "Daggers,Rune Daggers" }, { "implicit.stat_2387423236", "Claws,Daggers,Wands,Sceptres,Rune Daggers,Bows,Staves,Warstaves" }, { "implicit.stat_709508406", "Claws,Daggers,Wands,Sceptres,Rune Daggers,Bows,Staves,Warstaves" }, { "implicit.stat_3336890334", "Claws,Daggers,Wands,Sceptres,Rune Daggers,Bows,Staves,Warstaves" },
            { "implicit.stat_1940865751", "One Hand Swords,One Hand Axes,One Hand Maces,Bows,Two Hand Swords,Two Hand Axes,Two Hand Maces" }, { "implicit.stat_4253454700", "Shields" }, { "implicit.stat_2375316951", "Claws,Wands,Thrusting One Hand Swords,Staves,Warstaves,Daggers,Sceptres,Rune Daggers" }, { "implicit.stat_1509134228", "Bows,One Hand Swords,One Hand Axes,One Hand Maces,Two Hand Swords,Two Hand Axes,Two Hand Maces" },
            { "implicit.stat_2974417149", "Daggers,Rune Daggers" }, { "implicit.stat_350598685", "Two Hand Axes" }, { "implicit.stat_1126826428", "Jewel" }, { "implicit.stat_820939409", "Rings" }, { "implicit.stat_1515657623", "Boots" }, { "implicit.stat_4078695", "Gloves" }, { "implicit.stat_227523295", "Helmets" }, { "implicit.stat_4124805414", "Shields" }, { "implicit.stat_569299859", "Amulets,Body Armours" },
            { "implicit.stat_1589917703", "Jewel" }, { "implicit.stat_2250533757", "Amulets,Boots" }, { "implicit.stat_561307714", "Amulets,Shields" }, { "implicit.stat_979246511", "Quivers" }, { "implicit.stat_369494213", "Quivers" }, { "implicit.stat_219391121", "Quivers" }, { "implicit.stat_1871056256", "Shields" }, { "implicit.stat_3342989455", "Shields" }, { "implicit.stat_425242359", "Shields" },
            { "implicit.stat_4129825612", "Shields" }, { "implicit.stat_2896346114", "Quivers" }, { "implicit.stat_3970432307", "Amulets" }, { "implicit.stat_4193390599", "Amulets" }, { "implicit.stat_3822878124", "Amulets" }, { "implicit.stat_105466375", "Amulets" }, { "implicit.stat_2960683632", "Body Armours" }, { "implicit.stat_3303114033", "Body Armours" }, { "implicit.stat_3001376862", "Shields" },
            { "implicit.stat_1425651005", "Shields" }, { "implicit.stat_1276918229", "Body Armours" }, { "implicit.stat_3855016469", "Body Armours,Shields" }, { "implicit.stat_2227180465", "Jewel" }, { "implicit.stat_2841027131", "Boots" }, { "implicit.stat_3943945975", "One Hand Swords,Thrusting One Hand Swords,Two Hand Swords" }, { "implicit.stat_1654414582", "Jewel" }, { "implicit.stat_2865550257", "Helmets" },
            { "implicit.stat_1567462963", "Two Hand Swords" }, { "implicit.stat_2223640518", "Bows" }, { "implicit.stat_3922006600", "Two Hand Axes" }, { "implicit.stat_99089516", "Bows" }, { "implicit.stat_107118693", "Two Hand Swords,Two Hand Axes,Two Hand Maces" }, { "implicit.stat_2032386732", "Two Hand Axes" }, { "implicit.stat_3237923082", "Bows,Two Hand Swords,Two Hand Axes,Two Hand Maces" },
            { "implicit.stat_1866911844", "Two Hand Axes,Two Hand Maces" }, { "implicit.stat_387439868", "Amulets" }, { "implicit.stat_2265307453", "Rings" }, { "implicit.stat_2115168758", "Gloves,Boots,Body Armours,Helmets,Shields" }, { "implicit.stat_2551600084", "Gloves,Boots,Body Armours,Helmets,Shields" }, { "implicit.stat_2452998583", "Gloves,Boots,Body Armours,Helmets,Shields" },
            { "implicit.stat_3691695237", "Gloves,Boots,Body Armours,Helmets,Shields" }, { "implicit.stat_150668988", "Gloves,Boots,Body Armours,Helmets,Shields" }, { "implicit.stat_1672793731", "Gloves,Boots,Body Armours,Helmets,Shields" }, { "implicit.stat_2176571093", "Gloves,Boots,Body Armours,Helmets,Shields" }, { "implicit.stat_983749596", "Belts,Gloves,Boots,Body Armours,Helmets,Shields" },
            { "implicit.stat_2482852589", "Belts,Gloves,Boots,Body Armours,Helmets,Shields" }, { "implicit.stat_3917489142", "Belts" }, { "implicit.stat_884586851", "Amulets,Rings,Belts" }, { "implicit.stat_2181791238", "Amulets,Rings,Belts" }, { "implicit.stat_1592278124", "Amulets,Rings,Belts" }, { "implicit.stat_3742945352", "Amulets, Rings, Belts" },
            { "implicit.stat_3653400807", "Amulets, Rings, Belts" }, { "implicit.stat_788317702", "Amulets, Rings, Belts" }, { "implicit.stat_397427740", "Amulets, Rings, Belts" }, { "implicit.stat_4175197580", "Amulets, Rings, Belts" }, { "implicit.stat_4096052153", "Amulets, Rings, Belts" }, { "implicit.stat_4247488219", "Amulets, Rings, Belts" },
            { "implicit.stat_656461285", "Amulets, Rings, Belts" }, { "implicit.stat_4139681126", "Amulets, Rings, Belts" }, { "implicit.stat_734614379", "Amulets, Rings, Belts" }, /*{ "implicit.stat_2901986750", "Amulets, Rings, Belts" },*/ { "implicit.stat_2653955271", "Jewel" }, { "implicit.stat_3417711605", "Jewel" }, { "implicit.stat_818778753", "Jewel" }, { "implicit.stat_2264523604", "Jewel" },
            { "implicit.stat_2101383955", "Wands, Thrusting One Hand Swords, Jewel" }, { "implicit.stat_2495041954", "Jewel" }, { "implicit.stat_notindatabase_notlegacy1", "Cannot be Ignited:Rings" }, { "implicit.stat_notindatabase_notlegacy2", "#% chance to Fortify on Melee hit:One Hand Swords,One Hand Axes,One Hand Maces" }, { "implicit.stat_notindatabase_legacy1", "#% reduced Fire Damage taken:Body Armours" },
            { "implicit.stat_2587176568", "Jewel" }

            /*{ "implicit.stat_2551779822", "Armour while stationary" }, { "implicit.stat_1662717006", "Cold Damage to Spells and Attacks" }, { "implicit.stat_215124030", "Cold Damage to Bow Attacks" }, { "implicit.stat_3825877290", "Global Evasion Rating while moving" }, { "implicit.stat_3964634628", "Fire Damage to Spells and Attacks" },
            { "implicit.stat_3120164895", "Fire Damage to Bow Attacks" }, { "implicit.stat_2885144362", "Lightning Damage to Spells and Attacks" }, { "implicit.stat_1040269876", "Lightning Damage to Bow Attacks" }, { "implicit.stat_742529963", "Bow Attacks fire an additional Arrow" },
            { "implicit.stat_1787073323", "Skills Chain # times" }, { "implicit.stat_240289863", "to Critical Strike Multiplier during any Flask Effect" }, { "implicit.stat_30642521", "You can apply an additional Curse" }, { "implicit.stat_2181129193", "additional Physical Damage Reduction while stationary" }, { "implicit.stat_74338099", "Skills fire an additional Projectile" },
            { "implicit.stat_484879947", "Grants Level # Anger Skill" }, { "implicit.stat_1436284579", "Cannot be Blinded" }, { "implicit.stat_2530372417", "Chance to Block Attack Damage" }, { "implicit.stat_2451060005", "You can catch Corrupted Fish" }, { "implicit.stat_1901158930", "Bleeding cannot be inflicted on you" },
            { "implicit.stat_3835551335", "Cannot be Poisoned" }, { "implicit.stat_1519615863", "#% chance to cause Bleeding on Hit" }, { "implicit.stat_3944782785", "increased Attack Damage against Bleeding Enemies" }, { "implicit.stat_2749862839", "chance to Dodge Attack Hits" },
            { "implicit.stat_1582887649", "chance to gain an Endurance Charge when you Stun an Enemy" }, { "implicit.stat_1826802197", "chance to gain a Frenzy Charge on Kill" }, { "implicit.stat_3023957681", "chance to gain Onslaught for 4 seconds on Kill" }, { "implicit.stat_3814876985", "chance to gain a Power Charge on Critical Strike" },
            { "implicit.stat_3562211447", "chance to gain Unholy Might for 3 seconds on Kill" }, { "implicit.stat_696707743", "chance to Dodge Spell Hits" }, { "implicit.stat_3511815065", "Grants Level # Clarity Skill" }, { "implicit.stat_3999401129", "#% of Cold Damage Leeched as Life" }, { "implicit.stat_461472247", "Grants Level # Conductivity Skill" },
            { "implicit.stat_1658498488", "Corrupted Blood cannot be inflicted on you" }, { "implicit.stat_2764915899", "Curse Enemies with Level # Despair on Hit" }, { "implicit.stat_2028847114", "Curse Enemies with Level # Elemental Weakness on Hit" }, { "implicit.stat_1625819882", "Curse Enemies with Level # Enfeeble on Hit" }, { "implicit.stat_3433724931", "Curse Enemies with Level # Temporal Chains on Hit" },
            { "implicit.stat_3967845372", "Curse Enemies with Level # Vulnerability on Hit" }, { "implicit.stat_2044547677", "Grants Level # Despair Skill" }, { "implicit.stat_4265392510", "Grants Level # Determination Skill" }, { "implicit.stat_2341269061", "Grants Level # Discipline Skill" }, { "implicit.stat_845428765", "chance to Dodge Attack Hits while moving" },
            { "implicit.stat_682182849", "chance to Dodge Spell Hits while moving" }, { "implicit.stat_2166444903", "Chance to Block Attack Damage while Dual Wielding" }, { "implicit.stat_3848282610", "#% of Fire Damage Leeched as Life" }, { "implicit.stat_3802667447", "#% increased Quantity of Fish Caught" },
            { "implicit.stat_3310914132", "#% increased Rarity of Fish Caught" }, { "implicit.stat_2209668839", "Grants Level # Flammability Skill" }, { "implicit.stat_1169502663", "Grants Level # Frostbite Skill" }, { "implicit.stat_3868549606", "Gain a Frenzy Charge after Spending a total of 200 Mana" }, { "implicit.stat_2843100721", "# to Level of Socketed Gems" },
            { "implicit.stat_3556824919", "to Global Critical Strike Multiplier" }, { "implicit.stat_2867050084", "Grants Level # Grace Skill" }, { "implicit.stat_1188846263", "Grants Level # Haste Skill" }, { "implicit.stat_2429546158", "Grants Level # Hatred Skill" }, { "implicit.stat_2148556029", "Grants Level # Malevolence Skill" }, { "implicit.stat_3224664127", "Grants Level # Zealotry Skill" },
            { "implicit.stat_4184565463", "Grants Level # Pride Skill" }, { "implicit.stat_721014846", "You cannot be Hindered" }, { "implicit.stat_782230869", "increased Effect of Non-Damaging Ailments" }, { "implicit.stat_280731498", "increased Area of Effect" }, { "implicit.stat_2572042788", "Attacks have #% to Critical Strike Chance" }, { "implicit.stat_681332047", "increased Attack Speed" },
            { "implicit.stat_1365052901", "increased Attack Speed during any Flask Effect" }, { "implicit.stat_1175385867", "increased Burning Damage" }, { "implicit.stat_2891184298", "increased Cast Speed" }, { "implicit.stat_252194507", "increased Cast Speed during any Flask Effect" }, { "implicit.stat_828179689", "increased Effect of Chill" }, { "implicit.stat_587431675", "increased Global Critical Strike Chance" },
            { "implicit.stat_2898434123", "increased Critical Strike Chance during any Flask Effect" }, { "implicit.stat_2154246560", "increased Damage" }, { "implicit.stat_967627487", "increased Damage over Time" }, { "implicit.stat_3377888098", "increased Skill Effect Duration" }, { "implicit.stat_1310194496", "increased Global Physical Damage" },
            { "implicit.stat_836936635", "Regenerate #% of Life per second" }, { "implicit.stat_304970526", "increased Movement Speed during any Flask Effect" }, { "implicit.stat_1923210508", "Projectiles deal #% increased Damage for each time they have Chained" }, { "implicit.stat_883169830", "Projectiles deal #% increased Damage for each Enemy Pierced" },
            { "implicit.stat_2527686725", "increased Effect of Shock" }, { "implicit.stat_791835907", "Spells have #% to Critical Strike Chance" }, { "implicit.stat_1645459191", "# to Level of Socketed Cold Gems" }, { "implicit.stat_339179093", "# to Level of Socketed Fire Gems" }, { "implicit.stat_4043416969", "# to Level of Socketed Lightning Gems" }, { "implicit.stat_80079005", "#% of Lightning Damage Leeched as Life" },
            { "implicit.stat_3531280422", "Adds # to # Chaos Damage" }, { "implicit.stat_2387423236", "Adds # to # Cold Damage" }, { "implicit.stat_709508406", "Adds # to # Fire Damage" }, { "implicit.stat_3336890334", "Adds # to # Lightning Damage" }, { "implicit.stat_1940865751", "Adds # to # Physical Damage" }, { "implicit.stat_4253454700", "#% Chance to Block (Shields)" },
            { "implicit.stat_2375316951", "increased Critical Strike Chance" }, { "implicit.stat_1509134228", "increased Physical Damage" }, { "implicit.stat_2974417149", "increased Spell Damage" }, { "implicit.stat_350598685", "# to Weapon Range" }, { "implicit.stat_1126826428", "You cannot be Maimed" }, { "implicit.stat_820939409", "Mana gained for each Enemy hit by your Attacks" },
            { "implicit.stat_1515657623", "# to Maximum Endurance Charges" }, { "implicit.stat_4078695", "# to Maximum Frenzy Charges" }, { "implicit.stat_227523295", "# to Maximum Power Charges" }, { "implicit.stat_4124805414", "#% to maximum Chance to Block Attack Damage" }, { "implicit.stat_569299859", "#% to all maximum Resistances" }, { "implicit.stat_1589917703", "Minions deal #% increased Damage" },
            { "implicit.stat_2250533757", "increased Movement Speed" }, { "implicit.stat_561307714", "#% Chance to Block Spell Damage" }, { "implicit.stat_979246511", "Gain #% of Physical Damage as Extra Cold Damage" }, { "implicit.stat_369494213", "Gain #% of Physical Damage as Extra Fire Damage" }, { "implicit.stat_219391121", "Gain #% of Physical Damage as Extra Lightning Damage" },
            { "implicit.stat_1871056256", "#% of Physical Damage from Hits taken as Cold Damage" }, { "implicit.stat_3342989455", "#% of Physical Damage from Hits taken as Fire Damage" }, { "implicit.stat_425242359", "#% of Physical Damage from Hits taken as Lightning Damage" }, { "implicit.stat_4129825612", "#% of Physical Damage from Hits taken as Chaos Damage" }, { "implicit.stat_2896346114", "Point Blank" },
            { "implicit.stat_3970432307", "Grants Level # Purity of Fire Skill" }, { "implicit.stat_4193390599", "Grants Level # Purity of Ice Skill" }, { "implicit.stat_3822878124", "Grants Level # Purity of Lightning Skill" }, { "implicit.stat_105466375", "Grants Level # Purity of Elements Skill" }, { "implicit.stat_2960683632", "#% reduced Chaos Damage taken" }, { "implicit.stat_3303114033", "#% reduced Cold Damage taken" },
            { "implicit.stat_3001376862", "#% reduced Area Damage taken from Hits" }, { "implicit.stat_1425651005", "#% reduced Damage taken from Projectiles" }, { "implicit.stat_1276918229", "#% reduced Lightning Damage taken" }, { "implicit.stat_3855016469", "You take #% reduced Extra Damage from Critical Strikes" }, { "implicit.stat_2227180465", "#% increased Mana Reserved (1% reduced Mana Reserved)" },
            { "implicit.stat_2841027131", "Regenerate # Life per second while moving" }, { "implicit.stat_3943945975", "Resolute Technique" }, { "implicit.stat_1654414582", "You cannot be Cursed with Silence" }, { "implicit.stat_2865550257", "Socketed Skill Gems get a #% Mana Multiplier" }, { "implicit.stat_1567462963", "Socketed Gems are supported by Level # Additional Accuracy" }, { "implicit.stat_2223640518", "Socketed Gems are supported by Level # Blind" },
            { "implicit.stat_3922006600", "Socketed Gems are Supported by Level # Blood Magic" }, { "implicit.stat_99089516", "Socketed Gems are supported by Level # Faster Projectiles" }, { "implicit.stat_107118693", "Socketed Gems are Supported by Level # Fortify" }, { "implicit.stat_2032386732", "Socketed Gems are Supported by Level # Life Gain On Hit" }, { "implicit.stat_3237923082", "Socketed Gems are Supported by Level # Onslaught" },
            { "implicit.stat_1866911844", "Socketed Gems are Supported by Level # Inspiration" }, { "implicit.stat_387439868", "#% increased Elemental Damage with Attack Skills" }, { "implicit.stat_2265307453", "Grants Level # Wrath Skill" }, { "implicit.stat_2115168758", "# to Level of Socketed Duration Gems" }, { "implicit.stat_2551600084", "# to Level of Socketed AoE Gems" }, { "implicit.stat_2452998583", "# to Level of Socketed Aura Gems" },
            { "implicit.stat_3691695237", "# to Level of Socketed Curse Gems" }, { "implicit.stat_150668988", "# to Level of Socketed Trap or Mine Gems" }, { "implicit.stat_1672793731", "# to Level of Socketed Warcry Gems" }, { "implicit.stat_2176571093", "# to Level of Socketed Projectile Gems" }, { "implicit.stat_983749596", "#% increased maximum Life" }, { "implicit.stat_2482852589", "#% increased maximum Energy Shield" },
            { "implicit.stat_3917489142", "#% increased Rarity of Items found" }, { "implicit.stat_884586851", "#% increased Quantity of Items found" }, { "implicit.stat_2181791238", "Wrath has #% increased Aura Effect" }, { "implicit.stat_1592278124", "Anger has #% increased Aura Effect" }, { "implicit.stat_3742945352", "Hatred has #% increased Aura Effect" }, { "implicit.stat_3653400807", "Determination has #% increased Aura Effect" },
            { "implicit.stat_788317702", "Discipline has #% increased Aura Effect" }, { "implicit.stat_397427740", "Grace has #% increased Aura Effect" }, { "implicit.stat_4175197580", "Malevolence has #% increased Aura Effect" }, { "implicit.stat_4096052153", "Zealotry has #% increased Aura Effect" }, { "implicit.stat_4247488219", "Pride has #% increased Aura Effect" }, { "implicit.stat_656461285", "#% increased Intelligence" },
            { "implicit.stat_4139681126", "#% increased Dexterity" }, { "implicit.stat_734614379", "#% increased Strength" }, { "implicit.stat_2901986750", "#% to all Elemental Resistances" }, { "implicit.stat_2653955271", "Damage Penetrates #% Fire Resistance" }, { "implicit.stat_3417711605", "Damage Penetrates #% Cold Resistance" }, { "implicit.stat_818778753", "Damage Penetrates #% Lightning Resistance" },
            { "implicit.stat_2101383955", "Damage Penetrates #% Elemental Resistances" }, { "implicit.stat_2495041954", "Enemies have #% to Total Physical Damage Reduction against your Hits" }, { "implicit.stat_notindatabase_notlegacy1", "Cannot be Ignited" }, { "implicit.stat_notindatabase_notlegacy2", "#% chance to Fortify on Melee hit" }, { "implicit.stat_notindatabase_legacy1", "#% reduced Fire Damage taken" }*/
        };

        internal static readonly List<string> lSpecialImplicits = new()
        {
            "implicit.stat_227523295", // # to Maximum Power Charges
            "implicit.stat_1515657623", // # to Maximum Endurance Charges
            "implicit.stat_4078695", // # to Maximum Frenzy Charges
            "implicit.stat_3967845372", // Curse Enemies with Vulnerability on Hit, with #% increased Effect
            "implicit.stat_2028847114", // Curse Enemies with Elemental Weakness on Hit, with #% increased Effect
            "implicit.stat_4096052153", // Zealotry has #% increased Aura Effect
            "implicit.stat_4175197580", // Malevolence has #% increased Aura Effect
            "implicit.stat_2763429652", // #% chance to Maim on Hit
            "implicit.stat_3023957681", // #% chance to gain Onslaught for 4 seconds on Kill
            "implicit.stat_3433724931", // Curse Enemies with Temporal Chains on Hit, with #% increased Effect
            "implicit.stat_30642521", // You can apply # additional Curses
            "implicit.stat_1619454789", // Onslaught
            "implicit.stat_2264523604", // #% increased Reservation of Skills
            "implicit.stat_1658498488", // Corrupted Blood cannot be inflicted on you
            "implicit.stat_2843100721", // # to Level of Socketed Gems
            "implicit.stat_1592278124", // Anger has #% increased Aura Effect
            "implicit.stat_4247488219", // Pride has #% increased Aura Effect
            "implicit.stat_2495041954", // Overwhelm #% Physical Damage Reduction
            "implicit.stat_2551600084", // # to Level of Socketed AoE Gems
            "implicit.stat_2176571093", // # to Level of Socketed Projectile Gems
            "implicit.stat_2115168758", // # to Level of Socketed Duration Gems
            "implicit.stat_788317702", // Discipline has #% increased Aura Effect
            "implicit.stat_2452998583", // # to Level of Socketed Aura Gems
            "implicit.stat_2181791238", // Wrath has #% increased Aura Effect
            "implicit.stat_3742945352", // Hatred has #% increased Aura Effect
            "implicit.stat_397427740", // Grace has #% increased Aura Effect
            "implicit.stat_2067062068", // Projectiles Pierce # additional Targets
            "implicit.stat_3753703249", // Gain #% of Physical Damage as Extra Damage of a random Element
            "implicit.stat_452077019", // Slaying Enemies in a kill streak grants Rampage bonuses
            "implicit.stat_3814876985", // #% chance to gain a Power Charge on Critical Strike
            "implicit.stat_3943945975", // Resolute Technique
            "implicit.stat_742529963", // Bow Attacks fire # additional Arrows
            "implicit.stat_1172810729", // #% chance to deal Double Damage
            "implicit.stat_2524254339", // Culling Strike
            "implicit.stat_2896346114", // Point Blank
            "implicit.stat_369494213", // Gain #% of Physical Damage as Extra Fire Damage
            "implicit.stat_2429546158", // Grants Level # Hatred Skill
                                        //"implicit.stat_484879947", // Grants Level # Anger Skill
            "implicit.stat_74338099", // Skills fire an additional Projectile
            "implicit.stat_350598685", // # to Weapon Range
            "implicit.stat_979246511", // Gain #% of Physical Damage as Extra Cold Damage
            "implicit.stat_2192875806", // Socketed Skills apply Fire, Cold and Lightning Exposure on Hit
            "implicit.stat_219391121", // Gain #% of Physical Damage as Extra Lightning Damage
            "implicit.stat_3224664127", // Grants Level # Zealotry Skill
            "implicit.stat_2341269061", // Grants Level # Discipline Skill
            "implicit.stat_1181501418", // # to Maximum Rage
            "implicit.stat_3240769289", // #% of Physical Damage Converted to Lightning Damage
            "implicit.stat_1533563525", // #% of Physical Damage Converted to Fire Damage
            "implicit.stat_2133341901", // #% of Physical Damage Converted to Cold Damage
            "implicit.stat_338121249", // Curse Enemies with Flammability on Hit, with #% increased Effect
            "implicit.stat_4154259475", // # to Level of Socketed Support Gems
            "implicit.stat_1220361974", // Enemies you Kill Explode, dealing #% of their Life as Physical Damage
            "implicit.stat_1263158408", // Elemental Equilibrium
            "implicit.stat_710372469", // Curse Enemies with Conductivity on Hit, with #% increased Effect
            "implicit.stat_1866911844", // Socketed Gems are Supported by Level # Inspiration
            "implicit.stat_426847518", // Curse Enemies with Frostbite on Hit, with #% increased Effect
            "implicit.stat_3574189159", // Elemental Overload
            "implicit.stat_1880071428", // #% increased effect of Non-Curse Auras from your Skills
            "implicit.stat_1787073323", // Skills Chain # times
            "implicit.stat_1001077145", // Arrows Chain # times
            "implicit.stat_4223377453", // #% increased Brand Attachment range
        };

        internal static readonly List<string> lMagnitudeImplicits = new()
        {
            "implicit.stat_1794120699", // #% increased Prefix Modifier magnitudes
            "implicit.stat_1033086302", // #% increased Suffix Modifier magnitudes
            "implicit.stat_1581907402" // #% increased Explicit Modifier magnitudes
        };

        internal static readonly Dictionary<string, bool> dicDefaultPosition = new()
        {
            { "stat_3441651621", true }, { "stat_3853018505", true }, { "stat_969865219", true }, { "stat_4176970656", true },
            { "stat_3277537093", true }, { "stat_3691641145", true }, { "stat_3557561376", true }, { "stat_705686721", true },
            { "stat_2156764291", true }, { "stat_3743301799", true }, { "stat_1187803783", true }, { "stat_3612407781", true },
            { "stat_496011033", true }, { "stat_1625103793", true }, { "stat_308618188", true }, { "stat_2590715472", true },
            { "stat_1964333391", true }, { "stat_614758785", true }, { "stat_2440172920", true }, { "stat_321765853", true },
            { "stat_465051235", true }, { "stat_261654754", true }, { "stat_3522931817", true }, { "stat_1443108510", true },
            { "stat_2477636501", true }//, { "stat_2901986750", true}
        };
    }

    internal static class StatPoe2
    {
        //explicit
        internal const string RecoverManaKill1 = "explicit.stat_1030153674"; // Recover #% of Mana on Kill
        internal const string RecoverManaKill2 = "explicit.stat_1604736568"; // Recover #% of Mana on Kill
        internal const string IncArmour1 = "explicit.stat_1062208444"; // #% increased Armour
        internal const string IncArmour2 = "explicit.stat_2866361420"; // #% increased Armour
        internal const string IncEvasion1 = "explicit.stat_124859000"; // #% increased Evasion Rating
        internal const string IncEvasion2 = "explicit.stat_2106365538"; // #% increased Evasion Rating
        internal const string IncDuration1 = "explicit.stat_1256719186"; // #% increased Duration
        internal const string IncDuration2 = "explicit.stat_2541588185"; // #% increased Duration
        internal const string CharmSlot1 = "explicit.stat_1416292992"; // # Charm Slot
        internal const string CharmSlot2 = "explicit.stat_554899692"; // # Charm Slot
        internal const string IncAs1 = "explicit.stat_210067635"; // #% increased Attack Speed
        internal const string IncAs2 = "explicit.stat_681332047"; // #% increased Attack Speed
        internal const string EvasionRating1 = "explicit.stat_2144192055"; // # to Evasion Rating
        internal const string EvasionRating2 = "explicit.stat_53045048"; // # to Evasion Rating
        internal const string IncBlock1 = "explicit.stat_2481353198"; // #% increased Block chance
        internal const string IncBlock2 = "explicit.stat_4147897060"; // #% increased Block chance
        internal const string Armour1 = "explicit.stat_3484657501"; // # to Armour
        internal const string Armour2 = "explicit.stat_809229260"; // # to Armour
        internal const string EnergyShield1 = "explicit.stat_3489782002"; // # to maximum Energy Shield
        internal const string EnergyShield2 = "explicit.stat_4052037485"; // # to maximum Energy Shield
        internal const string IncXpGain1 = "explicit.stat_3666934677"; // #% increased Experience gain
        internal const string IncXpGain2 = "explicit.stat_57434274"; // #% increased Experience gain
        internal const string ChancePoison1 = "explicit.stat_3885634897"; // #% chance to Poison on Hit
        internal const string ChancePoison2 = "explicit.stat_795138349"; // #% chance to Poison on Hit
        internal const string AccuracyRating1 = "explicit.stat_691932474"; // # to Accuracy Rating
        internal const string AccuracyRating2 = "explicit.stat_803737631"; // # to Accuracy Rating
        // 0.3
        internal const string IncSpirit1 = "explicit.stat_1416406066"; // #% increased Spirit
        internal const string IncSpirit2 = "explicit.stat_3984865854"; // #% increased Spirit
        internal const string Spirit1 = "explicit.stat_2704225257"; // # to Spirit
        internal const string Spirit2 = "explicit.stat_3981240776"; // # to Spirit
        internal const string Daze1 = "explicit.stat_2933846633"; // Dazes on Hit
        internal const string Daze2 = "explicit.stat_3146310524"; // Dazes on Hit
        internal const string Aftershocks1 = "explicit.stat_1157523820"; // #% chance for Slam Skills you use yourself to cause Aftershocks
        internal const string Aftershocks2 = "explicit.stat_2045949233"; // #% chance for Slam Skills you use yourself to cause Aftershocks
        internal const string DeliFog1 = "explicit.stat_1174954559"; // Delirium Fog in Area lasts # additional seconds before dissipating
        internal const string DeliFog2 = "explicit.stat_3226351972"; // Delirium Fog in Area lasts # additional seconds before dissipating
        internal const string RandomShrine1 = "explicit.stat_2625554454"; // Every 10 seconds, gain a random non-damaging Shrine buff for 20 seconds
        internal const string RandomShrine2 = "explicit.stat_2879778895"; // Every 10 seconds, gain a random non-damaging Shrine buff for 20 seconds

        internal const string FireResistance = "explicit.stat_3372524247"; // #% to Fire Resistance
        internal const string ColdResistance = "explicit.stat_4220027924"; // #% to Cold Resistance
        internal const string LightningResistance = "explicit.stat_1671376347"; // #% to Lightning Resistance

        internal const string Strength = "explicit.stat_4080418644"; // # to Strength
        internal const string Dexterity = "explicit.stat_3261801346"; // # to Dexterity
        internal const string Intelligence = "explicit.stat_328541901"; // # to Intelligence

        //explicit specific
        internal const string AsPerDex1 = "explicit.stat_720908147"; // #% increased Attack Speed per 20 Dexterity (Hand of Wisdom and Action)
        internal const string AsPerDex2 = "explicit.stat_889691035"; // #% increased Attack Speed per 10 Dexterity (Pillar of the Caged God)
        internal const string AsPerDex3 = "explicit.stat_2241560081"; // #% increased Attack Speed per 25 Dexterity
        //enchant
        internal const string IncArmourEnch1 = "enchant.stat_1062208444"; // #% increased Armour
        internal const string IncArmourEnch2 = "enchant.stat_2866361420"; // #% increased Armour
        internal const string IncEvasionEnch1 = "enchant.stat_124859000"; // #% increased Evasion Rating
        internal const string IncEvasionEnch2 = "enchant.stat_2106365538"; // #% increased Evasion Rating

        //skill
        internal const string SkillLightningBolt = "skill.lightning_bolt"; // Grants Skill: Level # Lightning Bolt
        internal const string SkillLightningBoltUnique = "skill.unique_breach_lightning_bolt"; // Grants Skill: Level # Lightning Bolt

        //no duplicate
        internal const string IncEs = "stat_4015621042"; // #% increased Energy Shield
        internal const string IncArEs = "stat_3321629045"; // #% increased Armour and Energy Shield
        internal const string IncArEva = "stat_2451402625"; // #% increased Armour and Evasion
        internal const string IncArEvaEs = "stat_3523867985"; // #% increased Armour, Evasion and Energy Shield
        internal const string IncEvaEs = "stat_1999113824"; // #% increased Evasion and Energy Shield
        
        internal const string IncPhys = "stat_1509134228"; // #% increased Physical Damage
        internal const string AddPhys = "stat_1940865751"; // Adds # to # Physical Damage
        internal const string AddFire = "stat_709508406"; // Adds # to # Fire Damage
        internal const string AddCold = "stat_1037193709"; // Adds # to # Cold Damage
        internal const string AddLight = "stat_3336890334"; // Adds # to # Lightning Damage

        internal static readonly List<string> lDefenceMods = new()
        {
            IncArmour1.Split('.')[1], IncArmour2.Split('.')[1], 
            IncEvasion1.Split('.')[1], IncEvasion2.Split('.')[1], 
            EvasionRating1.Split('.')[1], EvasionRating2.Split('.')[1],
            Armour1.Split('.')[1], Armour2.Split('.')[1], 
            EnergyShield1.Split('.')[1], EnergyShield2.Split('.')[1], 
            IncEs, IncArEs, IncArEva, IncArEvaEs, IncEvaEs
        };

        internal static readonly List<string> lWeaponMods = new()
        {
            IncPhys, AddPhys, AddFire, AddCold, AddLight
        };
    }

    internal static class Cdn
    {
        internal const string Url = "https://web.poecdn.com";
        internal static readonly string Cards = Url + "/image/Art/2DItems/Divination/InventoryIcon.png?v=a8ae131b97fad3c64de0e6d9f250d743";
        internal static readonly string Prophecies = Url + "/image/Art/2DItems/Currency/ProphecyOrbRed.png?v=dc9105d2b038a79c7c316fc2ba30cef0";
        internal static readonly string MapsUnique = Url + "/gen/image/WzI4LDE0LHsiZiI6IjJESXRlbXNcL01hcHNcL1BlcmFuZHVzTWFub3IiLCJ3IjoxLCJoIjoxLCJzY2FsZSI6dHJ1ZSwicmVsaWMiOnRydWV9XQ/9ebdf3dc05/Item.png";
        internal static readonly string Beasts = Url + "/gen/image/WzI1LDE0LHsiZiI6IjJESXRlbXMvQ3VycmVuY3kvQmVzdGlhcnlPcmJGdWxsIiwidyI6MSwiaCI6MSwic2NhbGUiOjF9XQ/3214b44360/BestiaryOrbFull.png";
        internal static readonly string Heist = Url + "/gen/image/WzI1LDE0LHsiZiI6IjJESXRlbXMvQ3VycmVuY3kvSGVpc3QvQmx1ZXByaW50Tm90QXBwcm92ZWQiLCJ3IjoxLCJoIjoxLCJzY2FsZSI6MX1d/03e0a85e57/BlueprintNotApproved.png";
        internal static readonly string Sanctum = Url + "/gen/image/WzI1LDE0LHsiZiI6IjJESXRlbXMvQ3VycmVuY3kvU2FuY3R1bS9TYW5jdHVtS2V5IiwidyI6MSwiaCI6MSwic2NhbGUiOjF9XQ/d0326cac9a/SanctumKey.png";
        internal static readonly string ScoutingReport = Url + "/gen/image/WzI1LDE0LHsiZiI6IjJESXRlbXMvQ3VycmVuY3kvU2NvdXRpbmdSZXBvcnQiLCJ3IjoxLCJoIjoxLCJzY2FsZSI6MX1d/584635f3c8/ScoutingReport.png";
        internal static readonly string MapsBlightedWhite = Url + "/gen/image/WzI4LDE0LHsiZiI6IjJESXRlbXMvTWFwcy9BdGxhczJNYXBzL05ldy9Qcm9tZW5hZGUiLCJ3IjoxLCJoIjoxLCJzY2FsZSI6MSwibW4iOjEzLCJtdCI6NSwibWIiOnRydWV9XQ/0f67379a8d/Promenade.png";
        internal static readonly string MapsBlightedYellow = Url + "/gen/image/WzI4LDE0LHsiZiI6IjJESXRlbXMvTWFwcy9BdGxhczJNYXBzL05ldy9MdW5hcmlzVGVtcGxlIiwidyI6MSwiaCI6MSwic2NhbGUiOjEsIm1uIjoxMiwibXQiOjYsIm1iIjp0cnVlfV0/d86c0a2db5/LunarisTemple.png";
        internal static readonly string MapsBlightedRed = Url + "/gen/image/WzI4LDE0LHsiZiI6IjJESXRlbXMvTWFwcy9BdGxhczJNYXBzL05ldy9CdXJuIiwidyI6MSwiaCI6MSwic2NhbGUiOjEsIm1uIjoyMCwibXQiOjEzLCJtYiI6dHJ1ZX1d/e5e5a7c466/Burn.png";
        internal static readonly string MapsWhite = Url + "/gen/image/WzI4LDE0LHsiZiI6IjJESXRlbXNcL01hcHNcL0F0bGFzMk1hcHNcL05ld1wvSnVuZ2xlVmFsbGV5IiwidyI6MSwiaCI6MSwic2NhbGUiOnRydWUsIm1uIjoxMCwibXQiOjF9XQ/a6c8839eb3/Item.png";
        internal static readonly string MapsYellow = Url + "/gen/image/WzI4LDE0LHsiZiI6IjJESXRlbXNcL01hcHNcL0F0bGFzMk1hcHNcL05ld1wvSnVuZ2xlVmFsbGV5IiwidyI6MSwiaCI6MSwic2NhbGUiOnRydWUsIm1uIjoxMCwibXQiOjEwfV0/77d3b99f0f/Item.png";
        internal static readonly string MapsRed = Url + "/gen/image/WzI4LDE0LHsiZiI6IjJESXRlbXNcL01hcHNcL0F0bGFzMk1hcHNcL05ld1wvSnVuZ2xlVmFsbGV5IiwidyI6MSwiaCI6MSwic2NhbGUiOnRydWUsIm1uIjoxMCwibXQiOjE0fV0/9b6fde25af/Item.png";
    }

    // Collections

    internal static readonly Dictionary<string, string> dicStones = new()
    {
        { "timeless-eternal-emblem", "Timeless Eternal Emblem" }, { "timeless-karui-emblem", "Timeless Karui Emblem" }, { "timeless-maraketh-emblem", "Timeless Maraketh Emblem" },
        { "timeless-templar-emblem", "Timeless Templar Emblem" }, { "timeless-vaal-emblem", "Timeless Vaal Emblem" },
        { "unrelenting-timeless-eternal-emblem", "Unrelenting Timeless Eternal Emblem" }, { "unrelenting-timeless-karui-emblem", "Unrelenting Timeless Karui Emblem" }, { "unrelenting-timeless-maraketh-emblem", "Unrelenting Timeless Maraketh Emblem" },
        { "unrelenting-timeless-templar-emblem", "Unrelenting Timeless Templar Emblem" }, { "unrelenting-timeless-vaal-emblem", "Unrelenting Timeless Vaal Emblem" },
        { "simulacrum", "Simulacrum" },
        { "chayulas-breachstone", "Chayula's Breachstone" }, { "chayulas-charged-breachstone", "Chayula's Charged Breachstone" }, { "chayulas-enriched-breachstone", "Chayula's Enriched Breachstone" },
        { "chayulas-pure-breachstone", "Chayula's Pure Breachstone" }, { "chayulas-flawless-breachstone", "Chayula's Flawless Breachstone" },
        { "eshs-breachstone", "Esh's Breachstone" }, { "eshs-charged-breachstone", "Esh's Charged Breachstone" },
        { "eshs-enriched-breachstone", "Esh's Enriched Breachstone" }, { "eshs-pure-breachstone", "Esh's Pure Breachstone" }, { "eshs-flawless-breachstone", "Esh's Flawless Breachstone" },
        { "tuls-breachstone", "Tul's Breachstone" },
        { "tuls-charged-breachstone", "Tul's Charged Breachstone" }, { "tuls-enriched-breachstone", "Tul's Enriched Breachstone" }, { "tuls-pure-breachstone", "Tul's Pure Breachstone" },
        { "tuls-flawless-breachstone", "Tul's Flawless Breachstone" },
        { "uul-breachstone", "Uul-Netol's Breachstone" }, { "uul-charged-breachstone", "Uul-Netol's Charged Breachstone" }, { "uul-enriched-breachstone", "Uul-Netol's Enriched Breachstone" },
        { "uul-pure-breachstone", "Uul-Netol's Pure Breachstone" }, { "uul-flawless-breachstone", "Uul-Netol's Flawless Breachstone" },
        { "xophs-breachstone", "Xoph's Breachstone" }, { "xophs-charged-breachstone", "Xoph's Charged Breachstone" },
        { "xophs-enriched-breachstone", "Xoph's Enriched Breachstone" }, { "xophs-pure-breachstone", "Xoph's Pure Breachstone" }, { "xophs-flawless-breachstone", "Xoph's Flawless Breachstone" }
    };

    internal static readonly Dictionary<string, string> dicMainCur = new()
    {
        { "alch", "Orb of Alchemy" }, { "alt", "Orb of Alteration" }, { "ancient-orb", "Ancient Orb" },
        { "annul", "Orb of Annulment" }, { "aug", "Orb of Augmentation" }, { "bauble", "Glassblower's Bauble" },
        { "blessed", "Blessed Orb" }, { "chance", "Orb of Chance" }, { "chaos", "Chaos Orb" }, { "chisel", "Cartographer's Chisel" },
        { "chrome", "Chromatic Orb" }, { "divine", "Divine Orb" }, { "engineers", "Engineer's Orb" }, { "exalted", "Exalted Orb" },
        { "fusing", "Orb of Fusing" }, { "gcp", "Gemcutter's Prism" }, { "harbingers-orb", "Harbinger's Orb" },
        { "infused-engineers-orb", "Infused Engineer's Orb" }, { "jewellers", "Jeweller's Orb" }, { "instilling-orb", "Instilling Orb" },
        { "mirror", "Mirror of Kalandra" }, { "orb-of-horizons", "Orb of Horizons" }, { "scour", "Orb of Scouring" },
        { "regal", "Regal Orb" }, { "regret", "Orb of Regret" }, { "orb-of-binding", "Orb of Binding" },
        { "tailoring-orb", "Tailoring Orb" }, { "tempering-orb", "Tempering Orb" }, { "transmute", "Orb of Transmutation" },
        { "vaal", "Vaal Orb" }, { "orb-of-unmaking", "Orb of Unmaking" }, { "sacred-orb", "Sacred Orb" }, { "veiled-orb", "Veiled Orb" },
        { "fracturing-orb", "Fracturing Orb" }, { "stacked-deck", "Stacked Deck" }, { "enkindling-orb", "Enkindling Orb" }
    };

    internal static readonly Dictionary<string, string> dicExoticCur = new()
    {
        { "awakeners-orb", "Awakener's Orb" } , { "crusaders-exalted-orb", "Crusader's Exalted Orb" }, { "hunters-exalted-orb", "Hunter's Exalted Orb" },
        { "redeemers-exalted-orb", "Redeemer's Exalted Orb" }, { "warlords-exalted-orb", "Warlord's Exalted Orb" }, { "mavens-orb", "Orb of Dominance" },

        { "eldritch-chaos-orb", "Eldritch Chaos Orb" } , { "eldritch-exalted-orb", "Eldritch Exalted Orb" } ,{ "eldritch-orb-of-annulment", "Eldritch Orb of Annulment" } ,
        { "lesser-eldritch-ember", "Lesser Eldritch Ember" } , { "greater-eldritch-ember", "Greater Eldritch Ember" } ,{ "grand-eldritch-ember", "Grand Eldritch Ember" } ,
        { "exceptional-eldritch-ember", "Exceptional Eldritch Ember" } , { "lesser-eldritch-ichor", "Lesser Eldritch Ichor" } ,{ "greater-eldritch-ichor", "Greater Eldritch Ichor" } ,
        { "grand-eldritch-ichor", "Grand Eldritch Ichor" } , { "exceptional-eldritch-ichor", "Exceptional Eldritch Ichor" } ,{ "orb-of-conflict", "Orb of Conflict" } ,
        { "tainted-chromatic-orb", "Tainted Chromatic Orb" } , { "tainted-orb-of-fusing", "Tainted Orb of Fusing" } ,{ "tainted-jewellers-orb", "Tainted Jeweller's Orb" } ,
        { "tainted-chaos-orb", "Tainted Chaos Orb" } , { "tainted-exalted-orb", "Tainted Exalted Orb" } ,{ "tainted-mythic-orb", "Tainted Mythic Orb" } ,
        { "tainted-armourers-scrap", "Tainted Armourer's Scrap" } , { "tainted-blacksmiths-whetstone", "Tainted Blacksmith's Whetstone" } ,{ "tainted-divine-teardrop", "Tainted Divine Teardrop" } ,
        { "wild-lifeforce", "Wild Crystallised Lifeforce" } , { "vivid-lifeforce", "Vivid Crystallised Lifeforce" } ,{ "primal-lifeforce", "Primal Crystallised Lifeforce" } ,
        { "sacred-lifeforce", "Sacred Crystallised Lifeforce" } , { "hinekoras-lock", "Hinekora's Lock" }
    };

    internal static readonly Dictionary<string, string> dicWantToBuy = new()
    {
        { "Hi, I'd like to buy your", "en" }, { "Hi, I would like to buy your", "en" },
        { "Здравствуйте, хочу купить у вас", "ru" }, { "Hi, ich möchte", "de" }, { "안녕하세요,", "kr" },
        { "Hola, me gustaría comprar", "es" }, { "Hola, quisiera comprar", "es" },
        { "Olá, eu gostaria de comprar", "pt" }, { "Bonjour, je voudrais t'acheter", "fr" },
        { "Bonjour, je souhaiterais t'acheter", "fr" },
        { "你好，我想購買", "tw" }, { "你好，我希望购买", "cn" },
        { "สวัสดีฉันต้องการซื้อค", "th" }, { "สวัสดี, เราต้องการจะชื้อของคุณ", "th" }, { "สวัสดี เราต้องการชื้อ", "th" },
        { "สวัสดี เราต้องการแลก", "th" }, {"こんにちは、私は", "jp"}
    };

    internal static readonly Dictionary<string, string> dicCurrencyChars = new()
    {
        { "-map", string.Empty }, { "-tier-", "-t" }, { "vial-of-", string.Empty },
        { "essence-of-", string.Empty }, { "-fossil", string.Empty }, { "-resonator", string.Empty },
        { "-scarab", string.Empty }, { "-incubator", string.Empty }, { "-oil", string.Empty },
        { "-catalyst", string.Empty }, { "timeless-", string.Empty }, { "-sextant", string.Empty },
        { "orb-of-", string.Empty }, { "-orb", string.Empty }, { "-breachstone", string.Empty },
        { "fragment-of-", string.Empty }, { "-artifact", string.Empty }
    };

    internal static readonly Dictionary<string, string> dicOptionText = new()
    {
        { "Axe Attacks deal 12% increased Damage with Hits and Ailments\nSword Attacks deal 12% increased Damage with Hits and Ailments", "Axe or Sword Attacks deal 12% increased Damage with Hits and Ailments" },
        { "Staff Attacks deal 12% increased Damage with Hits and Ailments\nMace or Sceptre Attacks deal 12% increased Damage with Hits and Ailments", "Staff, Mace or Sceptre Attacks deal 12% increased Damage with Hits and Ailments" },
        { "Claw Attacks deal 12% increased Damage with Hits and Ailments\nDagger Attacks deal 12% increased Damage with Hits and Ailments", "Claw or Dagger Attacks deal 12% increased Damage with Hits and Ailments" },
        { "12% increased Damage with Bows\n12% increased Damage Over Time with Bow Skills", "12% increased Damage with Bows and DoT with Bow Skills" },
        { "12% increased Trap Damage\n12% increased Mine Damage", "12% increased Trap and Mine Damage" },
        { "10% increased Life Recovery from Flasks\n10% increased Mana Recovery from Flasks", "10% increased Life and Mana Recovery from Flasks" }
    };

    internal static readonly List<string> lTotalStatLifeUnwanted = new()
    { "per", "added small passive", "strength provides no bonus", "raised zombies have",
      "intelligence allocated in radius", "intelligence from passives", "dexterity from passives"
    };

    internal static readonly List<string> lTotalStatEsUnwanted = new()
    { "per", "added small passive", "left ring slot"
    };

    internal static readonly List<string> lTotalStatResistUnwanted = new()
    { "per", "added small passive", "effect", "maximum", "corrupted", "against", "while", "penetrate", "minions",
      "summoned", "enemies", "zombies", "totem", "chance"
    };

    internal static readonly List<string> lSpecialBases = new()
    {
        //"Astral Plate", "Assassin's Garb", "Occultist's Vestment", "Carnal Armour",
        //"Two-Toned Boots", "Fugitive Boots",
        "Stygian Vise" , "Crystal Belt",
        "Sacrificial Garb", "Basemetal Treads", "Darksteel Treads", "Brimstone Treads",
        "Cloudwhisper Boots", "Windbreak Boots", "Stormrider Boots", "Duskwalk Slippers",
        "Nightwind Slippers", "Dreamquest Slippers", "Taxing Gauntlets", "Gruelling Gauntlets",
        "Debilitation Gauntlets", "Spiked Gloves", "Gauche Gloves", "Southswing Gloves",
        "Gripped Gloves", "Sinistral Gloves", "Leyline Gloves", "Aetherwind Gloves",
        "Fingerless Silk Gloves", "Nexus Gloves", "Apothecary's Gloves", "Sorrow Mask",
        "Atonement Mask", "Penitent Mask", "Imp Crown", "Demon Crown", "Bone Helmet",
        "Archdemon Crown", "Gale Crown", "Winter Crown", "Blizzard Crown",
        "Exothermic Tower Shield", "Magmatic Tower Shield", "Heat-attuned Tower Shield",
        "Endothermic Buckler", "Polar Buckler", "Cold-attuned Buckler", "Exhausting Spirit Shield",
        "Subsuming Spirit Shield", "Transfer-attuned Spirit Shield",
        //"Eclipse Staff",
        "Oscillating Sceptre", "Stabilising Sceptre", "Alternating Sceptre", "Flare Mace",
        "Crack Mace", "Boom Mace", "Blunt Force Condenser", "Crushing Force Magnifier",
        "Impact Force Propagator", "Maltreatment Axe", "Disapprobation Axe",
        "Psychotic Axe", "Vaal Axe", "Apex Cleaver", "Fleshripper", "Fickle Spiritblade",
        "Capricious Spiritblade", "Anarchic Spiritblade", "Rebuking Blade",
        "Blasting Blade", "Banishing Blade", "Exquisite Blade", "Hedron Bow",
        "Foundry Bow", "Solarine Bow", "Shadow Fangs", "Malign Fangs",
        "Void Fangs", "Hollowpoint Dagger", "Pressurised Dagger", "Pneumatic Dagger",
        "Flickerflame Blade", "Flashfire Blade", "Infernal Blade", "Transformer Staff",
        "Reciprocation Staff", "Battery Staff", "Capacity Rod", "Potentiality Rod",
        "Eventuality Rod", "Assembler Wand", "Congregator Wand", "Accumulator Wand", "Convoking Wand"
    };

    internal static readonly Dictionary<string, string> dicFastenshteinExclude = new()
    {
        { "explicit.stat_2401834120", "Added Small Passive Skills also grant: #% increased Damage over Time" }
    };
}