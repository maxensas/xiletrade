using System.Collections.Generic;

namespace Xiletrade.Library.Shared;

public static partial class Strings
{
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

        internal const string Maps = "Maps"; // MapKey
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
        internal const string DjinnCoins = "DjinnCoins";

        internal const string Ducats = "Ducats";
        internal const string EnshroudingCrystals = "EnshroudingCrystals";
        internal const string Keepers = "Keepers";

        internal const string Legacy = "Legacy";

        internal static string GetCategory(string curClass, string curId)
        {
            return curClass is Currency ?
                    Collection.dicMainCur.TryGetValue(curId, out _) ? Resources.Resources.Main044_MainCur :
                    Collection.dicExoticCur.TryGetValue(curId, out _) ? Resources.Resources.Main207_ExoticCurrency : Resources.Resources.Main045_OtherCur :
                    curClass is DjinnCoins ? Resources.Resources.Main045_OtherCur :
                    curClass is Fragments ? Collection.dicStones.TryGetValue(curId, out _) ? Resources.Resources.Main047_Stones
                    : curId.Contain(Word.scarab) ? Resources.Resources.Main052_Scarabs : Resources.Resources.Main046_MapFrag :
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

        internal static readonly Dictionary<string, string> dicCurrencyCdnById = new()
        {
            [Cards] = Cdn.Cards,
            [Prophecies] = Cdn.Prophecies,
            [MapsUnique] = Cdn.MapsUnique,
            [Beasts] = Cdn.Beasts,
            [Heist] = Cdn.Heist,
            [Sanctum] = Cdn.Sanctum,
            [ScoutingReport] = Cdn.ScoutingReport
        };
    }
}