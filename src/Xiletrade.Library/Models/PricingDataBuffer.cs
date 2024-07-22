using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.Models;

internal sealed class PricingDataBuffer //MAYBE TO REDO
{
    internal ResultData DataToFetchDetail { get; set; } = null;
    internal string[] ExchangeCurrency { get; set; } = null;
    internal double NinjaChaosEqGet { get; set; }
    internal double NinjaChaosEqPay { get; set; }
    internal int[] StatsFetchBulk { get; set; } = new int[5];
    internal int[] StatsFetchDetail { get; set; } = new int[5];
}
