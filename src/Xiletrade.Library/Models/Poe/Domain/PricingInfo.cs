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

    internal string TradeEntity { get { return IsTradeEntity ? Entity[0][0] : null; } }
    internal string[] ExchangeCurrency { get { return IsExchangeEntity ? [Entity[0][0], Entity[1][0]] : null; } }
    internal string[] ExchangeHave { get { return IsExchangeEntity ? [.. Entity[0]] : null; } }
    internal string[] ExchangeWant { get { return IsExchangeEntity ? [.. Entity[1]] : null; } }

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
        IsTradeEntity = Entity[0]?.Count is 1 && Entity[1] is null;
        IsExchangeEntity = Entity[0]?.Count >= 1 && Entity[1]?.Count >= 1;
    }
}
