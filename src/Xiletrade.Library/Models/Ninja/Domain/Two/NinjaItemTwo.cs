using Xiletrade.Library.Models.Ninja.Contract.Two;

namespace Xiletrade.Library.Models.Ninja.Domain.Two;

internal sealed class NinjaItemTwo(string name) : CachedNinjaItem<NinjaItemTwoContract>(name)
{
    public override NinjaItemTwoContract GetJson() => Json;
}
