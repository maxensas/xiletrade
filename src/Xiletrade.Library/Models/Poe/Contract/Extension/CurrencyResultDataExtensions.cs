using System;
using System.Collections.Generic;
using Xiletrade.Library.Models.Application.Configuration.DTO;
using Xiletrade.Library.Models.Application.Configuration.DTO.Extension;
using Xiletrade.Library.Shared;

namespace Xiletrade.Library.Models.Poe.Contract.Extension;

internal static class CurrencyResultDataExtensions
{
    internal static CurrencyEntrie FindEntryById(this CurrencyResultData[] curDatas, 
        ReadOnlySpan<char> curId)
    {
        foreach (var curData in curDatas)
        {
            if (curData.Entries is null)
                continue;

            foreach (var entry in curData.Entries)
            {
                if (entry.Id.AsSpan().SequenceEqual(curId))
                {
                    return entry;
                }
            }
        }
        return null;
    }

    internal static CurrencyEntrie FindEntryByType(this CurrencyResultData[] curDatas, 
        ReadOnlySpan<char> type)
    {
        foreach (var curData in curDatas)
        {
            if (curData.Entries is null)
                continue;

            foreach (var entry in curData.Entries)
            {
                if (entry.Text.AsSpan().SequenceEqual(type))
                {
                    return entry;
                }
            }
        }
        return null;
    }

    internal static CurrencyEntrie FindEntryByType(this CurrencyResultData[] currencies,
        ReadOnlySpan<char> type, bool isMap)
    {
        foreach (var currency in currencies)
        {
            if (currency.Entries is null)
                continue;

            foreach (var entry in currency.Entries)
            {
                if (entry.Text is null)
                    continue;

                if (isMap ? entry.Text.AsSpan().Contain(type)
                        : entry.Text.AsSpan().SequenceEqual(type))
                {
                    return entry;
                }
            }
        }

        return null;
    }

    // TODO: maybe merge with other helper method
    internal static CurrencyEntrie FindEntryByTypeAndPossibleMapKind(
        this CurrencyResultData[] currencies,
        ReadOnlySpan<char> type, ReadOnlySpan<char> mapKind)
    {
        foreach (var currency in currencies)
        {
            if (currency.Entries is null || (mapKind.Length > 0 && currency.Id != mapKind))
                continue;

            foreach (var entry in currency.Entries)
            {
                if (entry.Text is null)
                    continue;

                if (entry.Text.AsSpan().SequenceEqual(type))
                {
                    return entry;
                }
            }
        }

        return null;
    }

    internal static CurrencyEntrie FindEntryByTypeAndEndId(
        this CurrencyResultData[] curDatas, 
        ReadOnlySpan<char> type, ReadOnlySpan<char> endId)
    {
        foreach (var curData in curDatas)
        {
            if (curData.Entries is null)
                continue;

            foreach (var entry in curData.Entries)
            {
                if (!entry.Text.AsSpan().Contain(type))
                    continue;

                if (entry.Id.AsSpan().EndWith(endId))
                {
                    return entry;
                }
            }
        }

        return null;
    }

    internal static (CurrencyEntrie Entry, string GroupId) FindEntryAndGroupIdByType(
        this CurrencyResultData[] curDatas, ReadOnlySpan<char> type, bool image = true)
    {
        foreach (var curData in curDatas)
        {
            if (curData.Entries is null || curData.Label is null)
                continue;

            foreach (var entry in curData.Entries)
            {
                if (entry.Id is Strings.sep)
                {
                    continue;
                }
                if (entry.Text.AsSpan().SequenceEqual(type))
                {
                    if (image && (entry.Img is null || entry.Img.Length is 0))
                    {
                        return (null, curData.Id);
                    }
                    return (entry, curData.Id);
                }
            }
        }
        return (null, string.Empty);
    }

    internal static (CurrencyEntrie Entry, string GroupId) FindEntryAndGroupIdByTypeOrId(
        this CurrencyResultData[] curDatas, ReadOnlySpan<string> searchWords)
    {
        foreach (var curData in curDatas)
        {
            if (curData.Entries is null || curData.Label is null)
                continue;

            foreach (var entry in curData.Entries)
            {
                if (entry.Id is Strings.sep)
                    continue;

                bool allWordsMatch = true;

                foreach (ReadOnlySpan<char> word in searchWords)
                {
                    if (!entry.Text.AsSpan().Contains(word, StringComparison.OrdinalIgnoreCase) &&
                        !entry.Id.AsSpan().Contains(word, StringComparison.OrdinalIgnoreCase))
                    {
                        allWordsMatch = false;
                        break;
                    }
                }

                if (allWordsMatch)
                {
                    return (entry, curData.Id);
                }
            }
        }

        return (null, string.Empty);
    }

