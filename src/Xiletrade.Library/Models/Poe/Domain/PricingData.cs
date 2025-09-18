using Xiletrade.Library.Models.Ninja.Domain;
using Xiletrade.Library.Models.Poe.Contract;

namespace Xiletrade.Library.Models.Poe.Domain;

internal sealed class PricingData
{
    internal ResultData ResultData { get; set; } = null;
    internal ResultBar ResultBar { get; set; } = null;
    internal NinjaEquivalence NinjaEq { get; set; } = new();
    internal StatFetch StatBulk { get; set; }
    internal StatFetch StatDetail { get; set; }
}
