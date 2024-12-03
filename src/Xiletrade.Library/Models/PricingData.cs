using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.Models;

internal sealed class PricingData
{
    internal ResultData DataToFetchDetail { get; set; } = null;
    internal string[] ExchangeCurrency { get; set; } = null;
    internal double NinjaChaosEqGet { get; set; } = -1;
    internal double NinjaChaosEqPay { get; set; } = -1;
    internal int[] StatsFetchBulk { get; set; } = new int[5];
    internal int[] StatsFetchDetail { get; set; } = new int[5];
}
