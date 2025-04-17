using System.Collections.Generic;

namespace Xiletrade.Library.Models;

internal sealed class PricingInfo
{
    internal List<string>[] Entity {  get; private set; }
    internal string League { get; private set; }
    internal string Market { get; private set; }
    internal int MinimumStock { get; private set; }
    internal int MaximumFetch { get; private set; }
    internal bool HideSameUser { get; private set; }

    internal PricingInfo(List<string>[] entity, string league, string market
        , int minimumStock, int maximumFetch, bool hideSameUser)
    {
        Entity = entity;
        League = league;
        Market = market;
        MinimumStock = minimumStock;
        MaximumFetch = maximumFetch;
        HideSameUser = hideSameUser;
    }
}