    internal static (CurrencyEntrie Entry, string ID) FindEntryAndGroupIdByCurId(
        this CurrencyResultData[] curDatas, ReadOnlySpan<char> id, 
        bool withImage = true, bool noCard = false, bool noMap = false)
    {
        foreach (var curData in curDatas)
        {
            if (curData.Entries is null || curData.Label is null)
                continue;

            if (noCard && curData.Id is Strings.CurrencyTypePoe1.Cards)
                continue;

            if (noMap && curData.Id.Contain(Strings.Maps))
                continue;

            foreach (var entry in curData.Entries)
            {
                if (entry.Id.AsSpan().SequenceEqual(id))
                {
                    if (withImage && (entry.Img is null || entry.Img.Length is 0))
                    {
                        return (null, curData.Id);
                    }
                    return (entry, curData.Id);
                }
            }
        }
        return (null, string.Empty);
    }

    internal static CurrencyEntrie FindMapEntryByType(this CurrencyResultData[] curDatas,
        ReadOnlySpan<char> type, ReadOnlySpan<char> mapKind)
    {
        foreach (var curData in curDatas)
        {
            if (!curData.Id.AsSpan().SequenceEqual(mapKind))
                continue;

            if (curData.Entries is null)
                continue;

            foreach (var entry in curData.Entries)
            {
                var textSpan = entry.Text.AsSpan();
                if (textSpan.StartsWith(type) || textSpan.EndsWith(type))
                {
                    return entry;
                }
            }
        }

        return null;
    }

    internal static List<string> GetCurrenciesList(this CurrencyResultData[] currencies,
        DivTiersResult[] divTiers, string searchKind, string selValue, string exchangeTier, 
        bool isDelve = false)
    {
        var returnList = new List<string>();
        foreach (var currencie in currencies)
        {
            if (currencie.Entries is null)
                continue;

            if (isDelve ? !currencie.Id.Contain(searchKind)
                : !currencie.Id.Equal(searchKind))
            {
                continue;
            }

            foreach (var entry in currencie.Entries)
            {
                if (entry.Text.Length is 0 || entry.Id is Strings.sep)
                    continue;

                bool addItem = false;

                if (searchKind is Strings.CurrencyTypePoe1.Maps
                        or Strings.CurrencyTypePoe1.MapsUnique
                        or Strings.CurrencyTypePoe1.MapsBlighted)
                {
                    if (exchangeTier.Length > 0)
                    {
                        string tier = Strings.tierPrefix + exchangeTier.Replace("T", string.Empty);
                        addItem = entry.Id.EndWith(tier);
                    }
                }
                else if (searchKind is Strings.CurrencyTypePoe1.Cards)
                {
                    var tmpDiv = divTiers.FindDivTierByTag(entry.Id);
                    if (tmpDiv is not null)
                    {
                        if (exchangeTier.Length > 0)
                        {
                            string tierVal = exchangeTier.ToLowerInvariant().Replace("t", string.Empty);
                            addItem = tierVal == tmpDiv.Tier;
                        }
                    }
                    else
                    {
                        if (exchangeTier.Length > 0)
                        {
                            addItem = exchangeTier
                                .Equals(Resources.Resources.Main016_TierNothing, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                }
                else if (searchKind is Strings.CurrencyTypePoe1.Currency)
                {
                    bool is_mainCur = Strings.dicMainCur.TryGetValue(entry.Id, out string curVal2);
                    bool is_exoticCur = Strings.dicExoticCur.TryGetValue(entry.Id, out string curVal3);
                    addItem = selValue == Resources.Resources.Main044_MainCur ? is_mainCur && !is_exoticCur
                            : selValue == Resources.Resources.Main207_ExoticCurrency ? !is_mainCur && is_exoticCur
                            : selValue == Resources.Resources.Main045_OtherCur ? !is_mainCur && !is_exoticCur
                            : addItem;
                }
                else if (searchKind is Strings.CurrencyTypePoe1.Fragments)
                {
                    bool is_scarab = entry.Id.Contain(Strings.scarab);
                    bool is_stone = Strings.dicStones.TryGetValue(entry.Id, out string stoneVal);
                    addItem = selValue == Resources.Resources.Main047_Stones ? (is_stone && !is_scarab)
                            : selValue == Resources.Resources.Main046_MapFrag ? (!is_stone && !is_scarab)
                            : selValue == Resources.Resources.Main052_Scarabs ? (!is_stone && is_scarab)
                            : addItem;
                }
                else
                {
                    addItem = true;
                }

                if (addItem)
                    returnList.Add(entry.Text);
            }
        }

        return returnList;
    }
}
