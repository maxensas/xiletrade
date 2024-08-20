using System.Collections.Generic;

namespace Xiletrade.Library.Shared;

/// <summary> Static class containing ALL and ONLY global constants strings.</summary>
/// <remarks> Does not respect intentionally naming conventions for global constants.</remarks>
public static class Strings 
{
    // public members
    public const string UrlGithubVersion = "https://raw.githubusercontent.com/maxensas/xiletrade/master/version_win.xml";

    public static class WindowName
    {
        //public const string Main = "XileTrade";
        public const string Config = "Configuration";
        public const string Editor = "Editor";
        //public const string Start = "StartWindow";
        public const string Whisper = "WhisperListWindow";
        public const string Popup = "PopupWindow";
    }

    // internal members
    /// <summary>Carriage Return + Line Feed</summary>
    internal const string CRLF = "\r\n";
    /// <summary>Line Feed</summary>
    internal const string LF = "\n";
    internal const string DetailListFormat1 = "{0,5} {1,-12} {2,3} {3,-23} {4}{5}: {6}";
    internal const string DetailListFormat2 = "{0,5} {1,-12} {2,3} {3,-8} {4}{5}{6,2} {7,8}: {8}";
    internal const string PoeClass = "POEWindowClass";
    internal const string PoeCaption = "Path of Exile";
    internal const string Info = " [Xiletrade POE Helper]";
    internal const string TrueOption = "_TRUE_";
    internal const string Prophecy = "Prophecy";
    internal const string Blight = "Blight";
    internal const string Ravaged = "Ravaged";
    internal const string Maps = "Maps";
    internal const string Delve = "Delve";
    internal const string Captured = "Captured";
    internal const string AlternateGem = "Alternate Gem";
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

    internal const string UrlPoeWiki = "https://www.poewiki.net/wiki/"; // oldest: "https://pathofexile.gamepedia.com/"
    internal const string UrlPoeWikiRu = "https://pathofexile-ru.gamepedia.com/";
    internal const string ApiPoePrice = "https://www.poeprices.info/api?l=";
    internal const string ApiNinjaItem = "https://poe.ninja/api/data/itemoverview?league=";
    internal const string ApiNinjaCur = "https://poe.ninja/api/data/currencyoverview?league=";
    internal const string UrlPoelab = "https://www.poelab.com/";
    internal const string UrlPoedb = "https://poedb.tw/us/mod.php";
    internal const string UrlPoeNinja = "https://poe.ninja/";
    internal const string UrlPaypalDonate = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9TEA8EMSSB846";
    internal const string UrlGithubData = "https://raw.githubusercontent.com/maxensas/xiletrade/master/Xiletrade/Data/";
    
    internal static readonly string[] Culture = ["en-US", "ko-KR", "fr-FR", "es-ES", "de-DE", "pt-BR", "ru-RU", "th-TH", "zh-TW", "zh-CN", "ja-JP"];
    internal static readonly string[] TradeUrl = ["https://www.pathofexile.com/trade/search/", "https://poe.game.daum.net/trade/search/", "https://fr.pathofexile.com/trade/search/", "https://es.pathofexile.com/trade/search/", "https://de.pathofexile.com/trade/search/", "https://br.pathofexile.com/trade/search/", "https://ru.pathofexile.com/trade/search/", "https://th.pathofexile.com/trade/search/", "https://pathofexile.tw/trade/search/", "https://poe.game.qq.com/trade/search/", "https://jp.pathofexile.com/trade/search/"];
    internal static readonly string[] TradeApi = ["https://www.pathofexile.com/api/trade/search/", "https://poe.game.daum.net/api/trade/search/", "https://fr.pathofexile.com/api/trade/search/", "https://es.pathofexile.com/api/trade/search/", "https://de.pathofexile.com/api/trade/search/", "https://br.pathofexile.com/api/trade/search/", "https://ru.pathofexile.com/api/trade/search/", "https://th.pathofexile.com/api/trade/search/", "https://pathofexile.tw/api/trade/search/", "https://poe.game.qq.com/api/trade/search/", "https://jp.pathofexile.com/api/trade/search/"];
    internal static readonly string[] UpdateApi = ["https://www.pathofexile.com/api/trade/data/", "https://poe.game.daum.net/api/trade/data/", "https://fr.pathofexile.com/api/trade/data/", "https://es.pathofexile.com/api/trade/data/", "https://de.pathofexile.com/api/trade/data/", "https://br.pathofexile.com/api/trade/data/", "https://ru.pathofexile.com/api/trade/data/", "https://th.pathofexile.com/api/trade/data/", "https://pathofexile.tw/api/trade/data/", "https://poe.game.qq.com/api/trade/data/", "https://jp.pathofexile.com/api/trade/data/"];
    internal static readonly string[] FetchApi = ["https://www.pathofexile.com/api/trade/fetch/", "https://poe.game.daum.net/api/trade/fetch/", "https://fr.pathofexile.com/api/trade/fetch/", "https://es.pathofexile.com/api/trade/fetch/", "https://de.pathofexile.com/api/trade/fetch/", "https://br.pathofexile.com/api/trade/fetch/", "https://ru.pathofexile.com/api/trade/fetch/", "https://th.pathofexile.com/api/trade/fetch/", "https://pathofexile.tw/api/trade/fetch/", "https://poe.game.qq.com/api/trade/fetch/", "https://jp.pathofexile.com/api/trade/fetch/"];
    internal static readonly string[] ExchangeUrl = ["https://www.pathofexile.com/trade/exchange/", "https://poe.game.daum.net/trade/exchange/", "https://fr.pathofexile.com/trade/exchange/", "https://es.pathofexile.com/trade/exchange/", "https://de.pathofexile.com/trade/exchange/", "https://br.pathofexile.com/trade/exchange/", "https://ru.pathofexile.com/trade/exchange/", "https://th.pathofexile.com/trade/exchange/", "https://pathofexile.tw/trade/exchange/", "https://poe.game.qq.com/trade/exchange/", "https://jp.pathofexile.com/trade/exchange/"];
    internal static readonly string[] ExchangeApi = ["https://www.pathofexile.com/api/trade/exchange/", "https://poe.game.daum.net/api/trade/exchange/", "https://fr.pathofexile.com/api/trade/exchange/", "https://es.pathofexile.com/api/trade/exchange/", "https://de.pathofexile.com/api/trade/exchange/", "https://br.pathofexile.com/api/trade/exchange/", "https://ru.pathofexile.com/api/trade/exchange/", "https://th.pathofexile.com/api/trade/exchange/", "https://pathofexile.tw/api/trade/exchange/", "https://poe.game.qq.com/api/trade/exchange/", "https://jp.pathofexile.com/api/trade/exchange/"];

