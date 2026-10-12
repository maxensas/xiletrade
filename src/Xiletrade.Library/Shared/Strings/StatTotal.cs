using System;

namespace Xiletrade.Library.Shared;

public static partial class Strings
{
    internal class StatTotal
    {
        private static readonly string[] lTotalStatLifeUnwanted =
                [ "per", "added small passive", "strength provides no bonus", "raised zombies have",
            "intelligence allocated in radius", "intelligence from passives", "dexterity from passives"];

        private static readonly string[] lTotalStatEsUnwanted =
            ["per", "added small passive", "left ring slot"];

        private static readonly string[] lTotalStatResistUnwanted =
            [ "per", "added small passive", "effect", "maximum", "corrupted", "against", "while", "penetrate", "minions",
            "summoned", "enemies", "zombies", "totem", "chance"];

        internal static bool IsTotalStat(ReadOnlySpan<char> modEn, Enum.Stat stat)
        {
            ReadOnlySpan<string> unwanted = stat switch
            {
                Enum.Stat.Life => lTotalStatLifeUnwanted,
                Enum.Stat.Es => lTotalStatEsUnwanted,
                _ => lTotalStatResistUnwanted
            };

            foreach (var word in unwanted)
            {
                if (modEn.Contains(word.AsSpan(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return stat switch
            {
                Enum.Stat.Life =>
                    modEn.Contains(Strings.Word.ToMaxLife, StringComparison.OrdinalIgnoreCase) ||
                    modEn.Contains(Strings.Word.ToStrength, StringComparison.OrdinalIgnoreCase),

                Enum.Stat.Es =>
                    modEn.Contains(Strings.Word.ToMaxEs, StringComparison.OrdinalIgnoreCase),

                Enum.Stat.Resist =>
                    modEn.Contains(Strings.Word.Resistance, StringComparison.OrdinalIgnoreCase) &&
                    !modEn.Contains(Strings.Word.Chaos, StringComparison.OrdinalIgnoreCase),

                _ => false
            };
        }
    }
}

