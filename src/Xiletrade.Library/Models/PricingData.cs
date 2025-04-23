using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.Models;

internal sealed class PricingData
{
    internal ResultData ResultData { get; set; } = null;
    internal ResultBar ResultBar { get; set; } = null;
    internal NinjaEquivalence NinjaEq { get; set; } = new();
    internal StatFetch StatBulk { get; set; }
    internal StatFetch StatDetail { get; set; }
}
