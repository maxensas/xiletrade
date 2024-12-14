namespace Xiletrade.Json;

/// <summary>
/// Class containing strings related to PoE 1 or 2.
/// </summary>
internal sealed class GameStrings
{
    private static bool _isPoe2;

    internal string BaseItemTypes { get; } = "baseitemtypes";
    internal string Mods { get; } = "mods";
    internal string MonsterVarieties { get; } = "monstervarieties";
    internal string Words { get; } = "words";
    internal string Gems { get; } = "gemeffects";

    internal string[] PathGgpk { get; private set; }

    /// <summary>
    /// Contains collections : DatNames as Keys and JsonNames as Values.
    /// </summary>
    internal Dictionary<string, string> Names { get; private set; }

    internal GameStrings(bool isPoe2 = false)
    {
        _isPoe2 = isPoe2;
        if (_isPoe2)
        {
            Names = new()
            { 
                { BaseItemTypes, "BasesTwo.json"},
                { Mods, "ModsTwo.json"},
                { Words, "WordsTwo.json"} 
            };
            PathGgpk = GetPoe2Path();
            return;
        }

        Names = new()
            {
                { BaseItemTypes, "Bases.json"},
                { Mods, "Mods.json"},
                { MonsterVarieties, "Monsters.json"},
                { Words, "Words.json"},
                { Gems, "Gems.json"}
            };
        PathGgpk = GetPoePath();
    }

    private static string[] GetPoePath()
    {
        return [
            "C:\\Path of Exile\\Content.ggpk",
            "C:\\Jeux\\Path of Exile\\Content.ggpk",
            "C:\\Program Files\\Path of Exile\\Content.ggpk",
            "C:\\Program Files (x86)\\Path of Exile\\Content.ggpk",
            "C:\\Games\\Path of Exile\\Content.ggpk",
            "D:\\Jeux\\POE-Chinese\\Content.ggpk",
            "D:\\Path of Exile\\Content.ggpk",
            "D:\\Jeux\\Path of Exile\\Content.ggpk",
            "D:\\Games\\Path of Exile\\Content.ggpk"
        ];
    }

    private static string[] GetPoe2Path()
    {
        return [
            "C:\\Path of Exile 2\\Content.ggpk",
            "C:\\Jeux\\Path of Exile 2\\Content.ggpk",
            "C:\\Program Files\\Path of Exile 2\\Content.ggpk",
            "C:\\Program Files (x86)\\Path of Exile 2\\Content.ggpk",
            "C:\\Games\\Path of Exile 2\\Content.ggpk",
            "D:\\Path of Exile 2\\Content.ggpk",
            "D:\\Jeux\\Path of Exile 2\\Content.ggpk",
            "D:\\Games\\Path of Exile 2\\Content.ggpk"
        ];
    }

    /// <summary>
    /// Return json name.
    /// </summary>
    /// <returns></returns>
    public string GetDefinition()
    {
        return "DatDefinitions" + (_isPoe2 ? "2" : string.Empty) + ".json";
    }

    /// <summary>
    /// Return game version.
    /// </summary>
    /// <returns></returns>
    public string GetVersion()
    {
        return _isPoe2 ? "2" : "1";
    }

    /// <summary>
    /// Return DAT file extension.
    /// </summary>
    /// <returns></returns>
    public string GetDatExtension()
    {
        return _isPoe2 ? ".datc64" : ".dat64";
    }
}
