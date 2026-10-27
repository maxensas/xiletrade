using System.Globalization;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Serialization;

namespace Xiletrade.DatToJson;

internal sealed class ExportResult
{
    /// <summary>Written files, relative to the output directory.</summary>
    internal List<string> Files { get; } = [];
    internal List<string> Warnings { get; } = [];
}

/// <summary>
/// Writes the json files consumed by Xiletrade, for every language, from the dat files and the minimal schema.
/// </summary>
/// <remarks>
/// English is exported first: the other languages take their english names from it,
/// and entries that do not exist in english are left out.
/// In each file, entries keep the dat row order and a name only appears once (first occurrence kept).
/// Files are written through <see cref="JsonChanges"/>, which compares them with the previous generation first.
/// </remarks>
internal sealed class JsonExporter(GameSchema schema, IDatSource source, string outputDirectory, JsonChanges changes)
{
    private readonly JsonHelper _json = new();

    // First written result of each table (english), indexed for the other languages.
    private Dictionary<int, ItemClass>? _itemClassesEn;
    private Dictionary<string, BaseResultData>? _basesEn;
    private Dictionary<string, ModResultData>? _modsEn;
    private Dictionary<string, BaseResultData>? _monstersEn;
    private Dictionary<string, GemResultData>? _gemsEn;

    // Bases of the last exported language, used to find the type of gems.
    private BaseResultData[] _bases = [];

    internal ExportResult Export()
    {
        var result = new ExportResult();
        var languages = IsTencentClient() ? GameData.TencentLanguages : GameData.GlobalLanguages;

        foreach (var (language, culture) in languages)
        {
            var directory = Path.Combine(outputDirectory, culture);
            foreach (var (tableName, jsonFile) in GameData.GetOutputs(schema.Game))
            {
                var path = GameData.GetDatPath(schema.Game, language, tableName);
                var data = source.Read(path);
                if (data is null)
                {
                    result.Warnings.Add($"Not found in game files: {path}");
                    continue;
                }

                var reader = new TableReader(DatFile.Parse(data), GetTable(tableName), path);
                var jsonPath = Path.Combine(directory, jsonFile);
                var written = tableName switch
                {
                    GameData.ItemClasses => ExportItemClasses(reader, jsonPath),
                    GameData.BaseItemTypes => ExportBases(reader, language, jsonPath),
                    GameData.Mods => ExportMods(reader, language, jsonPath),
                    GameData.MonsterVarieties => ExportMonsters(reader, jsonPath),
                    GameData.Words => ExportWords(reader, jsonPath),
                    GameData.GemEffects => ExportGems(reader, jsonPath),
                    _ => throw new InvalidOperationException($"No export for table {tableName}.")
                };

                if (written)
                {
                    result.Files.Add(Path.Combine(culture, jsonFile));
                }
                else
                {
                    result.Warnings.Add($"Nothing to write for {path}");
                }
            }
        }
        return result;
    }

    /// <summary>The Tencent client only has english and simplified chinese.</summary>
    private bool IsTencentClient()
    {
        var (language, _) = GameData.TencentLanguages[1];
        return source.Exists(GameData.GetDatPath(schema.Game, language, GameData.ItemClasses));
    }

    private TableSchema GetTable(string name)
        => schema.Tables.Find(t => t.Name == name)
            ?? throw new InvalidDataException($"Table {name} is missing in the schema.");

    private bool ExportItemClasses(TableReader reader, string jsonPath)
    {
        int idOffset = reader.Offset("Id"), nameOffset = reader.Offset("Name");
        List<ItemClass> list = [];
        HashSet<string> names = new(StringComparer.Ordinal);

        for (var row = 0; row < reader.RowCount; row++)
        {
            ItemClass? english = null;
            if (_itemClassesEn is not null && !_itemClassesEn.TryGetValue(row, out english))
            {
                continue;
            }

            var name = reader.String(row, nameOffset);
            if (!names.Add(name))
            {
                continue;
            }
            list.Add(new ItemClass
            {
                Id = row,
                IdOrigin = reader.String(row, idOffset),
                Name = name,
                NameEn = english is null ? name : english.Name
            });
        }

        if (list.Count is 0)
        {
            return false;
        }
        _itemClassesEn ??= IndexFirst(list, static c => c.Id);
        Write<ItemClassData>(new ItemClassData { ItemClass = [.. list] }, jsonPath);
        return true;
    }

