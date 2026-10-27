namespace Xiletrade.DatToJson;

/// <summary>
/// Where the dat files are in the game index, and which json files are produced from them.
/// </summary>
internal static class GameData
{
    internal const string English = "english";

    internal const string ItemClasses = "itemclasses";
    internal const string BaseItemTypes = "baseitemtypes";
    internal const string Mods = "mods";
    internal const string MonsterVarieties = "monstervarieties";
    internal const string Words = "words";
    internal const string GemEffects = "gemeffects";

    /// <summary>Language folder in the game index => culture folder of Xiletrade.</summary>
    internal static readonly (string Folder, string Culture)[] GlobalLanguages =
    [
        (English, "en-US"),
        ("french", "fr-FR"),
        ("german", "de-DE"),
        ("japanese", "ja-JP"),
        ("korean", "ko-KR"),
        ("portuguese", "pt-BR"),
        ("russian", "ru-RU"),
        ("spanish", "es-ES"),
        ("thai", "th-TH"),
        ("traditional chinese", "zh-TW")
    ];

    /// <summary>Languages of the Tencent client (chinese realm).</summary>
    internal static readonly (string Folder, string Culture)[] TencentLanguages =
    [
        (English, "en-US"),
        ("simplified chinese", "zh-CN")
    ];

    /// <summary>
    /// Dat tables exported, in processing order (bases are needed by gems), with their json file.
    /// English is always processed first: the other languages take their english names from it.
    /// </summary>
    internal static IReadOnlyList<(string Table, string JsonFile)> GetOutputs(int game)
        => game is 2 ? Poe2Outputs : Poe1Outputs;

    private static readonly (string Table, string JsonFile)[] Poe1Outputs =
    [
        (ItemClasses, "ItemClass.json"),
        (BaseItemTypes, "Bases.json"),
        (Mods, "Mods.json"),
        (MonsterVarieties, "Monsters.json"),
        (Words, "Words.json"),
        (GemEffects, "Gems.json")
    ];

    private static readonly (string Table, string JsonFile)[] Poe2Outputs =
    [
        (ItemClasses, "ItemClassTwo.json"),
        (BaseItemTypes, "BasesTwo.json"),
        (Mods, "ModsTwo.json"),
        (Words, "WordsTwo.json")
    ];

    /// <summary>Path of a dat file inside the game index (PoE 2 tables are under data/balance).</summary>
    internal static string GetDatPath(int game, string language, string table)
    {
        var root = game is 2 ? "data/balance/" : "data/";
        return language is English
            ? root + table + ".datc64"
            : root + language + "/" + table + ".datc64";
    }
}
