namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class CurrencyTypePoe2
    {
        internal const string Currency = "Currency";
        internal const string Fragments = "Fragments";
        internal const string Runes = "Runes";
        internal const string Essences = "Essences";
        internal const string Relics = "Relics";
        internal const string Ultimatum = "Ultimatum";
        internal const string Breach = "Breach";
        internal const string Expedition = "Expedition";
        internal const string Ritual = "Ritual";
        internal const string Delirium = "Delirium";
        internal const string Waystones = "Waystones";
        internal const string VaultKeys = "VaultKeys";
        internal const string Talismans = "Talismans";
        internal const string Abyss = "Abyss";
        internal const string UncutGems = "UncutGems";
        internal const string LineageSupportGems = "LineageSupportGems";
        internal const string Idol = "Idol";
        internal const string Verisium = "Verisium";
        internal const string Vaal = "Vaal";

        internal static string GetCategory(string curClass, string curId)
        {
            return curClass is Currency ?
                    Strings.Collection.dicMainCur.TryGetValue(curId, out _) ?
                    Resources.Resources.Main044_MainCur : Resources.Resources.Main045_OtherCur :
                    curClass is Fragments ? Resources.Resources.Main046_MapFrag :
                    curClass is Runes ? Resources.Resources.General132_Rune :
                    curClass is Essences ? Resources.Resources.Main054_Essences :
                    curClass is Relics ? Resources.Resources.ItemClass_sanctumRelic :
                    curClass is Ultimatum ? Resources.Resources.General069_Ultimatum :
                    curClass is Breach ? Resources.Resources.Main049_Catalysts :
                    curClass is Expedition ? Resources.Resources.Main186_Expedition :
                    curClass is Ritual ? Resources.Resources.ItemClass_omen :
                    curClass is Delirium ? Resources.Resources.Main236_Delirium :
                    curClass is Waystones ? Resources.Resources.ItemClass_maps :
                    curClass is Talismans ? Resources.Resources.ItemClass_talismans :
                    curClass is VaultKeys ? Resources.Resources.ItemClass_vaultKeys :
                    curClass is Abyss ? Resources.Resources.Main235_AbyssalBones :
                    curClass is UncutGems ? Resources.Resources.Main237_UncutGems :
                    curClass is LineageSupportGems ? Resources.Resources.Main238_LineageGems :
                    string.Empty;
        }
    }
}