    private bool ExportBases(TableReader reader, string language, string jsonPath)
    {
        int idOffset = reader.Offset("Id"), classOffset = reader.Offset("ItemClassesKey"),
            nameOffset = reader.Offset("Name"), inheritsOffset = reader.Offset("InheritsFrom");
        List<BaseResultData> list = [];
        HashSet<string> names = new(StringComparer.Ordinal);

        for (var row = 0; row < reader.RowCount; row++)
        {
            var inherits = reader.String(row, inheritsOffset).Replace(ExportRules.MetaItem, string.Empty, StringComparison.Ordinal);
            if (ExportRules.BaseInheritsUnwanted.Contains(inherits))
            {
                continue;
            }

            var id = reader.String(row, idOffset).Replace(ExportRules.MetaItem, string.Empty, StringComparison.Ordinal);
            if (inherits == ExportRules.StackableCurrency && !id.Contains(ExportRules.IncursionVial, StringComparison.Ordinal))
            {
                continue;
            }

            var name = TranslateBase(language, ExportRules.CleanBaseName(reader.String(row, nameOffset)));
            if (name.Contains(ExportRules.NameUnwanted, StringComparison.Ordinal))
            {
                continue;
            }

            BaseResultData? english = null;
            if (_basesEn is not null && !_basesEn.TryGetValue(id, out english))
            {
                continue;
            }
            if (!names.Add(name))
            {
                continue;
            }
            list.Add(new BaseResultData
            {
                Id = id,
                Name = name,
                NameEn = english is null ? name : ExportRules.CleanBaseName(english.Name),
                ItemClassId = reader.ForeignRow(row, classOffset)
            });
        }

        if (list.Count is 0)
        {
            return false;
        }
        _bases = [.. list];
        _basesEn ??= IndexFirst(list, static b => b.Id);
        Write<BaseData>(new BaseData { Result = [new BaseResult { Data = _bases }] }, jsonPath);
        return true;
    }

    private static string TranslateBase(string language, string name)
    {
        var translations = language switch
        {
            "russian" => ExportRules.BasesNotTranslatedRussian,
            "portuguese" => ExportRules.BasesNotTranslatedPortuguese,
            _ => null
        };
        return name.Length > 0 && translations is not null && translations.TryGetValue(name, out var translated) && translated.Length > 0
            ? translated
            : name;
    }

    private bool ExportMods(TableReader reader, string language, string jsonPath)
    {
        int idOffset = reader.Offset("Id"), levelOffset = reader.Offset("Level"), nameOffset = reader.Offset("Name");
        List<ModResultData> list = [];
        HashSet<string> names = new(StringComparer.Ordinal);

        for (var row = 0; row < reader.RowCount; row++)
        {
            // Checked on the raw values first: most mods have no name or an unwanted id.
            if (reader.Text(row, nameOffset).IsEmpty || ExportRules.IsUnwantedModId(reader.Text(row, idOffset)))
            {
                continue;
            }
            var rawName = reader.String(row, nameOffset);
            if (rawName.Length is 0)
            {
                continue;
            }

            var id = reader.String(row, idOffset);
            ModResultData? english = null;
            if (_modsEn is not null && !_modsEn.TryGetValue(id, out english))
            {
                continue;
            }

            var name = ExportRules.ParseMultipleName(ExportRules.FixModName(language, rawName));
            if (!names.Add(name))
            {
                continue;
            }
            list.Add(new ModResultData
            {
                Id = id,
                Name = name,
                NameEn = english is null ? name : english.Name,
                Level = reader.Int32(row, levelOffset).ToString(CultureInfo.InvariantCulture)
            });
        }

        if (list.Count is 0)
        {
            return false;
        }
        _modsEn ??= IndexFirst(list, static m => m.Id);
        Write<ModData>(new ModData { Result = [new ModResult { Data = [.. list] }] }, jsonPath);
        return true;
    }

    private bool ExportMonsters(TableReader reader, string jsonPath)
    {
        int idOffset = reader.Offset("Id"), nameOffset = reader.Offset("Name");
        List<BaseResultData> list = [];
        HashSet<string> names = new(StringComparer.Ordinal);

        for (var row = 0; row < reader.RowCount; row++)
        {
            // Only bestiary and harvest monsters are used: checked before allocating anything.
            var rawId = reader.Text(row, idOffset);
            if (rawId.StartsWith(ExportRules.MetaMonster, StringComparison.Ordinal))
            {
                rawId = rawId[ExportRules.MetaMonster.Length..];
            }
            if (!rawId.StartsWith(ExportRules.LeagueBestiary, StringComparison.Ordinal)
                && !rawId.StartsWith(ExportRules.LeagueHarvest, StringComparison.Ordinal))
            {
                continue;
            }

            var id = reader.String(row, idOffset).Replace(ExportRules.MetaMonster, string.Empty, StringComparison.Ordinal);
            BaseResultData? english = null;
            if (_monstersEn is not null && !_monstersEn.TryGetValue(id, out english))
            {
                continue;
            }

            var name = reader.String(row, nameOffset);
            if (!names.Add(name))
            {
                continue;
            }
            list.Add(new BaseResultData
            {
                Id = id,
                Name = name,
                NameEn = english is null ? name : english.Name
            });
        }

        RemoveDuplicatedMonster(list);
        if (list.Count is 0)
        {
            return false;
        }
        _monstersEn ??= IndexFirst(list, static m => m.Id);
        Write<BaseData>(new BaseData { Result = [new BaseResult { Data = [.. list] }] }, jsonPath);
        return true;
    }

