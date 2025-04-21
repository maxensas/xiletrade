using System.Collections.Generic;

namespace Xiletrade.Library.Models;

internal sealed class CurrencyFetch
{
    internal Dictionary<string, int> ListCur { get; set; } = new();
    internal int Total { get; set; } = 0;
    internal int Unpriced { get; set; } = 0;

    internal void Add(CurrencyFetch cur)
    {
        foreach (var dic in cur.ListCur)
        {
            if (ListCur.TryGetValue(dic.Key, out int value))
                ListCur[dic.Key] = ++value;
            else
                ListCur.Add(dic.Key, 1);
        }
        Total += cur.Total;
        Unpriced += cur.Unpriced;
    }
}
