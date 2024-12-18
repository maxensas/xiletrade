using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Xiletrade.Library.Services;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Serializable;

public sealed class JsonDataTwo
{
    [JsonPropertyName("query")]
    public QueryTwo Query { get; set; } = new();

    [DataMember(Name = "sort")]
    [JsonPropertyName("sort")]
    public Sort Sort { get; set; } = new();

    internal JsonDataTwo(XiletradeItem xiletradeItem, ItemBaseName currentItem, bool useSaleType, string market)
    {
        OptionTxt optTrue = new("true"), optFalse = new("false");

        bool errorsFilters = false;
        string Inherit = currentItem.Inherits.Length > 0 ? currentItem.Inherits[0] : string.Empty;
        string Inherit2 = currentItem.Inherits.Length > 1 ? currentItem.Inherits[1] : string.Empty;

        Query.Stats = [];
        Query.Status = new(market);
        Sort.Price = "asc";
        Query.Filters.Trade.Disabled = DataManager.Config.Options.SearchBeforeDay == 0;

        if (DataManager.Config.Options.SearchBeforeDay != 0)
        {
            Query.Filters.Trade.Filters.Indexed = new(BeforeDayToString(DataManager.Config.Options.SearchBeforeDay));
        }
        if (useSaleType)
        {
            Query.Filters.Trade.Filters.SaleType = new("priced");
        }



        //TODO



        if (errorsFilters)
        {
            int errorCount = 0;
            List<int> errors = new();
            for (int i = 0; i < xiletradeItem.ItemFilters.Count; i++)
            {
                if (xiletradeItem.ItemFilters[i].IsNull)
                {
                    errorCount++;
                    errors.Add(i + 1);
                }
            }
            throw new Exception(string.Format("{0} Mod error(s) detected: \r\n\r\nMod lines : {1}\r\n\r\n", errorCount, errors.ToString()));
        }
    }

    private static string BeforeDayToString(int day)
    {
        if (day < 3) return "1day";
        if (day < 7) return "3days";
        if (day < 14) return "1week";
        return "2weeks";
    }

    private static string GetEnglishRarity(string rarityLang)
    {
        System.Globalization.CultureInfo cultureEn = new(Strings.Culture[0]);
        System.Resources.ResourceManager rm = new(typeof(Resources.Resources));

        return rarityLang == Resources.Resources.General005_Any ? rm.GetString("General005_Any", cultureEn) :
            rarityLang == Resources.Resources.General110_FoilUnique ? rm.GetString("General110_FoilUnique", cultureEn) :
            rarityLang == Resources.Resources.General006_Unique ? rm.GetString("General006_Unique", cultureEn) :
            rarityLang == Resources.Resources.General007_Rare ? rm.GetString("General007_Rare", cultureEn) :
            rarityLang == Resources.Resources.General008_Magic ? rm.GetString("General008_Magic", cultureEn) :
            rarityLang == Resources.Resources.General009_Normal ? rm.GetString("General009_Normal", cultureEn) :
            rarityLang == Resources.Resources.General010_AnyNU ? rm.GetString("General010_AnyNU", cultureEn) : string.Empty;
    }

    private static string GetAffixType(string inputType)
    {
        return inputType is "explicit" ? Resources.Resources.General015_Explicit :
            inputType is "implicit" ? Resources.Resources.General013_Implicit :
            inputType is "enchant" ? Resources.Resources.General011_Enchant :
            inputType is "rune" ? "Rune" :
            inputType is "sanctum" ? Resources.Resources.General111_Sanctum :
            inputType is "skill" ? "Skill" : string.Empty;
    }
}