    internal static class File
    {
        internal const string Config = "Config.json";
        internal const string DefaultConfig = "DefaultConfig.json";
        internal const string Divination = "Divination.json";
        internal const string ParsingRules = "ParsingRules.json";
        internal const string Currency = "Currency.json";
        internal const string Filters = "Filters.json";
        internal const string Leagues = "Leagues.json";
        internal const string Bases = "Bases.json";
        internal const string Mods = "Mods.json";
        internal const string Monsters = "Monsters.json";
        internal const string Words = "Words.json";
        internal const string Gems = "Gems.json";
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
            whoislast, tradechan, globalchan, invite, kick, leave, afk, autoreply, dnd, chat1, chat2, chat3};
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
        internal static readonly List<string> ChatCommands = new() { "remaining", "reset_xp", "ladder", "cls", 
            "deaths", "dance", "kills", "passives", "age", "played", "destroy", "atlaspassives", "togglenochat",
            "guild", "menagerie", "delve", "sanctum", "kingsmarch"};
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
    }

    internal static class ItemLabel
    {
        internal const string Implicit = "(implicit)";
        internal const string Scourge = "(scourge)";
        internal const string Fractured = "(fractured)";
        internal const string Enchant = "(enchant)";
        internal const string Crafted = "(crafted)";
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

    internal static class CurrencyType
    {
        internal const string Cards = "Cards";
        internal const string Currency = "Currency";
        internal const string DelveResonators = "DelveResonators";
        internal const string Fragments = "Fragments";
        internal const string Incubators = "Incubators";
        internal const string Scarabs = "Scarabs";
        internal const string DelveFossils = "DelveFossils";
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
        internal const string MemoryLine = "MemoryLine";
        internal const string Ancestor = "Ancestor";
        internal const string Sanctum = "Sanctum";
        internal const string Crucible = "Crucible";

        internal const string Embers = "Embers";
        internal const string Coffins = "Coffins";
        internal const string Runes = "Runes";
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

        internal const string VeiledPrefix = "veiled.mod_65000"; // Veiled
        internal const string VeiledSuffix = "veiled.mod_63099"; // of the Veil

        internal const string IncAs = "stat_681332047"; // #% increased Attack Speed
        internal const string IncPhys = "stat_1509134228"; // #% increased Physical Damage
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
        internal const string UseRemaining = "stat_1479533453"; // # use remaining : enchant.stat_290368246 / explicit.stat_1479533453
        internal const string PassiveSkill = "stat_3086156145"; // Adds # Passive Skills
        internal const string PassiveJewel = "stat_4079888060"; // # Added Passive Skills are Jewel Sockets
        internal const string GrantNothing = "stat_1085446536"; // Adds # Small Passive Skills which grant nothing
        internal const string Crafted = "stat_1859333175"; // Can have up to 3 Crafted Modifiers
        internal const string LogbookBoss = "stat_3159649981"; // Area contains an Expedition Boss (#)
        internal const string LogbookArea = "stat_1160596338"; // Area contains an additional Underground Area
        internal const string LogbookTwice = "stat_3239978999"; // Excavated Chests have a #% chance to contain twice as many Items

        internal const string IncEs = "stat_4015621042"; // #% increased Energy Shield (Local)
        internal const string IncEva = "stat_124859000"; // #% increased Evasion Rating (Local)
        internal const string IncArmour = "stat_1062208444"; // #% increased Armour (Local)
        internal const string IncAe = "stat_2451402625"; // #% increased Armour and Evasion (Local)
        internal const string IncAes = "stat_3321629045"; // #% increased Armour and Energy Shield (Local)
        internal const string IncEes = "stat_1999113824"; // #% increased Evasion and Energy Shield (Local)
        internal const string IncArEes = "stat_3523867985"; // #% increased Armour, Evasion and Energy Shield (Local)
        internal const string AddArmorFlat = "stat_3484657501"; // # to Armour (Local)
        internal const string AddEsFlat = "stat_4052037485"; // # to maximum Energy Shield (Local)
        internal const string AddEvaFlat = "stat_53045048"; // # to Evasion Rating (Local)

        internal const string AddArmor = "stat_809229260"; // # to Armour
        internal const string AddEs = "stat_3489782002"; // # to maximum Energy Shield
        internal const string AddEva = "stat_2144192055"; // # to Evasion Rating

        internal const string Block = "stat_4253454700"; // #% Chance to Block (Shields)
        internal const string BlockStaffWeapon = "stat_1001829678"; // #% Chance to Block Attack Damage while wielding a Staff (Staves)
        internal const string BlockStaff = "stat_1778298516"; // #% Chance to Block Attack Damage while wielding a Staff

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

        internal const string NecroExplicit = "necropolis.stat_616298078"; // +# Explicit Modifier

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

        internal const string Tablet01 = "pseudo.lake_50846"; // Reflection of Paradise (Difficulty #)
        internal const string Tablet02 = "pseudo.lake_36591"; // Reflection of Kalandra (Difficulty #)
        internal const string Tablet03 = "pseudo.lake_60034"; // Reflection of the Sun (Difficulty #)
        internal const string Tablet04 = "pseudo.lake_40794"; // Reflection of Angling (Difficulty #)

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
        internal const string ActionSpeed = "implicit.stat_2878959938"; // #% reduced Action Speed

        internal const string SocketsUnmodifiable = "explicit.stat_3192592092"; // Sockets cannot be modified

        internal const string TheBlueDream = "explicit.stat_926444104";
        internal const string TheBlueNightmare = "explicit.stat_1224928411";

        internal const string FireTakenOld = "explicit.stat_1029319062"; // #% of Fire Damage from Hits taken as Physical Damage
        internal const string FireTakenNew = "explicit.stat_3205239847"; // #% of Fire Damage from Hits taken as Physical Damage

        internal const string CritFlaskChargeOld = "explicit.stat_2858921304"; // #% chance to gain a Flask Charge when you deal a Critical Strike
        internal const string CritFlaskChargeNew = "explicit.stat_3738001379"; // #% chance to gain a Flask Charge when you deal a Critical Strike

        internal const string Accuracy = "explicit.stat_803737631"; // # to Accuracy Rating
        internal const string AccuracyLocal = "explicit.stat_691932474"; // # to Accuracy Rating (Local)

        internal const string SocketedPierce1 = "explicit.stat_254728692"; // Socketed Gems are Supported by Level # Pierce
        internal const string SocketedPierce2 = "explicit.indexable_support_33"; // Socketed Gems are Supported by Level # Pierce
        //internal const string SocketedPierce3 = "explicit.stat_2433615566"; // Socketed Gems are supported by Level # Pierce

        internal const string CurseVulnerability = "explicit.stat_3967845372"; // Curse Enemies with Vulnerability on Hit
        internal const string CurseVulnerabilityChance = "explicit.stat_2213584313"; // #% chance to Curse Enemies with Vulnerability on Hit

        internal const string ArmorLocal = "explicit.stat_3484657501"; // # to Armour (Local)
        internal const string EsLocal = "explicit.stat_4052037485"; // # to maximum Energy Shield (Local)
        internal const string EvaLocal = "explicit.stat_53045048"; // # to Evasion Rating (Local)

        internal const string Armor = "explicit.stat_809229260"; // # to Armour
        internal const string Es = "explicit.stat_3489782002"; // # to maximum Energy Shield
        internal const string Eva = "explicit.stat_2144192055"; // # to Evasion Rating

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

        internal const string Hatred = "stat_1920370417";
        internal const string Grace = "stat_1803598623";
        internal const string Datermination = "stat_2721871046";
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

        internal static readonly List<string> lSkipOldMods = new()
        {
            Hatred, Grace, Datermination, Pride, Anger, Zealotry, Malevolence, Wrath, Discipline, HeraldIce, HeraldAsh,
            HeraldPurity, HeraldAgony, HeraldThunder, ArcticArmour, PurityFire, PurityLightning, PurityIce
        };
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
    /*
    internal static readonly Dictionary<string, string> lGemQualityProperties = new()
    {
        { "Abyssal Cry", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Added Chaos Damage Support", " 10% increased Chaos Damage (0.5% per 1% Q)" },
        { "Added Cold Damage Support", " 10% increased Cold Damage (0.5% per 1% Q)" },
        { "Added Fire Damage Support", " 10% increased Fire Damage (0.5% per 1% Q)" },
        { "Added Lightning Damage Support", " 10% increased Lightning Damage (0.5% per 1% Q)" },
        { "Additional Accuracy Support", " 20% increased Accuracy Rating (1% per 1% Q)" },
        { "Advanced Traps Support", " 10% increased Trap Damage (0.5% per 1% Q)" },
        { "Ancestral Call Support", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Ancestral Protector", " 20% increased Totem Damage (1% per 1% Q)" },
        { "Ancestral Warchief", " 20% increased Totem Damage (1% per 1% Q)" },
        { "Anger", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Animate Guardian", " Minions have 40% increased Movement Speed (2% per 1% Q)" },
        { "Animate Weapon", " Minions have 40% increased Movement Speed (2% per 1% Q)" },
        { "Arc", " 10% chance to Shock enemies (0.5% per 1% Q)" },
        { "Arcane Surge Support", " 10% increased Spell Damage (0.5% per 1% Q)" },
        { "Arctic Armour", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Arctic Breath", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Armageddon Brand", " 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Assassin's Mark", " Cursed enemies have a 10% chance to grant a Power Charge when slain (0.5% per 1% Q)" },
        { "Ball Lightning", " 20% increased Lightning Damage (1% per 1% Q)" },
        { "Bane", " Applied Curses have 10% increased Effect (0.5% per 1% Q)" },
        { "Barrage", " 10% increased Projectile Damage (0.5% per 1% Q)" },
        { "Bear Trap", " 20% increased Physical Damage (1% per 1% Q)" },
        { "Berserk", " 20% increased Attack Damage (1% per 1% Q)" },
        { "Blade Flurry", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Blade Vortex", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Bladefall", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Bladestorm", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Blasphemy Support", " 10% increased Effect of Supported Curses (0.5% per 1% Q)" },
        { "Blast Rain", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Blastchain Mine Support", " 10% increased Mine Throwing Speed (0.5% per 1% Q)" },
        { "Blight", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Blind Support", " 20% increased Blinding duration (1% per 1% Q)" },
        { "Blink Arrow", " 30% increased Arrow Speed (1.5% per 1% Q)" },
        { "Block Chance Reduction Support", " 5% reduced Enemy Block Chance (0.25% per 1% Q)" },
        { "Blood and Sand", " 10% increased Cooldown Recovery Speed (0.5% per 1% Q)" },
        { "Blood Magic Support", " 10% reduced Mana Cost (0.5% per 1% Q)" },
        { "Blood Rage", " 5% increased Attack Speed (0.25% per 1% Q)" },
        { "Bloodlust Support", " 10% increased Melee Damage against Bleeding Enemies (0.5% per 1% Q)" },
        { "Bodyswap", " 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Bone Offering", " 10% increased Skill Effect Duration (0.5% per 1% Q)" },
        { "Bonechill Support", " 20% increased Chill Duration on Enemies (1% per 1% Q)" },
        { "Brand Recall", " Brands gain 20% increased Attachment Range (1% per 1% Q)" },
        { "Brutality Support", " 10% increased Physical Damage (0.5% per 1% Q)" },
        { "Burning Arrow", " 60% increased Ignite Duration on enemies (3% per 1% Q)" },
        { "Burning Damage Support", " 10% increased Burning Damage (0.5% per 1% Q)" },
        { "Cast On Critical Strike Support", " 20% increased Attack Critical Strike Chance (1% per 1% Q)" },
        { "Cast on Death Support", " 60% increased Area of Effect while Dead (3% per 1% Q)" },
        { "Cast on Melee Kill Support", " 10% increased Attack Damage (0.5% per 1% Q)" },
        { "Cast when Damage Taken Support", " 10% increased Damage (0.5% per 1% Q)" },
        { "Cast when Stunned Support", " 10% increased Damage (0.5% per 1% Q)" },
        { "Cast while Channelling Support", " Supported Channelling Skills deal 10% increased Damage (0.5% per 1% Q)" },
        { "Caustic Arrow", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Chain Hook", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Chain Support", " 20% increased Projectile Speed (1% per 1% Q)" },
        { "Chance to Bleed Support", " Supported Attacks deal 10% increased Damage with Bleeding (0.5% per 1% Q)" },
        { "Chance to Flee Support", " 20% chance to Cause Monsters to Flee (1% per 1% Q)" },
        { "Charged Dash", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Charged Mines Support", " 10% increased Mine Damage (0.5% per 1% Q)" },
        { "Charged Traps Support", " 10% increased Trap Damage (0.5% per 1% Q)" },
        { "Clarity", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Cleave", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Close Combat Support", " 10% increased Melee Damage (0.5% per 1% Q)" },
        { "Cluster Traps Support", " 10% increased Trap Damage (0.5% per 1% Q)" },
        { "Cobra Lash", " 30% increased Critical Strike Chance (1.5% per 1% Q)" },
        { "Cold Penetration Support", " 10% increased Cold Damage (0.5% per 1% Q)" },
        { "Cold Snap", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Cold to Fire Support", " 10% increased Cold Damage 10% increased Fire Damage (0.5% per 1% Q)" },
        { "Combustion Support", " 10% increased Fire Damage (0.5% per 1% Q)" },
        { "Concentrated Effect Support", " 10% increased Area Damage (0.5% per 1% Q)" },
        { "Conductivity", " Shocks on Cursed enemies have 20% increased Duration (1% per 1% Q)" },
        { "Consecrated Path", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Contagion", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Controlled Destruction Support", " 10% increased Spell Damage (0.5% per 1% Q)" },
        { "Conversion Trap", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Convocation", " 20% increased Skill Effect Duration 20% increased Cooldown Recovery Speed (1% per 1% Q)" },
        { "Cremation", " 20% increased Fire Damage (1% per 1% Q)" },
        { "Culling Strike Support", " 10% increased Attack Speed 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Curse On Hit Support", " 10% increased Effect of Supported Curses (0.5% per 1% Q)" },
        { "Cyclone", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Damage on Full Life Support", " 10% increased Damage (0.5% per 1% Q)" },
        { "Dark Pact", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Dash", " 10% increased Cooldown Recovery Speed (0.5% per 1% Q)" },
        { "Deadly Ailments Support", " 10% increased Damage over Time (0.5% per 1% Q)" },
        { "Deathmark Support", " Minions from Supported Skills deal 10% increased Damage (0.5% per 1% Q)" },
        { "Decay Support", " 10% increased Chaos Damage (0.5% per 1% Q)" },
        { "Decoy Totem", " 20% increased totem life (1% per 1% Q)" },
        { "Desecrate", " 20% increased Cast Speed (1% per 1% Q)" },
        { "Despair", " Cursed enemies take 10% increased Damage from Damage Over Time effects (0.5% per 1% Q)" },
        { "Determination", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Detonate Dead", " 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Detonate Mines", " Mines have 20% increased Detonation Speed (1% per 1% Q)" },
        { "Devouring Totem", " 20% increased totem life (1% per 1% Q)" },
        { "Discharge", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Discipline", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Divine Ire", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Dominating Blow", " Minions deal 20% increased Damage (1% per 1% Q)" },
        { "Double Strike", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Dread Banner", " 10% increased effect of Aura (0.5% per 1% Q)" },
        { "Dual Strike", " 20% increased Critical Strike Chance +10% to Critical Strike Multiplier (1% / 0.5% per 1% Q)" },
        { "Earthquake", " 20% increased Damage (1% per 1% Q)" },
        { "Efficacy Support", " 10% increased Damage over Time (0.5% per 1% Q)" },
        { "Elemental Army Support", " Minions from Supported Skills have +2% to all maximum Elemental Resistances (0.1% per 1% Q)" },
        { "Elemental Damage with Attacks Support", " 10% increased Elemental Damage with Attack Skills (0.5% per 1% Q)" },
        { "Elemental Focus Support", " 10% increased Elemental Damage (0.5% per 1% Q)" },
        { "Elemental Hit", " 20% increased Elemental Damage (1% per 1% Q)" },
        { "Elemental Proliferation Support", " 10% increased Duration of Elemental Ailments on Enemies (0.5% per 1% Q)" },
        { "Elemental Weakness", " Cursed enemies have -5% to Elemental Resistances (-0.25% per 1% Q)" },
        { "Empower Support", " This Gem gains 100% increased Experience (5% per 1% Q)" },
        { "Endurance Charge on Melee Stun Support", " 20% increased Stun Duration on Enemies (1% per 1% Q)" },
        { "Enduring Cry", " 60% increased Area of Effect (3% per 1% Q)" },
        { "Energy Leech Support", " 10% increased Damage while Leeching Energy Shield (0.5% per 1% Q)" },
        { "Enfeeble", " Cursed enemies have 4% reduced Accuracy Rating Cursed enemies have 10% reduced Critical Strike Chance (0.2% / 0.5% per 1% Q)" },
        { "Enhance Support", " This Gem gains 100% increased Experience (5% per 1% Q)" },
        { "Enlighten Support", " This Gem gains 100% increased Experience (5% per 1% Q)" },
        { "Essence Drain", " 20% increased Chaos Damage (1% per 1% Q)" },
        { "Ethereal Knives", " 20% increased Projectile Speed (1% per 1% Q)" },
        { "Explosive Arrow", " 20% chance to Ignite enemies (1% per 1% Q)" },
        { "Explosive Trap", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Faster Attacks Support", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Faster Casting Support", " 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Faster Projectiles Support", " 10% increased Attack Speed 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Feeding Frenzy Support", " Feeding Frenzy base Duration is 1 second (0.05 per 1% Q)" },
        { "Fire Penetration Support", " 10% increased Fire Damage (0.5% per 1% Q)" },
        { "Fire Trap", " 20% increased Fire Damage (1% per 1% Q)" },
        { "Fireball", " 10% chance to Ignite enemies (0.5% per 1% Q)" },
        { "Firestorm", " 20% increased Damage (1% per 1% Q)" },
        { "Flame Dash", " 20% increased Cooldown Recovery Speed (1% per 1% Q)" },
        { "Flame Surge", " 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Flameblast", " 20% increased Damage (1% per 1% Q)" },
        { "Flamethrower Trap", " 20% increased Fire Damage (1% per 1% Q)" },
        { "Flammability", " Ignite on Cursed enemies has 10% increased Duration (0.5% per 1% Q)" },
        { "Flesh and Stone", " 10% increased Cooldown Recovery Speed (0.5% per 1% Q)" },
        { "Flesh Offering", " 10% increased Skill Effect Duration (0.5% per 1% Q)" },
        { "Flicker Strike", " 5% chance to gain a Frenzy Charge on Hit (0.25% per 1% Q)" },
        { "Fork Support", " 10% increased Projectile Damage (0.5% per 1% Q)" },
        { "Fortify Support", " 10% increased Attack Damage (0.5% per 1% Q)" },
        { "Freezing Pulse", " 40% increased Projectile Speed (2% per 1% Q)" },
        { "Frenzy", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Frost Blades", " 20% increased Damage 20% increased Projectile Speed (1% per 1% Q)" },
        { "Frost Bomb", " 20% increased Cold Damage (1% per 1% Q)" },
        { "Frost Wall", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Frostbite", " Freezes on Cursed enemies have 20% increased Duration (1% per 1% Q)" },
        { "Frostblink", " 20% increased Effect of Chill (1% per 1% Q)" },
        { "Frostbolt", " 20% increased Cold Damage (1% per 1% Q)" },
        { "Generosity Support", " 40% increased Aura Area of Effect (2% per 1% Q)" },
        { "Glacial Cascade", " 20% increased Damage (1% per 1% Q)" },
        { "Glacial Hammer", " 40% increased Chill Duration on enemies 20% increased Freeze Duration on enemies (2% / 1% per 1% Q)" },
        { "Grace", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Greater Multiple Projectiles Support", " 10% increased Attack Speed 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Ground Slam", " 20% increased Stun Duration on enemies 10% increased Area of Effect (1% / 0.5% per 1% Q)" },
        { "Haste", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Hatred", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Heavy Strike", " 20% increased Stun Duration on enemies 20% increased Damage (1% per 1% Q)" },
        { "Herald of Agony", " 20% increased Minion Movement Speed (1% per 1% Q)" },
        { "Herald of Ash", " 15% increased Fire Damage (0.75% per 1% Q)" },
        { "Herald of Ice", " 15% increased Cold Damage (0.75% per 1% Q)" },
        { "Herald of Purity", " 20% increased Minion Movement Speed (1% per 1% Q)" },
        { "Herald of Thunder", " 15% increased Lightning Damage (0.75% per 1% Q)" },
        { "High-Impact Mine Support", " 10% increased Mine Throwing Speed (0.5% per 1% Q)" },
        { "Holy Flame Totem", " 20% increased Projectile Speed (1% per 1% Q)" },
        { "Hypothermia Support", " 30% increased Chill Duration on Enemies (1.5% per 1% Q)" },
        { "Ice Bite Support", " 20% increased Damage with Hits against Frozen Enemies (1% per 1% Q)" },
        { "Ice Crash", " 20% increased Damage (1% per 1% Q)" },
        { "Ice Nova", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Ice Shot", " 20% increased Cold Damage (1% per 1% Q)" },
        { "Ice Spear", " 40% increased Projectile Speed (2% per 1% Q)" },
        { "Ice Trap", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Icicle Mine", " 20% increased Projectile Damage (1% per 1% Q)" },
        { "Ignite Proliferation Support", " 10% increased Fire Damage (0.5% per 1% Q)" },
        { "Immolate Support", " 10% increased Fire Damage (0.5% per 1% Q)" },
        { "Immortal Call", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Impale Support", " Supported Attacks have 10% increased Impale Effect (0.5% per 1% Q)" },
        { "Incinerate", " 20% increased Fire Damage (1% per 1% Q)" },
        { "Increased Area of Effect Support", " 10% increased Area Damage (0.5% per 1% Q)" },
        { "Increased Critical Damage Support", " +15% to Critical Strike Multiplier (0.75% per 1% Q)" },
        { "Increased Critical Strikes Support", " 20% increased Critical Strike Chance (1% per 1% Q)" },
        { "Increased Duration Support", " 10% increased Skill Effect Duration (0.5% per 1% Q)" },
        { "Infernal Blow", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Infernal Legion Support", " Minions from Supported Skills take 20% reduced Fire Damage (1% per 1% Q)" },
        { "Infused Channelling Support", " Gain Infusion after Channelling a Supported Skill for -0.4 seconds (-0.02 per 1% Q)" },
        { "Innervate Support", " 30% increased Shock Duration on Enemies (1.5% per 1% Q)" },
        { "Inspiration Support", " 20% increased Inspiration Charge Duration (1% per 1% Q)" },
        { "Intensify Support", " 10% increased Area Damage (0.5% per 1% Q)" },
        { "Iron Grip Support", " 10% increased Projectile Damage (0.5% per 1% Q)" },
        { "Iron Will Support", " 10% increased Spell Damage (0.5% per 1% Q)" },
        { "Item Quantity Support", " 7% increased Quantity of Items Dropped by Enemies Slain (0.35% per 1% Q)" },
        { "Item Rarity Support", " 10% increased Rarity of Items Dropped by Enemies Slain (0.5% per 1% Q)" },
        { "Kinetic Blast", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Knockback Support", " 10% chance to Knock Enemies Back on hit (0.5% per 1% Q)" },
        { "Lacerate", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Lancing Steel", " 10% chance to Impale Enemies on Hit (0.5% per 1% Q)" },
        { "Leap Slam", " 20% increased Stun Duration on enemies (1% per 1% Q)" },
        { "Less Duration Support", " 10% reduced Skill Effect Duration (0.5% per 1% Q)" },
        { "Lesser Multiple Projectiles Support", " 10% increased Attack Speed 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Lesser Poison Support", " 10% increased Damage with Poison (0.5% per 1% Q)" },
        { "Life Gain on Hit Support", " +10 Life gained for each Enemy hit by Attacks (0.5 per 1% Q)" },
        { "Life Leech Support", " 10% increased Damage while Leeching Life (0.5% per 1% Q)" },
        { "Lightning Arrow", " 20% chance to Shock enemies (1% per 1% Q)" },
        { "Lightning Penetration Support", " 10% increased Lightning Damage (0.5% per 1% Q)" },
        { "Lightning Spire Trap", " 20% chance to Shock enemies (1% per 1% Q)" },
        { "Lightning Strike", " 20% increased Damage 20% increased Projectile Speed (1% per 1% Q)" },
        { "Lightning Tendrils", " 20% increased Lightning Damage (1% per 1% Q)" },
        { "Lightning Trap", " 20% increased Lightning Damage 10% increased Effect of Shock (1% / 0.5% per 1% Q)" },
        { "Lightning Warp", " 20% increased Cast Speed (1% per 1% Q)" },
        { "Magma Orb", " 20% increased Damage (1% per 1% Q)" },
        { "Maim Support", " 10% increased Physical Damage (0.5% per 1% Q)" },
        { "Malevolence", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Mana Leech Support", " 10% increased Damage while Leeching Mana (0.5% per 1% Q)" },
        { "Meat Shield Support", " Minions from Supported Skills have 5% additional Physical Damage Reduction (0.25% per 1% Q)" },
        { "Melee Physical Damage Support", " 10% increased Melee Physical Damage (0.5% per 1% Q)" },
        { "Melee Splash Support", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Minefield Support", " 10% increased Mine Throwing Speed (0.5% per 1% Q)" },
        { "Minion Damage Support", " Minions from Supported Skills deal 15% increased Damage (0.75% per 1% Q)" },
        { "Minion Life Support", " 15% increased Minion maximum Life (0.75% per 1% Q)" },
        { "Minion Speed Support", " 10% increased Minion Movement Speed (0.5% per 1% Q)" },
        { "Mirage Archer Support", " 10% increased Attack Damage (0.5% per 1% Q)" },
        { "Mirror Arrow", " 30% increased Arrow Speed (1.5% per 1% Q)" },
        { "Molten Shell", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Molten Strike", " 20% increased Fire Damage (1% per 1% Q)" },
        { "Multiple Totems Support", " 20% increased Totem Placement speed (1% per 1% Q)" },
        { "Multiple Traps Support", " 20% increased Trap Trigger Area of Effect (1% per 1% Q)" },
        { "Multistrike Support", " 10% increased Melee Physical Damage (0.5% per 1% Q)" },
        { "Nightblade Support", " 20% increased Critical Strike Chance (1% per 1% Q)" },
        { "Onslaught Support", " 5% increased Attack and Cast Speed (0.25% per 1% Q)" },
        { "Orb of Storms", " 20% increased Lightning Damage (1% per 1% Q)" },
        { "Perforate", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Pestilent Strike", " +10% to Damage over Time Multiplier for Poison (0.5% per 1% Q)" },
        { "Phase Run", " 10% increased Movement Speed (0.5% per 1% Q)" },
        { "Physical to Lightning Support", " 10% increased Physical Damage 10% increased Lightning Damage (0.5% per 1% Q)" },
        { "Pierce Support", " 10% increased Projectile Damage (0.5% per 1% Q)" },
        { "Plague Bearer", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Poacher's Mark", " Cursed enemies have a 10% chance to grant a Frenzy Charge when slain (0.5% per 1% Q)" },
        { "Point Blank Support", " 10% increased Projectile Damage (0.5% per 1% Q)" },
        { "Poison Support", " 10% increased Damage with Poison (0.5% per 1% Q)" },
        { "Portal", " 60% increased Cast Speed (3% per 1% Q)" },
        { "Power Charge On Critical Support", " 20% increased Critical Strike Chance (1% per 1% Q)" },
        { "Power Siphon", " 20% increased Damage (1% per 1% Q)" },
        { "Precision", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Pride", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Projectile Weakness", " Cursed enemies take 10% increased Damage from Projectile Hits (0.5% per 1% Q)" },
        { "Pulverise Support", " 10% increased Area Damage (0.5% per 1% Q)" },
        { "Puncture", " Bleeding inflicted by this Skill deals Damage 10% faster (0.5% per 1% Q)" },
        { "Punishment", " Cursed enemies grant 5% increased Attack Speed on Melee hit (0.25% per 1% Q)" },
        { "Purifying Flame", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Purity of Elements", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Purity of Fire", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Purity of Ice", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Purity of Lightning", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Pyroclast Mine", " 20% increased Area of Effect (1% per 1% Q)" },
        { "Rage Support", " 10% increased Attack Damage (0.5% per 1% Q)" },
        { "Rain of Arrows", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Raise Spectre", " 20% increased Minion Movement Speed (1% per 1% Q)" },
        { "Raise Zombie", " 20% increased Minion Maximum Life 20% increased Minion Movement Speed (1% per 1% Q)" },
        { "Rallying Cry", " 30% increased Skill Effect Duration (1.5% per 1% Q)" },
        { "Ballista Totem Support", " 20% increased Totem Placement speed (1% per 1% Q)" },
        { "Reave", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Reckoning", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Rejuvenation Totem", " 60% increased Aura Area of Effect (3% per 1% Q)" },
        { "Righteous Fire", " 20% increased Burning Damage (1% per 1% Q)" },
        { "Riposte", " 20% increased Damage (1% per 1% Q)" },
        { "Ruthless Support", " 10% increased Attack Damage (0.5% per 1% Q)" },
        { "Scorching Ray", " 10% increased beam length (0.5% per 1% Q)" },
        { "Scourge Arrow", " 20% increased Projectile Speed (1% per 1% Q)" },
        { "Searing Bond", " 20% increased totem life (1% per 1% Q)" },
        { "Seismic Trap", " 20% increased Physical Damage (1% per 1% Q)" },
        { "Shattering Steel", " 20% increased Impale Effect (1% per 1% Q)" },
        { "Shield Charge", " 20% increased Movement Speed (1% per 1% Q)" },
        { "Shock Nova", " 40% increased Shock Duration on enemies (2% per 1% Q)" },
        { "Shockwave Support", " 10% increased Melee Damage (0.5% per 1% Q)" },
        { "Shockwave Totem", " 20% increased totem life (1% per 1% Q)" },
        { "Shrapnel Shot", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Siege Ballista", " 20% increased Totem Placement speed (1% per 1% Q)" },
        { "Siphoning Trap", " 20% increased Effect of Chill (1% per 1% Q)" },
        { "Slower Projectiles Support", " 10% increased Projectile Damage (0.5% per 1% Q)" },
        { "Smite", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Smoke Mine", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Soulrend", " 20% increased Chaos Damage (1% per 1% Q)" },
        { "Spark", " 20% increased Projectile Speed (1% per 1% Q)" },
        { "Spectral Shield Throw", " 40% increased Projectile Speed (2% per 1% Q)" },
        { "Spectral Throw", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Spell Cascade Support", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Spell Echo Support", " 10% increased Spell Damage (0.5% per 1% Q)" },
        { "Spell Totem Support", " 20% increased Totem Placement speed (1% per 1% Q)" },
        { "Spirit Offering", " 10% increased Skill Effect Duration (0.5% per 1% Q)" },
        { "Split Arrow", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Static Strike", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Steelskin", " 10% increased Cooldown Recovery Speed (0.5% per 1% Q)" },
        { "Storm Brand", " 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Storm Burst", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Storm Call", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Stormblast Mine", " 10% chance to Shock enemies 10% increased Effect of Shock (0.5% per 1% Q)" },
        { "Stun Support", " 30% increased Stun Duration on Enemies (1.5% per 1% Q)" },
        { "Summon Carrion Golem", " 20% increased Minion Maximum Life Minions deal 20% increased Damage (1% per 1% Q)" },
        { "Summon Chaos Golem", " 20% increased Minion Maximum Life Minions deal 20% increased Damage (1% per 1% Q)" },
        { "Summon Flame Golem", " 20% increased Minion Maximum Life Minions deal 20% increased Damage (1% per 1% Q)" },
        { "Summon Holy Relic", " Minions have 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Summon Ice Golem", " 20% increased Minion Maximum Life Minions deal 20% increased Damage (1% per 1% Q)" },
        { "Summon Lightning Golem", " 20% increased Minion Maximum Life Minions deal 20% increased Damage (1% per 1% Q)" },
        { "Summon Phantasm Support", " 10% chance to Summon a Phantasm when Supported Skills, or Non-Phantasm Minions from Supported Skills, deal a Killing Blow (0.5% per 1% Q)" },
        { "Summon Raging Spirit", " 20% increased Minion Movement Speed (1% per 1% Q)" },
        { "Summon Skeletons", " Minions deal 20% increased Damage (1% per 1% Q)" },
        { "Summon Skitterbots", " 40% increased Minion Movement Speed (2% per 1% Q)" },
        { "Summon Stone Golem", " 20% increased Minion Maximum Life Minions deal 20% increased Damage (1% per 1% Q)" },
        { "Sunder", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Sweep", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Swift Affliction Support", " 10% increased Damage over Time (0.5% per 1% Q)" },
        { "Swift Assembly Support", " 10% increased Mine Throwing Speed 10% increased Trap Throwing Speed (0.5% per 1% Q)" },
        { "Tectonic Slam", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Tempest Shield", " 20% increased Lightning Damage (1% per 1% Q)" },
        { "Temporal Chains", " Cursed Normal and Magic Enemies have 10% less Action Speed (0.5% per 1% Q)" },
        { "Tornado Shot", " 20% increased Projectile Damage (1% per 1% Q)" },
        { "Toxic Rain", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Trap and Mine Damage Support", " 10% increased Damage (0.5% per 1% Q)" },
        { "Trap Support", " 10% increased Trap Throwing Speed (0.5% per 1% Q)" },
        { "Unbound Ailments Support", " 10% increased Duration of Ailments on Enemies (0.5% per 1% Q)" },
        { "Unearth", " 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Unleash Support", " 10% increased Spell Damage (0.5% per 1% Q)" },
        { "Vaal Ancestral Warchief", " 20% increased Totem Damage (1% per 1% Q)" },
        { "Vaal Arc", " 30% increased Shock Duration on enemies (1.5% per 1% Q)" },
        { "Vaal Blade Vortex", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Vaal Blight", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Vaal Breach", " 60% increased Cast Speed (3% per 1% Q)" },
        { "Vaal Burning Arrow", " 60% increased Ignite Duration on enemies (3% per 1% Q)" },
        { "Vaal Clarity", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Vaal Cold Snap", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Vaal Cyclone", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Vaal Detonate Dead", " 20% increased Area Damage (1% per 1% Q)" },
        { "Vaal Discipline", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Vaal Double Strike", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Vaal Earthquake", " 20% increased Damage (1% per 1% Q)" },
        { "Vaal Fireball", " 10% chance to Ignite enemies (0.5% per 1% Q)" },
        { "Vaal Flameblast", " 20% increased Damage (1% per 1% Q)" },
        { "Vaal Glacial Hammer", " 40% increased Chill Duration on enemies 20% increased Freeze Duration on enemies (2% / 1% per 1% Q)" },
        { "Vaal Grace", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Vaal Ground Slam", " 20% increased Stun Duration on enemies 10% increased Area of Effect (1% / 0.5% per 1% Q)" },
        { "Vaal Haste", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Vaal Ice Nova", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Vaal Immortal Call", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Vaal Impurity of Fire", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Vaal Impurity of Ice", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Vaal Impurity of Lightning", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Vaal Lightning Strike", " 20% increased Damage 20% increased Projectile Speed (1% per 1% Q)" },
        { "Vaal Lightning Trap", " Shocked Ground causes 5% increased Damage taken (0.25% per 1% Q)" },
        { "Vaal Lightning Warp", " 20% increased Cast Speed (1% per 1% Q)" },
        { "Vaal Molten Shell", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Vaal Power Siphon", " 20% increased Damage (1% per 1% Q)" },
        { "Vaal Rain of Arrows", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Vaal Reave", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Vaal Righteous Fire", " 20% increased Burning Damage (1% per 1% Q)" },
        { "Vaal Spark", " 20% increased Projectile Speed (1% per 1% Q)" },
        { "Vaal Spectral Throw", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Vaal Storm Call", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Vaal Summon Skeletons", " Minions deal 20% increased Damage (1% per 1% Q)" },
        { "Vengeance", " 10% chance to Trigger this Skill when Hit (0.5% per 1% Q)" },
        { "Venom Gyre", " 20% increased Poison Duration (1% per 1% Q)" },
        { "Vicious Projectiles Support", " 10% increased Physical Damage (0.5% per 1% Q)" },
        { "Vigilant Strike", " 40% increased Fortify duration (2% per 1% Q)" },
        { "Vile Toxins Support", " 20% increased Damage with Poison (1% per 1% Q)" },
        { "Viper Strike", " 10% increased Attack Speed 10% increased Poison Duration (0.5% per 1% Q)" },
        { "Vitality", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Void Manipulation Support", " 10% increased Chaos Damage (0.5% per 1% Q)" },
        { "Volatile Dead", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Volley Support", " 20% increased Projectile Damage (1% per 1% Q)" },
        { "Vortex", " 10% increased Area of Effect (0.5% per 1% Q)" },
        { "Vulnerability", " Cursed Enemies have 10% chance to Bleed when Hit by Attacks (0.5% per 1% Q)" },
        { "War Banner", " 10% increased effect of Aura (0.5% per 1% Q)" },
        { "Warlord's Mark", " Cursed enemies have a 10% chance to grant an Endurance Charge when slain (0.5% per 1% Q)" },
        { "Wave of Conviction", " 20% increased Elemental Damage (1% per 1% Q)" },
        { "Whirling Blades", " 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Wild Strike", " 20% increased Elemental Damage (1% per 1% Q)" },
        { "Winter Orb", " 10% increased Cast Speed (0.5% per 1% Q)" },
        { "Wither", " 20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Withering Step", " 10% increased Effect of Elusive from this Skill (0.5% per 1% Q)" },
        { "Withering Touch Support", " 10% increased Chaos Damage (0.5% per 1% Q)" },
        { "Wrath", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Zealotry", " 40% increased Area of Effect (2% per 1% Q)" },
        { "Barrage Support", "Supported Skills have 10% increased Attack Speed (0.5% per 1% Q)" },
        { "Awakened Added Chaos Damage Support", "Supported Skills deal 10% increased Chaos Damage (0.5% per 1% Q)" }, { "Awakened Added Cold Damage Support", "Supported Skills deal 10% increased Cold Damage (0.5% per 1% Q)" }, { "Awakened Added Fire Damage Support", "Supported Skills deal 20% increased Fire Damage (1% per 1% Q)" }, { "Awakened Added Lightning Damage Support", "Supported Skills deal 10% increased Lightning Damage (0.5% per 1% Q)" },
        { "Awakened Ancestral Call Support", "Supported Skills have 10% increased Attack Speed Extra Targets for Supported Skills can be found 20% further away (0.5% & 1% per 1% Q)" },
        { "Awakened Arrow Nova Support", "Supported Skills have 20% increased Attack Speed (1% per 1% Q)" }, { "Awakened Blasphemy Support", "10% increased Effect of Supported Curses \n+1 to Level of Supported Curse Skill Gems (0.5% & 0.05 per 1% Q)" }, { "Awakened Brutality Support", "Supported Skills deal 10% increased Physical Damage Enemies have -10% to Total Physical Damage Reduction against your Hits (0.5% & 0.5% per 1% Q)" }, { "Awakened Cold Penetration Support", "Supported Skills deal 20% increased Cold Damage (1% per 1% Q)" },
        { "Awakened Burning Damage Support", "Supported Skills deal 10% increased Burning Damage, +10% to Fire Damage over Time Multiplier (0.5% & 0.5% per 1% Q)" }, { "Awakened Cast On Critical Strike Support", "Supported Skills have 20% increased Attack Critical Strike Chance\nSupported Skills have 20% increased Spell Critical Strike Chance (1% per 1% Q)" }, { "Awakened Cast While Channelling Support", "Supported Channelling Skills deal 10% increased Damage\nSupported Triggered Spells deal 10% increased Damage (0.5% per 1% Q)" }, { "Awakened Chain Support", "Supported Skills have 20% increased Projectile Speed\n10% increased Attack and Cast Speed (1% & 0.5% per 1% Q)" },
        { "Awakened Controlled Destruction Support", "Supported Skills deal 10% increased Spell Damage\nSupported Skills have 10% increased Cast Speed (0.5% per 1% Q)" }, { "Awakened Curse On Hit Support", "10% increased Effect of Supported Curses\nSupported Curse Skills have 10% chance to apply to Hexproof Enemies (0.5% per 1% Q)" }, { "Awakened Deadly Ailments Support", "Supported Skills deal 10% increased Damage over Time (0.5% per 1% Q)" }, { "Awakened Elemental Damage With Attacks Support", "20% increased Elemental Damage with Attack Skills (1% per 1% Q)" },
        { "Awakened Elemental Focus Support", "Supported Skills deal 20% increased Elemental Damage (1% per 1% Q)" }, { "Awakened Fire Penetration Support", "Supported Skills deal 20% increased Fire Damage (1% per 1% Q)" }, { "Awakened Fork Support", "Supported Skills deal 20% increased Projectile Damage (1% per 1% Q)" }, { "Awakened Generosity Support", "Supported Skills have 40% increased Aura Area of Effect\n+1 to Level of Supported Aura Skill Gems (2% & 0.05 per 1% Q)" },
        { "Awakened Greater Multiple Projectiles Support", "Supported Skills have 20% increased Attack Speed\nSupported Skills have 20% increased Cast Speed (1% per 1% Q)" }, { "Awakened Increased Area Of Effect Support", "Supported Skills deal 10% increased Area Damage (0.5% per 1% Q)" }, { "Awakened Lightning Penetration Support", "Supported Skills deal 20% increased Lightning Damage (1% per 1% Q)" }, { "Awakened Melee Physical Damage Support", "Supported Skills deal 10% increased Melee Physical Damage\n10% chance to Intimidate Enemies for 4 seconds on Hit (0.5% per 1% Q)" },
        { "Awakened Melee Splash Support", "Supported Skills have 20% increased Area of Effect (1% per 1% Q)" }, { "Awakened Minion Damage Support", "Minions from Supported Skills deal 20% increased Damage (1% per 1% Q)" }, { "Awakened Multistrike Support", "Supported Skills deal 10% increased Melee Physical Damage\nSupported Skills have 10% increased Attack Speed (0.5% per 1% Q)" }, { "Awakened Spell Cascade Support", "Supported Skills have 10% increased Area of Effect\nSupported Skills deal 10% increased Spell Damage (0.5% per 1% Q)" },
        { "Awakened Spell Echo Support", "Supported Skills deal 10% increased Spell Damage\nSupported Skills have 10% increased Cast Speed (0.5% per 1% Q)" }, { "Awakened Swift Affliction Support", "Supported Skills deal 10% increased Damage over Time \n+10% to Damage over Time Multiplier (0.5% per 1% Q)" }, { "Awakened Unbound Ailments Support", "Supported Skills have 10% increased Duration of Ailments on Enemies, +10% to Damage over Time Multiplier (0.5% per 1% Q)" }, { "Awakened Unleash Support", "Supported Skills deal 20% increased Spell Damage (1% per 1% Q)" },
        { "Awakened Vicious Projectiles Support", "Supported Skills deal 20% increased Physical Damage (1% per 1% Q)" }, { "Awakened Void Manipulation Support", "Supported Skills deal 20% increased Chaos Damage (1% per 1% Q)" },
        { "Arcane Cloak", "20% increased Skill Effect Duration (1% per 1% Q)" },
        { "Stormbind", "10% increased Cast Speed (0.5% per 1% Q)" },
        { "Kinetic Bolt", "20% increased Projectile Speed (1% per 1% Q)" },
        { "Blade Blast", "10% increased Area of Effect (0.5% per 1% Q)" },
        { "Archmage Support", "Supported Skills have 10% increased Mana Cost (0.5% per 1% Q)" },
        { "Second Wind Support", "Supported Skills have 5% increased Cooldown Recovery Speed (0.25% per 1% Q)" },
        { "Urgent Orders Support", "Supported Skills have (0–10)% increased Warcry Speed" },
        { "Swiftbrand Support", "Supported Skills have (0–5)% increased Activation frequency" },
        { "General's Cry", "(0–10)% increased Cooldown Recovery Rate" },
        { "Earthshatter", "(0–10)% increased Area of Effect" },
        { "Wintertide Brand", "+(0–10)% to Cold Damage over Time Multiplier" },
        { "Penance Brand", "(0–10)% increased Area of Effect" },
        { "Ancestral Cry", "(0–10)% increased Cooldown Recovery Rate" },
        { "Intimidating Cry", "(0–10)% increased Cooldown Recovery Rate" },
        { "Arcanist Brand", "(0–10)% increased Activation frequency" },
        { "Fist of War Support", "Supported Skills have (0–10)% increased Area of Effect" },
        { "Seismic Cry", "(0–10)% increased Cooldown Recovery Rate" },
        { "Pinpoint Support", "Supported Skills deal (0–10)% increased Projectile Damage" },
        { "Hexblast", "(0–10)% increased Area of Effect" },
        { "Impending Doom Support", "Curse Skills have (0–40)% reduced Skill Effect Duration" },
        { "Flame Wall", "(0–10)% increased Area of Effect" },
        { "Sigil of Power", "(0–10)% increased Area of Effect" },
        { "Splitting Steel", "(0–20)% increased Impale Effect" },
        { "Frost Shield", "(0–20)% increased Effect of Cold Ailments" },
        { "Crackling Lance", "(0–10)% chance to Shock enemies" },
        { "Void Sphere", "(0–10)% increased Area of Effect" },
        { "Blazing Salvo", "(0–10)% increased Area of Effect" },
        { "Elemental Penetration Support", "Supported Skills deal (0–10)% increased Elemental Damage" }
    };
    */

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
    /*
    internal static Dictionary<string, string> lShards = new Dictionary<string, string>()
    {
        { "annulment-shard", "Annulment Shard" }, { "mirror-shard", "Mirror Shard" }, { "exalted-shard", "Exalted Shard" },
        { "binding-shard", "Binding Shard" }, { "horizon-shard", "Horizon Shard" }, { "harbingers-shard", "Harbinger's Shard" },
        { "engineers-shard", "Engineer's Shard" }, { "ancient-shard", "Ancient Shard" }, { "chaos-shard", "Chaos Shard" },
        { "regal-shard", "Regal Shard" }, { "splinter-xoph", "Splinter of Xoph" }, { "splinter-tul", "Splinter of Tul" },
        { "splinter-esh", "Splinter of Esh" }, { "splinter-uul", "Splinter of Uul-Netol" }, { "splinter-chayula", "Splinter of Chayula" },
        { "timeless-karui-splinter", "Timeless Karui Splinter" }, { "timeless-maraketh-splinter", "Timeless Maraketh Splinter" }, { "timeless-eternal-empire-splinter", "Timeless Eternal Empire Splinter" },
        { "timeless-templar-splinter", "Timeless Templar Splinter" }, { "timeless-vaal-splinter", "Timeless Vaal Splinter" }, { "simulacrum-splinter", "Simulacrum Splinter" },
        { "crescent-splinter", "Crescent Splinter" }, { "ritual-splinter", "Ritual Splinter" }
    };
    */
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
    /* Others currency
     { "p", "Perandus Coin" }, { "portal", "Portal Scroll" }, { "scrap", "Armourer's Scrap" }, { "eternal", "Eternal Orb" }, { "facetors", "Facetor's Lens" },
    { "orb-of-binding", "Orb of Binding" }, { "rogues-marker", "Rogue's Marker" }, { "silver", "Silver Coin" }, { "stacked-deck", "Stacked Deck" },
    { "whetstone", "Blacksmith's Whetstone" }, { "wisdom", "Scroll of Wisdom" },
    */

    internal static readonly Dictionary<string, string> dicPublicID = new()
    {
        { "Driftwood Sceptre", "Sceptres" }, { "Darkwood Sceptre", "Sceptres" }, { "Bronze Sceptre", "Sceptres" }, { "Quartz Sceptre", "Sceptres" }, { "Iron Sceptre", "Sceptres" },
        { "Ochre Sceptre", "Sceptres" }, { "Ritual Sceptre", "Sceptres" }, { "Shadow Sceptre", "Sceptres" }, { "Grinning Fetish", "Sceptres" }, { "Horned Sceptre", "Sceptres" },
        { "Sekhem", "Sceptres" }, { "Crystal Sceptre", "Sceptres" }, { "Lead Sceptre", "Sceptres" }, { "Blood Sceptre", "Sceptres" }, { "Royal Sceptre", "Sceptres" },
        { "Abyssal Sceptre", "Sceptres" }, { "Stag Sceptre", "Sceptres" }, { "Karui Sceptre", "Sceptres" }, { "Tyrant's Sekhem", "Sceptres" }, { "Opal Sceptre", "Sceptres" },
        { "Platinum Sceptre", "Sceptres" }, { "Vaal Sceptre", "Sceptres" }, { "Carnal Sceptre", "Sceptres" }, { "Void Sceptre", "Sceptres" }, { "Sambar Sceptre", "Sceptres" },
        { "Alternating Sceptre", "Sceptres" }, { "Stabilising Sceptre", "Sceptres" }, { "Oscillating Sceptre", "Sceptres" },

        { "Driftwood Club", "One Hand Maces" }, { "Tribal Club", "One Hand Maces" }, { "Spiked Club", "One Hand Maces" }, { "Stone Hammer", "One Hand Maces" },
        { "War Hammer", "One Hand Maces" }, { "Bladed Mace", "One Hand Maces" }, { "Ceremonial Mace", "One Hand Maces" }, { "Dream Mace", "One Hand Maces" },
        { "Wyrm Mace", "One Hand Maces" }, { "Petrified Club", "One Hand Maces" }, { "Barbed Club", "One Hand Maces" }, { "Battle Hammer", "One Hand Maces" },
        { "Flanged Mace", "One Hand Maces" }, { "Ornate Mace", "One Hand Maces" }, { "Phantom Mace", "One Hand Maces" }, { "Dragon Mace", "One Hand Maces" },
        { "Ancestral Club", "One Hand Maces" }, { "Tenderizer", "One Hand Maces" }, { "Gavel", "One Hand Maces" }, { "Legion Hammer", "One Hand Maces" },
        { "Pernarch", "One Hand Maces" }, { "Auric Mace", "One Hand Maces" }, { "Nightmare Mace", "One Hand Maces" }, { "Behemoth Mace", "One Hand Maces" },
        { "Boom Mace", "One Hand Maces" }, { "Crack Mace", "One Hand Maces" }, { "Flare Mace", "One Hand Maces" },

        { "Driftwood Maul", "Two Hand Maces" }, { "Tribal Maul", "Two Hand Maces" }, { "Mallet", "Two Hand Maces" }, { "Sledgehammer", "Two Hand Maces" },
        { "Jagged Maul", "Two Hand Maces" }, { "Brass Maul", "Two Hand Maces" }, { "Fright Maul", "Two Hand Maces" }, { "Morning Star", "Two Hand Maces" },
        { "Totemic Maul", "Two Hand Maces" }, { "Great Mallet", "Two Hand Maces" }, { "Steelhead", "Two Hand Maces" }, { "Spiny Maul", "Two Hand Maces" },
        { "Plated Maul", "Two Hand Maces" }, { "Dread Maul", "Two Hand Maces" }, { "Solar Maul", "Two Hand Maces" }, { "Karui Maul", "Two Hand Maces" },
        { "Colossus Mallet", "Two Hand Maces" }, { "Piledriver", "Two Hand Maces" }, { "Meatgrinder", "Two Hand Maces" }, { "Imperial Maul", "Two Hand Maces" },
        { "Terror Maul", "Two Hand Maces" }, { "Coronal Maul", "Two Hand Maces" }, { "Impact Force Propagator", "Two Hand Maces" }, { "Crushing Force Magnifier", "Two Hand Maces" },
        { "Blunt Force Condenser", "Two Hand Maces" },

        { "Charan's Sword", "One Hand Swords" }, { "Rusted Sword", "One Hand Swords" }, { "Copper Sword", "One Hand Swords" }, { "Sabre", "One Hand Swords" },
        { "Broad Sword", "One Hand Swords" }, { "War Sword", "One Hand Swords" }, { "Ancient Sword", "One Hand Swords" }, { "Elegant Sword", "One Hand Swords" },
        { "Dusk Blade", "One Hand Swords" }, { "Hook Sword", "One Hand Swords" }, { "Variscite Blade", "One Hand Swords" }, { "Cutlass", "One Hand Swords" },
        { "Baselard", "One Hand Swords" }, { "Battle Sword", "One Hand Swords" }, { "Elder Sword", "One Hand Swords" }, { "Graceful Sword", "One Hand Swords" },
        { "Twilight Blade", "One Hand Swords" }, { "Grappler", "One Hand Swords" }, { "Gemstone Sword", "One Hand Swords" }, { "Corsair Sword", "One Hand Swords" },
        { "Gladius", "One Hand Swords" }, { "Legion Sword", "One Hand Swords" }, { "Vaal Blade", "One Hand Swords" }, { "Eternal Sword", "One Hand Swords" },
        { "Midnight Blade", "One Hand Swords" }, { "Tiger Hook", "One Hand Swords" }, { "Anarchic Spiritblade", "One Hand Swords" }, { "Capricious Spiritblade", "One Hand Swords" },
        { "Fickle Spiritblade", "One Hand Swords" },

        { "Corroded Blade", "Two Hand Swords" }, { "Longsword", "Two Hand Swords" }, { "Bastard Sword", "Two Hand Swords" }, { "Two-Handed Sword", "Two Hand Swords" },
        { "Etched Greatsword", "Two Hand Swords" }, { "Ornate Sword", "Two Hand Swords" }, { "Spectral Sword", "Two Hand Swords" }, { "Curved Blade", "Two Hand Swords" },
        { "Butcher Sword", "Two Hand Swords" }, { "Footman Sword", "Two Hand Swords" }, { "Highland Blade", "Two Hand Swords" }, { "Engraved Greatsword", "Two Hand Swords" },
        { "Tiger Sword", "Two Hand Swords" }, { "Wraith Sword", "Two Hand Swords" }, { "Lithe Blade", "Two Hand Swords" }, { "Headman's Sword", "Two Hand Swords" },
        { "Reaver Sword", "Two Hand Swords" }, { "Ezomyte Blade", "Two Hand Swords" }, { "Vaal Greatsword", "Two Hand Swords" }, { "Lion Sword", "Two Hand Swords" },
        { "Infernal Sword", "Two Hand Swords" }, { "Exquisite Blade", "Two Hand Swords" }, { "Banishing Blade", "Two Hand Swords" }, { "Blasting Blade", "Two Hand Swords" },
        { "Rebuking Blade", "Two Hand Swords" },

        { "Rusted Spike", "Thrusting One Hand Swords" }, { "Whalebone Rapier", "Thrusting One Hand Swords" }, { "Battered Foil", "Thrusting One Hand Swords" },
        { "Basket Rapier", "Thrusting One Hand Swords" }, { "Jagged Foil", "Thrusting One Hand Swords" }, { "Antique Rapier", "Thrusting One Hand Swords" },
        { "Elegant Foil", "Thrusting One Hand Swords" }, { "Thorn Rapier", "Thrusting One Hand Swords" }, { "Smallsword", "Thrusting One Hand Swords" },
        { "Wyrmbone Rapier", "Thrusting One Hand Swords" }, { "Burnished Foil", "Thrusting One Hand Swords" }, { "Estoc", "Thrusting One Hand Swords" },
        { "Serrated Foil", "Thrusting One Hand Swords" }, { "Primeval Rapier", "Thrusting One Hand Swords" }, { "Fancy Foil", "Thrusting One Hand Swords" },
        { "Apex Rapier", "Thrusting One Hand Swords" }, { "Courtesan Sword", "Thrusting One Hand Swords" }, { "Dragonbone Rapier", "Thrusting One Hand Swords" },
        { "Tempered Foil", "Thrusting One Hand Swords" }, { "Pecoraro", "Thrusting One Hand Swords" }, { "Spiraled Foil", "Thrusting One Hand Swords" },
        { "Vaal Rapier", "Thrusting One Hand Swords" }, { "Jewelled Foil", "Thrusting One Hand Swords" }, { "Harpy Rapier", "Thrusting One Hand Swords" },
        { "Dragoon Sword", "Thrusting One Hand Swords" },

        { "Nailed Fist", "Claws" }, { "Sharktooth Claw", "Claws" }, { "Awl", "Claws" }, { "Cat's Paw", "Claws" }, { "Blinder", "Claws" }, { "Timeworn Claw", "Claws" },
        { "Sparkling Claw", "Claws" }, { "Fright Claw", "Claws" }, { "Double Claw", "Claws" }, { "Thresher Claw", "Claws" }, { "Gouger", "Claws" }, { "Tiger's Paw", "Claws" },
        { "Gut Ripper", "Claws" }, { "Prehistoric Claw", "Claws" }, { "Noble Claw", "Claws" }, { "Eagle Claw", "Claws" }, { "Twin Claw", "Claws" }, { "Great White Claw", "Claws" },
        { "Throat Stabber", "Claws" }, { "Hellion's Paw", "Claws" }, { "Eye Gouger", "Claws" }, { "Vaal Claw", "Claws" }, { "Imperial Claw", "Claws" }, { "Terror Claw", "Claws" },
        { "Gemini Claw", "Claws" }, { "Void Fangs", "Claws" }, { "Malign Fangs", "Claws" }, { "Shadow Fangs", "Claws" },

        { "Glass Shank", "Daggers" }, { "Skinning Knife", "Daggers" }, { "Stiletto", "Daggers" }, { "Prong Dagger", "Daggers" }, { "Flaying Knife", "Daggers" },
        { "Poignard", "Daggers" }, { "Trisula", "Daggers" }, { "Gutting Knife", "Daggers" }, { "Ambusher", "Daggers" }, { "Sai", "Daggers" }, { "Ethereal Blade", "Daggers" },
        { "Pneumatic Dagger", "Daggers" }, { "Pressurised Dagger", "Daggers" }, { "Hollowpoint Dagger", "Daggers" },

        { "Carving Knife", "Rune Daggers" }, { "Boot Knife", "Rune Daggers" }, { "Copper Kris", "Rune Daggers" }, { "Skean", "Rune Daggers" }, { "Imp Dagger", "Rune Daggers" },
        { "Butcher Knife", "Rune Daggers" }, { "Boot Blade", "Rune Daggers" }, { "Golden Kris", "Rune Daggers" }, { "Royal Skean", "Rune Daggers" },
        { "Fiend Dagger", "Rune Daggers" }, { "Slaughter Knife", "Rune Daggers" }, { "Ezomyte Dagger", "Rune Daggers" }, { "Platinum Kris", "Rune Daggers" },
        { "Imperial Skean", "Rune Daggers" }, { "Demon Dagger", "Rune Daggers" }, { "Infernal Blade", "Rune Daggers" }, { "Flashfire Blade", "Rune Daggers" }, { "Flickerflame Blade", "Rune Daggers" },

        { "Driftwood Wand", "Wands" }, { "Goat's Horn", "Wands" }, { "Carved Wand", "Wands" }, { "Quartz Wand", "Wands" }, { "Spiraled Wand", "Wands" },
        { "Sage Wand", "Wands" }, { "Pagan Wand", "Wands" }, { "Faun's Horn", "Wands" }, { "Engraved Wand", "Wands" }, { "Crystal Wand", "Wands" },
        { "Serpent Wand", "Wands" }, { "Omen Wand", "Wands" }, { "Heathen Wand", "Wands" }, { "Demon's Horn", "Wands" }, { "Imbued Wand", "Wands" },
        { "Opal Wand", "Wands" }, { "Tornado Wand", "Wands" }, { "Prophecy Wand", "Wands" }, { "Profane Wand", "Wands" }, { "Convoking Wand", "Wands" },
        { "Accumulator Wand", "Wands" }, { "Congregator Wand", "Wands" }, { "Assembler Wand", "Wands" },

        { "Rusted Hatchet", "One Hand Axes" }, { "Jade Hatchet", "One Hand Axes" }, { "Boarding Axe", "One Hand Axes" }, { "Cleaver", "One Hand Axes" },
        { "Broad Axe", "One Hand Axes" }, { "Arming Axe", "One Hand Axes" }, { "Decorative Axe", "One Hand Axes" }, { "Spectral Axe", "One Hand Axes" },
        { "Etched Hatchet", "One Hand Axes" }, { "Jasper Axe", "One Hand Axes" }, { "Tomahawk", "One Hand Axes" }, { "Wrist Chopper", "One Hand Axes" },
        { "War Axe", "One Hand Axes" }, { "Chest Splitter", "One Hand Axes" }, { "Ceremonial Axe", "One Hand Axes" }, { "Wraith Axe", "One Hand Axes" },
        { "Engraved Hatchet", "One Hand Axes" }, { "Karui Axe", "One Hand Axes" }, { "Siege Axe", "One Hand Axes" }, { "Reaver Axe", "One Hand Axes" },
        { "Butcher Axe", "One Hand Axes" }, { "Vaal Hatchet", "One Hand Axes" }, { "Royal Axe", "One Hand Axes" }, { "Infernal Axe", "One Hand Axes" },
        { "Runic Hatchet", "One Hand Axes" }, { "Psychotic Axe", "One Hand Axes" }, { "Disapprobation Axe", "One Hand Axes" }, { "Maltreatment Axe", "One Hand Axes" },

        { "Stone Axe", "Two Hand Axes" }, { "Jade Chopper", "Two Hand Axes" }, { "Woodsplitter", "Two Hand Axes" }, { "Poleaxe", "Two Hand Axes" },
        { "Double Axe", "Two Hand Axes" }, { "Gilded Axe", "Two Hand Axes" }, { "Shadow Axe", "Two Hand Axes" }, { "Dagger Axe", "Two Hand Axes" },
        { "Jasper Chopper", "Two Hand Axes" }, { "Timber Axe", "Two Hand Axes" }, { "Headsman Axe", "Two Hand Axes" }, { "Labrys", "Two Hand Axes" },
        { "Noble Axe", "Two Hand Axes" }, { "Abyssal Axe", "Two Hand Axes" }, { "Talon Axe", "Two Hand Axes" }, { "Karui Chopper", "Two Hand Axes" },
        { "Sundering Axe", "Two Hand Axes" }, { "Ezomyte Axe", "Two Hand Axes" }, { "Vaal Axe", "Two Hand Axes" }, { "Despot Axe", "Two Hand Axes" },
        { "Void Axe", "Two Hand Axes" }, { "Fleshripper", "Two Hand Axes" }, { "Apex Cleaver", "Two Hand Axes" }, { "Honed Cleaver", "Two Hand Axes" },
        { "Prime Cleaver", "Two Hand Axes" },

        { "Gnarled Branch", "Staves" }, { "Primitive Staff", "Staves" }, { "Long Staff", "Staves" }, { "Royal Staff", "Staves" }, { "Crescent Staff", "Staves" },
        { "Woodful Staff", "Staves" }, { "Quarterstaff", "Staves" }, { "Highborn Staff", "Staves" }, { "Moon Staff", "Staves" }, { "Primordial Staff", "Staves" },
        { "Lathi", "Staves" }, { "Imperial Staff", "Staves" }, { "Eclipse Staff", "Staves" }, { "Battery Staff", "Staves" }, { "Reciprocation Staff", "Staves" },
        { "Transformer Staff", "Staves" },

        { "Iron Staff", "Warstaves" }, { "Coiled Staff", "Warstaves" }, { "Vile Staff", "Warstaves" }, { "Military Staff", "Warstaves" }, { "Serpentine Staff", "Warstaves" },
        { "Foul Staff", "Warstaves" }, { "Ezomyte Staff", "Warstaves" }, { "Maelström Staff", "Warstaves" }, { "Judgement Staff", "Warstaves" },
        { "Eventuality Rod", "Warstaves" }, { "Potentiality Rod", "Warstaves" }, { "Capacity Rod", "Warstaves" },

        { "Crude Bow", "Bows" }, { "Short Bow", "Bows" }, { "Long Bow", "Bows" }, { "Composite Bow", "Bows" }, { "Recurve Bow", "Bows" }, { "Bone Bow", "Bows" },
        { "Royal Bow", "Bows" }, { "Death Bow", "Bows" }, { "Reflex Bow", "Bows" }, { "Grove Bow", "Bows" }, { "Decurve Bow", "Bows" }, { "Compound Bow", "Bows" },
        { "Sniper Bow", "Bows" }, { "Ivory Bow", "Bows" }, { "Highborn Bow", "Bows" }, { "Decimation Bow", "Bows" }, { "Steelwood Bow", "Bows" }, { "Thicket Bow", "Bows" },
        { "Citadel Bow", "Bows" }, { "Ranger Bow", "Bows" }, { "Assassin Bow", "Bows" }, { "Spine Bow", "Bows" }, { "Imperial Bow", "Bows" }, { "Harbinger Bow", "Bows" },
        { "Maraketh Bow", "Bows" }, { "Solarine Bow", "Bows" }, { "Foundry Bow", "Bows" }, { "Hedron Bow", "Bows" },

        { "Cured Quiver", "Quivers" }, { "Rugged Quiver", "Quivers" }, { "Conductive Quiver", "Quivers" }, { "Heavy Quiver", "Quivers" }, { "Light Quiver", "Quivers" },
        { "Serrated Arrow Quiver", "Quivers" }, { "Two-Point Arrow Quiver", "Quivers" }, { "Sharktooth Arrow Quiver", "Quivers" }, { "Blunt Arrow Quiver", "Quivers" },
        { "Fire Arrow Quiver", "Quivers" }, { "Broadhead Arrow Quiver", "Quivers" }, { "Penetrating Arrow Quiver", "Quivers" }, { "Spike-Point Arrow Quiver", "Quivers" },
        { "Ornate Quiver", "Quivers" },

        { "Splintered Tower Shield", "Shields" }, { "Corroded Tower Shield", "Shields" }, { "Rawhide Tower Shield", "Shields" }, { "Cedar Tower Shield", "Shields" },
        { "Copper Tower Shield", "Shields" }, { "Reinforced Tower Shield", "Shields" }, { "Painted Tower Shield", "Shields" }, { "Buckskin Tower Shield", "Shields" },
        { "Mahogany Tower Shield", "Shields" }, { "Bronze Tower Shield", "Shields" }, { "Girded Tower Shield", "Shields" }, { "Crested Tower Shield", "Shields" },
        { "Shagreen Tower Shield", "Shields" }, { "Ebony Tower Shield", "Shields" }, { "Ezomyte Tower Shield", "Shields" }, { "Colossal Tower Shield", "Shields" },
        { "Pinnacle Tower Shield", "Shields" }, { "Goathide Buckler", "Shields" }, { "Pine Buckler", "Shields" }, { "Painted Buckler", "Shields" },
        { "Hammered Buckler", "Shields" }, { "War Buckler", "Shields" }, { "Gilded Buckler", "Shields" }, { "Oak Buckler", "Shields" }, { "Enameled Buckler", "Shields" },
        { "Corrugated Buckler", "Shields" }, { "Battle Buckler", "Shields" }, { "Golden Buckler", "Shields" }, { "Ironwood Buckler", "Shields" },
        { "Lacquered Buckler", "Shields" }, { "Vaal Buckler", "Shields" }, { "Imperial Buckler", "Shields" }, { "Twig Spirit Shield", "Shields" },
        { "Yew Spirit Shield", "Shields" }, { "Bone Spirit Shield", "Shields" }, { "Tarnished Spirit Shield", "Shields" }, { "Jingling Spirit Shield", "Shields" },
        { "Brass Spirit Shield", "Shields" }, { "Walnut Spirit Shield", "Shields" }, { "Ivory Spirit Shield", "Shields" }, { "Ancient Spirit Shield", "Shields" },
        { "Chiming Spirit Shield", "Shields" }, { "Thorium Spirit Shield", "Shields" }, { "Lacewood Spirit Shield", "Shields" }, { "Fossilised Spirit Shield", "Shields" },
        { "Vaal Spirit Shield", "Shields" }, { "Harmonic Spirit Shield", "Shields" }, { "Titanium Spirit Shield", "Shields" }, { "Rotted Round Shield", "Shields" },
        { "Fir Round Shield", "Shields" }, { "Studded Round Shield", "Shields" }, { "Scarlet Round Shield", "Shields" }, { "Splendid Round Shield", "Shields" },
        { "Maple Round Shield", "Shields" }, { "Spiked Round Shield", "Shields" }, { "Crimson Round Shield", "Shields" }, { "Baroque Round Shield", "Shields" },
        { "Teak Round Shield", "Shields" }, { "Spiny Round Shield", "Shields" }, { "Cardinal Round Shield", "Shields" }, { "Elegant Round Shield", "Shields" },
        { "Plank Kite Shield", "Shields" }, { "Linden Kite Shield", "Shields" }, { "Reinforced Kite Shield", "Shields" }, { "Layered Kite Shield", "Shields" },
        { "Ceremonial Kite Shield", "Shields" }, { "Etched Kite Shield", "Shields" }, { "Steel Kite Shield", "Shields" }, { "Laminated Kite Shield", "Shields" },
        { "Angelic Kite Shield", "Shields" }, { "Branded Kite Shield", "Shields" }, { "Champion Kite Shield", "Shields" }, { "Mosaic Kite Shield", "Shields" },
        { "Archon Kite Shield", "Shields" }, { "Spiked Bundle", "Shields" }, { "Driftwood Spiked Shield", "Shields" }, { "Alloyed Spiked Shield", "Shields" },
        { "Burnished Spiked Shield", "Shields" }, { "Ornate Spiked Shield", "Shields" }, { "Redwood Spiked Shield", "Shields" }, { "Compound Spiked Shield", "Shields" },
        { "Polished Spiked Shield", "Shields" }, { "Sovereign Spiked Shield", "Shields" }, { "Alder Spiked Shield", "Shields" }, { "Ezomyte Spiked Shield", "Shields" },
        { "Mirrored Spiked Shield", "Shields" }, { "Supreme Spiked Shield", "Shields" }, { "Golden Flame", "Shields" },
        { "Transfer-attuned Spirit Shield", "Shields" }, { "Subsuming Spirit Shield", "Shields" }, { "Exhausting Spirit Shield", "Shields" }, { "Cold-attuned Buckler", "Shields" },
        { "Polar Buckler", "Shields" }, { "Endothermic Buckler", "Shields" }, { "Heat-attuned Tower Shield", "Shields" }, { "Magmatic Tower Shield", "Shields" }, { "Exothermic Tower Shield", "Shields" },

        { "Iron Gauntlets", "Gloves" }, { "Plated Gauntlets", "Gloves" }, { "Bronze Gauntlets", "Gloves" }, { "Steel Gauntlets", "Gloves" }, { "Antique Gauntlets", "Gloves" },
        { "Ancient Gauntlets", "Gloves" }, { "Goliath Gauntlets", "Gloves" }, { "Vaal Gauntlets", "Gloves" }, { "Titan Gauntlets", "Gloves" }, { "Rawhide Gloves", "Gloves" },
        { "Goathide Gloves", "Gloves" }, { "Deerskin Gloves", "Gloves" }, { "Nubuck Gloves", "Gloves" }, { "Eelskin Gloves", "Gloves" }, { "Sharkskin Gloves", "Gloves" },
        { "Shagreen Gloves", "Gloves" }, { "Stealth Gloves", "Gloves" }, { "Slink Gloves", "Gloves" }, { "Wool Gloves", "Gloves" }, { "Velvet Gloves", "Gloves" },
        { "Silk Gloves", "Gloves" }, { "Embroidered Gloves", "Gloves" }, { "Satin Gloves", "Gloves" }, { "Samite Gloves", "Gloves" }, { "Conjurer Gloves", "Gloves" },
        { "Arcanist Gloves", "Gloves" }, { "Sorcerer Gloves", "Gloves" }, { "Fishscale Gauntlets", "Gloves" }, { "Ironscale Gauntlets", "Gloves" },
        { "Bronzescale Gauntlets", "Gloves" }, { "Steelscale Gauntlets", "Gloves" }, { "Serpentscale Gauntlets", "Gloves" }, { "Wyrmscale Gauntlets", "Gloves" },
        { "Hydrascale Gauntlets", "Gloves" }, { "Dragonscale Gauntlets", "Gloves" }, { "Chain Gloves", "Gloves" }, { "Ringmail Gloves", "Gloves" }, { "Mesh Gloves", "Gloves" },
        { "Riveted Gloves", "Gloves" }, { "Zealot Gloves", "Gloves" }, { "Soldier Gloves", "Gloves" }, { "Legion Gloves", "Gloves" }, { "Crusader Gloves", "Gloves" },
        { "Wrapped Mitts", "Gloves" }, { "Strapped Mitts", "Gloves" }, { "Clasped Mitts", "Gloves" }, { "Trapper Mitts", "Gloves" }, { "Ambush Mitts", "Gloves" },
        { "Carnal Mitts", "Gloves" }, { "Assassin's Mitts", "Gloves" }, { "Murder Mitts", "Gloves" }, { "Golden Bracers", "Gloves" }, { "Spiked Gloves", "Gloves" },
        { "Gripped Gloves", "Gloves" }, { "Fingerless Silk Gloves", "Gloves" },

        { "Iron Greaves", "Boots" }, { "Steel Greaves", "Boots" }, { "Plated Greaves", "Boots" }, { "Reinforced Greaves", "Boots" }, { "Antique Greaves", "Boots" },
        { "Ancient Greaves", "Boots" }, { "Goliath Greaves", "Boots" }, { "Vaal Greaves", "Boots" }, { "Titan Greaves", "Boots" }, { "Kaom's Greaves", "Boots" },
        { "Rawhide Boots", "Boots" }, { "Goathide Boots", "Boots" }, { "Deerskin Boots", "Boots" }, { "Nubuck Boots", "Boots" }, { "Eelskin Boots", "Boots" },
        { "Sharkskin Boots", "Boots" }, { "Shagreen Boots", "Boots" }, { "Stealth Boots", "Boots" }, { "Slink Boots", "Boots" }, { "Wool Shoes", "Boots" },
        { "Velvet Slippers", "Boots" }, { "Silk Slippers", "Boots" }, { "Scholar Boots", "Boots" }, { "Satin Slippers", "Boots" }, { "Samite Slippers", "Boots" },
        { "Conjurer Boots", "Boots" }, { "Arcanist Slippers", "Boots" }, { "Sorcerer Boots", "Boots" }, { "Leatherscale Boots", "Boots" }, { "Ironscale Boots", "Boots" },
        { "Bronzescale Boots", "Boots" }, { "Steelscale Boots", "Boots" }, { "Serpentscale Boots", "Boots" }, { "Wyrmscale Boots", "Boots" }, { "Hydrascale Boots", "Boots" },
        { "Dragonscale Boots", "Boots" }, { "Chain Boots", "Boots" }, { "Ringmail Boots", "Boots" }, { "Mesh Boots", "Boots" }, { "Riveted Boots", "Boots" },
        { "Zealot Boots", "Boots" }, { "Soldier Boots", "Boots" }, { "Legion Boots", "Boots" }, { "Crusader Boots", "Boots" }, { "Wrapped Boots", "Boots" },
        { "Strapped Boots", "Boots" }, { "Clasped Boots", "Boots" }, { "Shackled Boots", "Boots" }, { "Trapper Boots", "Boots" }, { "Ambush Boots", "Boots" },
        { "Carnal Boots", "Boots" }, { "Assassin's Boots", "Boots" }, { "Murder Boots", "Boots" }, { "Golden Caligae", "Boots" }, { "Avian Slippers", "Boots" },
        { "Two-Toned Boots", "Boots" },

        { "Iron Hat", "Helmets" }, { "Cone Helmet", "Helmets" }, { "Barbute Helmet", "Helmets" }, { "Close Helmet", "Helmets" }, { "Gladiator Helmet", "Helmets" },
        { "Reaver Helmet", "Helmets" }, { "Siege Helmet", "Helmets" }, { "Samite Helmet", "Helmets" }, { "Ezomyte Burgonet", "Helmets" }, { "Royal Burgonet", "Helmets" },
        { "Eternal Burgonet", "Helmets" }, { "Leather Cap", "Helmets" }, { "Tricorne", "Helmets" }, { "Leather Hood", "Helmets" }, { "Wolf Pelt", "Helmets" },
        { "Hunter Hood", "Helmets" }, { "Noble Tricorne", "Helmets" }, { "Ursine Pelt", "Helmets" }, { "Silken Hood", "Helmets" }, { "Sinner Tricorne", "Helmets" },
        { "Lion Pelt", "Helmets" }, { "Vine Circlet", "Helmets" }, { "Iron Circlet", "Helmets" }, { "Torture Cage", "Helmets" }, { "Tribal Circlet", "Helmets" },
        { "Bone Circlet", "Helmets" }, { "Lunaris Circlet", "Helmets" }, { "Steel Circlet", "Helmets" }, { "Necromancer Circlet", "Helmets" }, { "Solaris Circlet", "Helmets" },
        { "Mind Cage", "Helmets" }, { "Hubris Circlet", "Helmets" }, { "Battered Helm", "Helmets" }, { "Sallet", "Helmets" }, { "Visored Sallet", "Helmets" },
        { "Gilded Sallet", "Helmets" }, { "Secutor Helm", "Helmets" }, { "Fencer Helm", "Helmets" }, { "Lacquered Helmet", "Helmets" }, { "Fluted Bascinet", "Helmets" },
        { "Pig-Faced Bascinet", "Helmets" }, { "Nightmare Bascinet", "Helmets" }, { "Rusted Coif", "Helmets" }, { "Soldier Helmet", "Helmets" }, { "Great Helmet", "Helmets" },
        { "Crusader Helmet", "Helmets" }, { "Aventail Helmet", "Helmets" }, { "Zealot Helmet", "Helmets" }, { "Great Crown", "Helmets" }, { "Magistrate Crown", "Helmets" },
        { "Prophet Crown", "Helmets" }, { "Praetor Crown", "Helmets" }, { "Scare Mask", "Helmets" }, { "Plague Mask", "Helmets" }, { "Iron Mask", "Helmets" },
        { "Festival Mask", "Helmets" }, { "Golden Mask", "Helmets" }, { "Raven Mask", "Helmets" }, { "Callous Mask", "Helmets" }, { "Regicide Mask", "Helmets" },
        { "Harlequin Mask", "Helmets" }, { "Vaal Mask", "Helmets" }, { "Deicide Mask", "Helmets" }, { "Golden Wreath", "Helmets" }, { "Golden Visage", "Helmets" },
        { "Bone Helmet", "Helmets" },

        { "Plate Vest", "Body Armours" }, { "Chestplate", "Body Armours" }, { "Copper Plate", "Body Armours" }, { "War Plate", "Body Armours" }, { "Full Plate", "Body Armours" },
        { "Arena Plate", "Body Armours" }, { "Lordly Plate", "Body Armours" }, { "Bronze Plate", "Body Armours" }, { "Battle Plate", "Body Armours" },
        { "Sun Plate", "Body Armours" }, { "Colosseum Plate", "Body Armours" }, { "Majestic Plate", "Body Armours" }, { "Golden Plate", "Body Armours" },
        { "Crusader Plate", "Body Armours" }, { "Astral Plate", "Body Armours" }, { "Gladiator Plate", "Body Armours" }, { "Glorious Plate", "Body Armours" },
        { "Kaom's Plate", "Body Armours" }, { "Shabby Jerkin", "Body Armours" }, { "Strapped Leather", "Body Armours" }, { "Buckskin Tunic", "Body Armours" },
        { "Wild Leather", "Body Armours" }, { "Full Leather", "Body Armours" }, { "Sun Leather", "Body Armours" }, { "Thief's Garb", "Body Armours" },
        { "Eelskin Tunic", "Body Armours" }, { "Frontier Leather", "Body Armours" }, { "Glorious Leather", "Body Armours" }, { "Coronal Leather", "Body Armours" },
        { "Cutthroat's Garb", "Body Armours" }, { "Sharkskin Tunic", "Body Armours" }, { "Destiny Leather", "Body Armours" }, { "Exquisite Leather", "Body Armours" },
        { "Zodiac Leather", "Body Armours" }, { "Assassin's Garb", "Body Armours" }, { "Simple Robe", "Body Armours" }, { "Silken Vest", "Body Armours" },
        { "Scholar's Robe", "Body Armours" }, { "Silken Garb", "Body Armours" }, { "Mage's Vestment", "Body Armours" }, { "Silk Robe", "Body Armours" },
        { "Cabalist Regalia", "Body Armours" }, { "Sage's Robe", "Body Armours" }, { "Silken Wrap", "Body Armours" }, { "Conjurer's Vestment", "Body Armours" },
        { "Spidersilk Robe", "Body Armours" }, { "Destroyer Regalia", "Body Armours" }, { "Savant's Robe", "Body Armours" }, { "Necromancer Silks", "Body Armours" },
        { "Occultist's Vestment", "Body Armours" }, { "Widowsilk Robe", "Body Armours" }, { "Vaal Regalia", "Body Armours" }, { "Scale Vest", "Body Armours" },
        { "Light Brigandine", "Body Armours" }, { "Scale Doublet", "Body Armours" }, { "Infantry Brigandine", "Body Armours" }, { "Full Scale Armour", "Body Armours" },
        { "Soldier's Brigandine", "Body Armours" }, { "Field Lamellar", "Body Armours" }, { "Wyrmscale Doublet", "Body Armours" }, { "Hussar Brigandine", "Body Armours" },
        { "Full Wyrmscale", "Body Armours" }, { "Commander's Brigandine", "Body Armours" }, { "Battle Lamellar", "Body Armours" }, { "Dragonscale Doublet", "Body Armours" },
        { "Desert Brigandine", "Body Armours" }, { "Full Dragonscale", "Body Armours" }, { "General's Brigandine", "Body Armours" }, { "Triumphant Lamellar", "Body Armours" },
        { "Chainmail Vest", "Body Armours" }, { "Chainmail Tunic", "Body Armours" }, { "Ringmail Coat", "Body Armours" }, { "Chainmail Doublet", "Body Armours" },
        { "Full Ringmail", "Body Armours" }, { "Full Chainmail", "Body Armours" }, { "Holy Chainmail", "Body Armours" }, { "Latticed Ringmail", "Body Armours" },
        { "Crusader Chainmail", "Body Armours" }, { "Ornate Ringmail", "Body Armours" }, { "Chain Hauberk", "Body Armours" }, { "Devout Chainmail", "Body Armours" },
        { "Loricated Ringmail", "Body Armours" }, { "Conquest Chainmail", "Body Armours" }, { "Elegant Ringmail", "Body Armours" }, { "Saint's Hauberk", "Body Armours" },
        { "Saintly Chainmail", "Body Armours" }, { "Padded Vest", "Body Armours" }, { "Oiled Vest", "Body Armours" }, { "Padded Jacket", "Body Armours" },
        { "Oiled Coat", "Body Armours" }, { "Scarlet Raiment", "Body Armours" }, { "Waxed Garb", "Body Armours" }, { "Bone Armour", "Body Armours" },
        { "Quilted Jacket", "Body Armours" }, { "Sleek Coat", "Body Armours" }, { "Crimson Raiment", "Body Armours" }, { "Lacquered Garb", "Body Armours" },
        { "Crypt Armour", "Body Armours" }, { "Sentinel Jacket", "Body Armours" }, { "Varnished Coat", "Body Armours" }, { "Blood Raiment", "Body Armours" },
        { "Sadist Garb", "Body Armours" }, { "Carnal Armour", "Body Armours" }, { "Sacrificial Garb", "Body Armours" }, { "Golden Mantle", "Body Armours" },

        { "Paua Amulet", "Amulets" }, { "Coral Amulet", "Amulets" }, { "Amber Amulet", "Amulets" }, { "Jade Amulet", "Amulets" }, { "Lapis Amulet", "Amulets" },
        { "Gold Amulet", "Amulets" }, { "Onyx Amulet", "Amulets" }, { "Turquoise Amulet", "Amulets" }, { "Agate Amulet", "Amulets" }, { "Citrine Amulet", "Amulets" },
        { "Ruby Amulet", "Amulets" }, { "Jet Amulet", "Amulets" }, { "Blue Pearl Amulet", "Amulets" }, { "Marble Amulet", "Amulets" }, { "Black Maw Talisman", "Amulets" },
        { "Bonespire Talisman", "Amulets" }, { "Ashscale Talisman", "Amulets" }, { "Lone Antler Talisman", "Amulets" }, { "Deep One Talisman", "Amulets" },
        { "Breakrib Talisman", "Amulets" }, { "Deadhand Talisman", "Amulets" }, { "Undying Flesh Talisman", "Amulets" }, { "Rot Head Talisman", "Amulets" },
        { "Mandible Talisman", "Amulets" }, { "Chrysalis Talisman", "Amulets" }, { "Writhing Talisman", "Amulets" }, { "Hexclaw Talisman", "Amulets" },
        { "Primal Skull Talisman", "Amulets" }, { "Wereclaw Talisman", "Amulets" }, { "Splitnewt Talisman", "Amulets" }, { "Clutching Talisman", "Amulets" },
        { "Avian Twins Talisman", "Amulets" }, { "Fangjaw Talisman", "Amulets" }, { "Horned Talisman", "Amulets" }, { "Spinefuse Talisman", "Amulets" },
        { "Three Rat Talisman", "Amulets" }, { "Monkey Twins Talisman", "Amulets" }, { "Longtooth Talisman", "Amulets" }, { "Rotfeather Talisman", "Amulets" },
        { "Monkey Paw Talisman", "Amulets" }, { "Three Hands Talisman", "Amulets" }, { "Greatwolf Talisman", "Amulets" }, { "Simplex Amulet", "Amulets" }, { "Astrolabe Amulet", "Amulets" },

        { "Golden Hoop", "Rings" }, { "Iron Ring", "Rings" }, { "Coral Ring", "Rings" }, { "Paua Ring", "Rings" }, { "Gold Ring", "Rings" }, { "Topaz Ring", "Rings" },
        { "Sapphire Ring", "Rings" }, { "Ruby Ring", "Rings" }, { "Prismatic Ring", "Rings" }, { "Moonstone Ring", "Rings" }, { "Amethyst Ring", "Rings" },
        { "Diamond Ring", "Rings" }, { "Two-Stone Ring", "Rings" }, { "Unset Ring", "Rings" }, { "Jet Ring", "Rings" }, { "Steel Ring", "Rings" }, { "Opal Ring", "Rings" },
        { "Vermillion Ring", "Rings" }, { "Cerulean Ring", "Rings" }, { "Breach Ring", "Rings" }, { "Geodesic Ring", "Rings" }, { "Cogwork Ring", "Rings" },

        { "Rustic Sash", "Belts" }, { "Chain Belt", "Belts" }, { "Leather Belt", "Belts" }, { "Heavy Belt", "Belts" }, { "Cloth Belt", "Belts" }, { "Studded Belt", "Belts" },
        { "Vanguard Belt", "Belts" }, { "Crystal Belt", "Belts" }, { "Golden Obi", "Belts" }, { "Stygian Vise", "Belts" }, { "Mechalarm Belt", "Belts" }, { "Micro-Distillery Belt", "Belts" },

        { "Crimson Jewel", "Jewel" }, { "Viridian Jewel", "Jewel" }, { "Cobalt Jewel", "Jewel" }, { "Prismatic Jewel", "Jewel" }, { "Timeless Jewel", "Jewel" },
        { "Large Cluster Jewel", "Jewel" }, { "Medium Cluster Jewel", "Jewel" }, { "Small Cluster Jewel", "Jewel" }, { "Murderous Eye Jewel", "Jewel" },
        { "Searching Eye Jewel", "Jewel" }, { "Hypnotic Eye Jewel", "Jewel" }, { "Ghastly Eye Jewel", "Jewel" },

        { "Fishing Rod", "Fishing Rods" }
    };

    internal static readonly Dictionary<string, string> dicInherit = new()
    {
        { "Weapons", "weapon" }, { "Quivers", "armour.quiver" }, { "Armours", "armour" },
        { "Amulets", "accessory.amulet" }, { "Rings", "accessory.ring" }, { "Belts", "accessory.belt" }, /* accessory */
        { "Jewels", "jewel" }, { "Flasks", "flask" }, { "DivinationCards", "card" }, { "Prophecies", "prophecy" }, { "Gems", "gem" },
        { "Currency", "currency" }, { "Maps", "map" }, /*{ "MapFragments", "map" },*/ { "Scarabs", "map" },
        {Inherit.Sentinel, "sentinel"}, {Inherit.Charms, "azmeri.charm"}, {Inherit.Tinctures, "tincture"}
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
}