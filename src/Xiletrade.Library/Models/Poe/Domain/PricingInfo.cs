using System;
using System.Collections.Generic;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class PricingInfo
{
    internal List<string>[] Entity {  get; private set; }
    internal string League { get; private set; }
    internal string Market { get; private set; }
    internal int MinimumStock { get; private set; }
    internal int MaximumFetch { get; private set; }
    internal bool HideSameUser { get; private set; }

    internal bool IsSimpleBulk { get; private set; }
    internal bool IsTradeEntity { get; private set; }
    internal bool IsExchangeEntity { get; private set; }

    /// <summary>Json of the search, in UTF-8.</summary>
    internal ReadOnlyMemory<byte> TradeJson { get; private set; }
    internal string[] ExchangeCurrency { get { return IsExchangeEntity ? [Entity[0][0], Entity[1][0]] : null; } }
    internal string[] ExchangeHave { get { return IsExchangeEntity ? [.. Entity[0]] : null; } }
    internal string[] ExchangeWant { get { return IsExchangeEntity ? [.. Entity[1]] : null; } }

    /// <summary>Search of items.</summary>
    internal PricingInfo(ReadOnlyMemory<byte> tradeJson, string league, string market
        , int maximumFetch, bool hideSameUser)
    {
        TradeJson = tradeJson;
        League = league;
        Market = market;
        MinimumStock = 1;
        MaximumFetch = maximumFetch;
        HideSameUser = hideSameUser;
        IsTradeEntity = true;
    }

    /// <summary>Exchange of currencies.</summary>
    internal PricingInfo(List<string>[] entity, string league, string market
        , int minimumStock, int maximumFetch, bool hideSameUser, bool simpleBulk)
    {
        Entity = entity;
        League = league;
        Market = market;
        MinimumStock = minimumStock;
        MaximumFetch = maximumFetch;
        HideSameUser = hideSameUser;
        IsSimpleBulk = simpleBulk;
        IsExchangeEntity = Entity[0]?.Count >= 1 && Entity[1]?.Count >= 1;
    }
}
