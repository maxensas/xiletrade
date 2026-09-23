namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class File
    {
        internal const string Config = "Config.json";
        internal const string DefaultConfig = "DefaultConfig.json";
        internal const string Divination = "Divination.json";
        internal const string ParsingRules = "ParsingRules.json";
        internal const string Monsters = "Monsters.json";
        internal const string Gems = "Gems.json";
        internal const string DustLevel = "DustLevel.json";
        internal const string SearchPreset = "SearchPreset.json";
        internal const string AppSettings = "AppSettings.json";

        internal const string _currency1 = "Currency.json";
        internal const string _filters1 = "Filters.json";
        internal const string _leagues1 = "Leagues.json";
        internal const string _bases1 = "Bases.json";
        internal const string _mods1 = "Mods.json";
        internal const string _words1 = "Words.json";
        internal const string _items1 = "Items.json";

        internal const string _currency2 = "CurrencyTwo.json";
        internal const string _filters2 = "FiltersTwo.json";
        internal const string _leagues2 = "LeaguesTwo.json";
        internal const string _bases2 = "BasesTwo.json";
        internal const string _mods2 = "ModsTwo.json";
        internal const string _words2 = "WordsTwo.json";
        internal const string _items2 = "ItemsTwo.json";

        internal static string Currency { get => IsPoe2 ? _currency2 : _currency1; }
        internal static string Filters { get => IsPoe2 ? _filters2 : _filters1; }
        internal static string Leagues { get => IsPoe2 ? _leagues2 : _leagues1; }
        internal static string Bases { get => IsPoe2 ? _bases2 : _bases1; }
        internal static string Mods { get => IsPoe2 ? _mods2 : _mods1; }
        internal static string Words { get => IsPoe2 ? _words2 : _words1; }
        internal static string Items { get => IsPoe2 ? _items2 : _items1; }
    }
}
