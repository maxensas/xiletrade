using Xiletrade.Library.Models.Ninja.Contract.Exchange;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed class NinjaExchange(string name) : CachedNinjaItem<NinjaExchangeContract>(name)
{
    public override NinjaExchangeContract GetJson() => Json;
}
