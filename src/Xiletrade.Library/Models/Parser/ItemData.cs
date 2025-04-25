using System;
using System.Globalization;
using System.Linq;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Parser;

internal sealed class ItemData
{
    internal ItemFlag Flag { get; }
    internal ItemBase Base { get; } = new();

    internal string[] Data { get; }
    internal string Class { get; }
    internal string Rarity { get; }
    internal string Name { get; set; }
    internal string Type { get; set; }

    internal ItemData(string[] clipData)
    {
        Data = clipData[0].Trim().Split(Strings.CRLF, StringSplitOptions.None);
        Class = Data[0].Split(':')[1].Trim();
        var rarityPrefix = Data[1].Split(':');
        Rarity = rarityPrefix.Length > 1 ? rarityPrefix[1].Trim() : string.Empty;

        Name = Data.Length > 3 && Data[2].Length > 0 ? Data[2] ?? string.Empty : string.Empty;
        Type = Data.Length > 3 && Data[3].Length > 0 ? Data[3] ?? string.Empty
            : Data.Length > 2 && Data[2].Length > 0 ? Data[2] ?? string.Empty
            : Data.Length > 1 && Data[1].Length > 0 ? Data[1] ?? string.Empty
            : string.Empty;

        if (DataManager.Config.Options.DevMode && DataManager.Config.Options.Language is not 0)
        {
            var tuple = GetTranslatedItemNameAndType(Name, Type);
            Name = tuple.Item1;
            Type = tuple.Item2;
        }

        Flag = new ItemFlag(clipData, Rarity, Type, Class);
        if (Flag.ScourgedMap)
        {
            Type = Type.Replace(Resources.Resources.General103_Scourged, string.Empty).Trim();
        }
    }

    /// <summary>
    /// Fix for item name/type not translated for non-english.
    /// </summary>
    /// <remarks>
    /// Only for unit tests in dev mode, not optimized.
    /// </remarks>
    /// <param name="itemName"></param>
    /// <param name="itemType"></param>
    /// <returns></returns>
    private static Tuple<string, string> GetTranslatedItemNameAndType(string itemName, string itemType)
    {
        string name = itemName;
        string type = itemType;

        if (name.Length > 0)
        {
            var word = DataManager.Words.FirstOrDefault(x => x.NameEn == name);
            if (word is not null && !word.Name.Contain('/'))
            {
                name = word.Name;
            }
        }

        var resultName =
                    from result in DataManager.Bases
                    where result.NameEn.Length > 0
                    && type.Contain(result.NameEn)
                    && !result.Id.StartWith("Gems")
                    select result.NameEn;
        if (resultName.Any())
        {
            string longestName = string.Empty;
            foreach (var result in resultName)
            {
                if (result.Length > longestName.Length)
                {
                    longestName = result;
                }
            }
            type = longestName;
        }

        var baseType = DataManager.Bases.FirstOrDefault(x => x.NameEn == type);
        if (baseType is not null && !baseType.Name.Contain('/'))
        {
            type = baseType.Name;
        }

        if (type == itemType)
        {
            bool isMap = type.Contain("Map");
            if (isMap)
            {
                CultureInfo cultureEn = new(Strings.Culture[0]);
                System.Resources.ResourceManager rm = new(typeof(Resources.Resources));
                type = type.Replace(rm.GetString("General040_Blighted", cultureEn), string.Empty)
                    .Replace(rm.GetString("General100_BlightRavaged", cultureEn), string.Empty).Trim();
            }

            var enCur =
                    from result in DataManager.CurrenciesEn
                    from Entrie in result.Entries
                    where isMap ? Entrie.Text.Contain(type) : Entrie.Text == type
                    select Entrie.Id;
            if (enCur.Any())
            {
                var cur = from result in DataManager.Currencies
                          from Entrie in result.Entries
                          where Entrie.Id == enCur.First()
                          select Entrie.Text;
                if (cur.Any())
                {
                    type = cur.First();
                    if (isMap)
                    {
                        type = type.Substring(0, type.IndexOf('(')).Trim();
                    }
                }
            }
        }

        if (type == itemType)
        {
            var findGem = DataManager.Gems.FirstOrDefault(x => x.NameEn == type);
            if (findGem is not null)
            {
                type = findGem.Name;
            }
        }

        return new Tuple<string, string>(name, type);
    }
}