    /// <summary>When both versions of the duplicated monster are present, only the bestiary one is kept.</summary>
    private static void RemoveDuplicatedMonster(List<BaseResultData> list)
    {
        var count = 0;
        foreach (var monster in list)
        {
            if (monster.Id is ExportRules.DuplicatedMonster or ExportRules.DuplicatedMonsterKept)
            {
                count++;
            }
        }
        if (count is 2)
        {
            list.RemoveAll(static m => m.Id is ExportRules.DuplicatedMonster);
        }
    }

    private bool ExportWords(TableReader reader, string jsonPath)
    {
        int textOffset = reader.Offset("Text"), text2Offset = reader.Offset("Text2");
        List<WordResultData> list = [];
        HashSet<string> names = new(StringComparer.Ordinal);

        for (var row = 0; row < reader.RowCount; row++)
        {
            var name = ExportRules.ParseMultipleName(reader.String(row, text2Offset));
            if (!names.Add(name))
            {
                continue;
            }
            list.Add(new WordResultData
            {
                Id = row,
                Name = name,
                NameEn = reader.String(row, textOffset).Trim()
            });
        }

        if (list.Count is 0)
        {
            return false;
        }
        Write<WordData>(new WordData { Result = [new WordResult { Data = [.. list] }] }, jsonPath);
        return true;
    }

    private bool ExportGems(TableReader reader, string jsonPath)
    {
        int idOffset = reader.Offset("Id"), nameOffset = reader.Offset("Name");
        List<GemResultData> list = [];
        HashSet<string> names = new(StringComparer.Ordinal);

        for (var row = 0; row < reader.RowCount; row++)
        {
            var name = reader.String(row, nameOffset).Trim();
            if (name.Length is 0 || name.Contains(ExportRules.NameUnwanted, StringComparison.Ordinal))
            {
                continue;
            }

            var id = reader.String(row, idOffset).Trim();
            GemResultData? english = null;
            if (_gemsEn is not null && !_gemsEn.TryGetValue(id, out english))
            {
                continue;
            }

            var delimiter = id.LastIndexOf(ExportRules.GemDelimiter, StringComparison.Ordinal);
            if (delimiter < 0)
            {
                throw new InvalidDataException($"Gem effect id without '{ExportRules.GemDelimiter}': {id}");
            }

            var gem = new GemResultData
            {
                Id = id,
                Name = name,
                NameEn = english is null ? name : english.Name,
                // English keeps an empty english type, as the historical generator did.
                TypeEn = english is null ? string.Empty : english.Type,
                Type = FindGemType(id, delimiter),
                Disc = id[delimiter..].ToLowerInvariant().Insert(ExportRules.GemDelimiter.Length, "_")
            };
            if (!names.Add(name))
            {
                continue;
            }
            list.Add(gem);
        }

        if (list.Count is 0)
        {
            return false;
        }
        _gemsEn ??= IndexFirst(list, static g => g.Id);
        Write<GemData>(new GemData { Result = [new GemResult { Data = [.. list] }] }, jsonPath);
        return true;
    }

    /// <summary>Name of the base whose id ends with the gem id (without its "Alt" part), otherwise a readable id.</summary>
    private string FindGemType(string id, int delimiter)
    {
        var shortId = id.AsSpan(0, delimiter);
        if (shortId.IsEmpty)
        {
            return string.Empty;
        }
        foreach (var gemBase in _bases)
        {
            if (gemBase.Id.AsSpan().EndsWith(shortId, StringComparison.Ordinal))
            {
                return gemBase.Name ?? ExportRules.GemTypeFromId(id);
            }
        }
        return ExportRules.GemTypeFromId(id);
    }

    /// <summary>Key => first entry with this key.</summary>
    private static Dictionary<TKey, T> IndexFirst<TKey, T>(List<T> list, Func<T, TKey> key) where TKey : notnull
    {
        var index = new Dictionary<TKey, T>(list.Count);
        foreach (var item in list)
        {
            index.TryAdd(key(item), item);
        }
        return index;
    }

    /// <summary>
    /// Same serializer and encoding as the historical generator: UTF-8 with byte order mark.
    /// The json stays in UTF-8 bytes, no intermediate string. The previous file is compared first (see <see cref="JsonChanges"/>).
    /// </summary>
    private void Write<T>(T data, string path) where T : class
        => changes.Write(outputDirectory, path, _json.SerializeToUtf8<T>(data));
}
