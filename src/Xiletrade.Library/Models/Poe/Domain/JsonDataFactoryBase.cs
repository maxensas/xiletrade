using System;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Poe.Contract;
using Xiletrade.Library.Models.Poe.Contract.Extension;
using Xiletrade.Library.Models.Poe.Domain.Interface;
using Xiletrade.Library.Models.Poe.Domain.Parser;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Domain;

/// <summary>
/// Abstract class used to generate JSON objects for POE 1 and 2 and allow string serialization.
/// </summary>
/// <typeparam name="T">Concrete JSON contract type (<see cref="JsonData"/> or <see cref="JsonDataTwo"/>).</typeparam>
/// <param name="dm"></param>
internal abstract class JsonDataFactoryBase<T>(DataManagerService dm) : IJsonDataFactory where T : class
{
    protected readonly DataManagerService _dm = dm;

    internal abstract T Create(XiletradeItem xItem, UniqueUnidentified unid, string market, string search);

    internal abstract T Create(XiletradeItem xItem, ItemData item, bool useSaleType, string market);

    public string CreateAndSerialize(XiletradeItem xItem, ItemData item, bool useSaleType, string market)
        => _dm.Json.Serialize<T>(Create(xItem, item, useSaleType, market));

    public string CreateAndSerialize(XiletradeItem xItem, UniqueUnidentified unid, string market, string search)
        => _dm.Json.Serialize<T>(Create(xItem, unid, market, search));

    // Utility
    protected static string GetAffixType(ReadOnlySpan<char> inputType) => inputType switch
    {
        // PoE 1 & 2 common
        Strings.Type.Pseudo => Resources.Resources.General014_Pseudo,
        Strings.Type.Explicit => Resources.Resources.General015_Explicit,
        Strings.Type.Implicit => Resources.Resources.General013_Implicit,
        Strings.Type.Fractured => Resources.Resources.General016_Fractured,
        Strings.Type.Crafted => Resources.Resources.General012_Crafted,
        Strings.Type.Enchant => Resources.Resources.General011_Enchant,

        // PoE 2
        Strings.Type.Augment => Resources.Resources.General145_Augment,
        Strings.Type.Desecrated => Resources.Resources.General158_Desecrated,
        Strings.Type.Skill => Resources.Resources.General144_Skill,

        // PoE 1 less common
        Strings.Type.Veiled => Resources.Resources.General019_Veiled,
        Strings.Type.Monster => Resources.Resources.General018_Monster,
        Strings.Type.Mercenary => Resources.Resources.General243_Mercenary,
        Strings.Type.Delve => Resources.Resources.General020_Delve,
        Strings.Type.Ultimatum => Resources.Resources.General069_Ultimatum,
        Strings.Type.Scourge => Resources.Resources.General099_Scourge,
        Strings.Type.Crucible => Resources.Resources.General112_Crucible,
        Strings.Type.Necropolis => Resources.Resources.General131_Necropolis,
        Strings.Type.Sanctum => Resources.Resources.General111_Sanctum,
        Strings.Type.Imbued => Resources.Resources.General197_ImbuedFilter,

        _ => string.Empty
    };

    protected static OptionTxt GetOptionTrue() => new("true");

    protected static OptionTxt GetOptionFalse() => new("false");

    protected static string BeforeDayToString(int day)
    {
        if (day < 3) return "1day";
        if (day < 7) return "3days";
        if (day < 14) return "1week";
        return "2weeks";
    }

    protected static string GetEnglishRarity(ReadOnlySpan<char> rarityLang)
    {
        var rm = Resources.Resources.ResourceManager;

        string matchedKey = null;
        foreach (var key in Strings.RarityResourceKeys)
        {
            if (rarityLang.SequenceEqual(rm.GetString(key)))
            {
                matchedKey = key;
                break;
            }
        }
        if (matchedKey is null) return string.Empty;

        var english = rm.GetEnglish(matchedKey);

        return english switch
        {
            "Any N-U" => "nonunique",
            "Foil Unique" => "uniquefoil",
            _ => english.ToLowerInvariant()
        };
    }

    protected string GetTradeIdentifier(ReadOnlySpan<char> text)
    {
        if (text.Length is 0)
            return string.Empty;

        var entry = _dm.Items.FindEntryByText(text);
        return !string.IsNullOrEmpty(entry.Type) ? entry.Type : string.Empty;
    }
}
