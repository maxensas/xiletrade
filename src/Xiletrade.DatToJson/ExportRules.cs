using System.Text;

namespace Xiletrade.DatToJson;

/// <summary>
/// Business rules applied to the game values before they are written for Xiletrade.
/// Same rules as the historical Xiletrade.Json generator, so the produced files are identical.
/// </summary>
internal static class ExportRules
{
    internal const string MetaItem = "Metadata/Items/";
    internal const string MetaMonster = "Metadata/Monsters/";

    internal const string NameUnwanted = "[DNT]";
    internal const string DoNotUse = "[DO NOT USE]";
    internal const string UnUsed = "[UNUSED]";
    internal const string DoNotUseKorean = "[사용 안 함]";

    internal const string StackableCurrency = "Currency/StackableCurrency";
    internal const string IncursionVial = "CurrencyIncursionVial";

    internal const string LeagueBestiary = "LeagueBestiary";
    internal const string LeagueHarvest = "LeagueHarvest";

    /// <summary>Monster duplicated by the game (Black Mórrigan): the bestiary version is kept.</summary>
    internal const string DuplicatedMonster = "LeagueAzmeri/GullGoliath_";
    internal const string DuplicatedMonsterKept = "LeagueAzmeri/GullGoliathBestiary_";

    internal const string GemDelimiter = "Alt";

    internal static readonly string[] ModIdPrefixUnwanted =
    [
        "Essence", "Enchantment", "WeaponEnchantment", "ArmourEnchantment",
        "BlightEnchantment", "TalismanMonster", "ArmageddonBrand", "StormBrand", "CritChanceWith"
    ];

    internal static readonly HashSet<string> BaseInheritsUnwanted = new(StringComparer.Ordinal)
    {
        "Currency/AbstractMicrotransaction", "HideoutDoodads/AbstractHideoutDoodad",
        "PantheonSouls/AbstactPantheonSoul", "Delve/DelveSocketableCurrency", "Delve/DelveStackableSocketableCurrency",
        "Maps/AbstractMap", "MapFragments/AbstractMapFragment", "DivinationCards/AbstractDivinationCard",
        "Legion/Incubator", "Item", "QuestItems/AbstractQuestItem", "Labyrinth/AbstractLabyrinthItem",
        "AtlasUpgrades/AtlasRegionUpgrade"
    };

    /// <summary>Bases the game does not translate (relics for now).</summary>
    internal static readonly Dictionary<string, string> BasesNotTranslatedPortuguese = new(StringComparer.Ordinal)
    {
        ["Censer Relic"] = "Relíquia: Incensário",
        ["Processional Relic"] = "Relíquia: Processional",
        ["Candlestick Relic"] = "Relíquia: Castiçal",
        ["Urn Relic"] = "Relíquia: Urna",
        ["Coffer Relic"] = "Relíquia: Arca",
        ["Tome Relic"] = "Relíquia: Tomo",
        ["Papyrus Relic"] = "Relíquia: Papiro"
    };

    /// <inheritdoc cref="BasesNotTranslatedPortuguese"/>
    internal static readonly Dictionary<string, string> BasesNotTranslatedRussian = new(StringComparer.Ordinal)
    {
        ["Censer Relic"] = "Курительница",
        ["Processional Relic"] = "Кадило",
        ["Candlestick Relic"] = "Подсвечник",
        ["Urn Relic"] = "Урна",
        ["Coffer Relic"] = "Ларец",
        ["Tome Relic"] = "Фолиант",
        ["Papyrus Relic"] = "Папирус"
    };

    /// <summary>Grammatical forms: "&lt;if:MS&gt;{a}&lt;elif:FS&gt;{b}" becomes "a/b".</summary>
    private static readonly (string Key, string Value)[] NameRules =
    [
        ("<if:MS>{", string.Empty),
        ("}<elif:FS>{", "/"),
        ("}<elif:NS>{", "/"),
        ("}<elif:MP>{", "/"),
        ("}<elif:FP>{", "/"),
        ("}<elif:NP>{", "/"),
        ("}", string.Empty),
        ("/NONEXISTENT", string.Empty),
        ("/ NONEXISTENT", string.Empty)
    ];

    /// <summary>
    /// Mod names broken on a new line by the game in some languages.
    /// </summary>
    internal static string FixModName(string language, string name) => language switch
    {
        "japanese" => name.Replace("\n信心深さの", "信心深さの", StringComparison.Ordinal),
        "spanish" => name.Replace("daño de fuego de Golpe infernal 1\n", "daño de fuego de Golpe infernal 1", StringComparison.Ordinal),
        _ => name
    };

    /// <summary>
    /// Joins the grammatical forms of a name with '/', without duplicates. Other names are only trimmed.
    /// </summary>
    internal static string ParseMultipleName(string name)
    {
        if (!name.StartsWith(NameRules[0].Key, StringComparison.Ordinal))
        {
            return name.Trim();
        }

        var sb = new StringBuilder(name);
        foreach (var (key, value) in NameRules)
        {
            sb.Replace(key, value);
        }
        var forms = sb.ToString().Split('/', StringSplitOptions.TrimEntries);
        return string.Join('/', forms.Distinct(StringComparer.Ordinal));
    }

    /// <summary>Base name cleaned from the markers the developers put on unused items.</summary>
    internal static string CleanBaseName(string name)
        => name.Replace(' ', ' ')
            .Replace(DoNotUse, string.Empty, StringComparison.Ordinal)
            .Replace(UnUsed, string.Empty, StringComparison.Ordinal)
            .Replace(DoNotUseKorean, string.Empty, StringComparison.Ordinal)
            .Trim();

    internal static bool IsUnwantedModId(ReadOnlySpan<char> id)
    {
        foreach (var prefix in ModIdPrefixUnwanted)
        {
            if (id.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Readable type of a gem effect when no base matches: "AltX"/"AltY" removed, a space before each capital.
    /// </summary>
    internal static string GemTypeFromId(string id)
    {
        var source = id.Replace("AltX", string.Empty, StringComparison.Ordinal)
            .Replace("AltY", string.Empty, StringComparison.Ordinal);
        var sb = new StringBuilder(source.Length + 8);
        for (var i = 0; i < source.Length; i++)
        {
            if (i > 0 && char.IsUpper(source[i]))
            {
                sb.Append(' ');
            }
            sb.Append(source[i]);
        }
        return sb.ToString();
    }
}
