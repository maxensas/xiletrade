namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal static class NinjaTypeTwo
    {
        internal const string Currency = "Currency";
        internal const string Fragments = "Fragments";
        internal const string Expedition = "Expedition";
        internal const string Essences = "Essences";
        internal const string Talismans = "Talismans";
        internal const string Runes = "Runes";
        internal const string LineageSupportGems = "LineageSupportGems";
        internal const string UncutGems = "UncutGems";
        internal const string Abyss = "Abyss"; // Abyssal Bones
        internal const string Delirium = "Delirium"; // Distilled Emotions
        internal const string SoulCores = "SoulCores"; // Ultimatum & Incursion
        internal const string Breach = "Breach"; // Catalysts
        internal const string Ritual = "Ritual"; // Omens
        internal const string Idols = "Idols";
        internal const string Verisium = "Verisium";

        internal static readonly string[] ExchangeNames =
        [
            Currency, Fragments, Abyss,
            UncutGems, LineageSupportGems, Essences,
            SoulCores, Idols, Runes,
            Ritual, Expedition, Delirium,
            Breach, Verisium
        ];

        // non currency exchange
        internal const string UniqueWeapons = "UniqueWeapons";
        internal const string UniqueArmours = "UniqueArmours";
        internal const string UniqueAccessories = "UniqueAccessories";
        internal const string UniqueFlasks = "UniqueFlasks";
        internal const string UniqueCharms = "UniqueCharms";
        internal const string UniqueJewels = "UniqueJewels";
        internal const string UniqueSanctumRelics = "UniqueSanctumRelics";
        internal const string UniqueTablets = "UniqueTablets";
        internal const string PrecursorTablets = "PrecursorTablets"; // not used

        internal static readonly string[] ItemNames =
        [
            UniqueWeapons, UniqueArmours, UniqueAccessories,
            UniqueFlasks, UniqueCharms, UniqueJewels,
            UniqueTablets, UniqueSanctumRelics
        ];
    }
}
