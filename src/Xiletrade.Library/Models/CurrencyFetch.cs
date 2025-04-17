using System.Collections.Generic;

namespace Xiletrade.Library.Models;

internal sealed class CurrencyFetch
{
    internal Dictionary<string, int> ListCur { get; set; } = new();
    internal int Total { get; set; } = 0;
    internal int Unpriced { get; set; } = 0;
}
