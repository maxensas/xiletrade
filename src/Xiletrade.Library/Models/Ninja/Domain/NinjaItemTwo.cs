using Xiletrade.Library.Models.Ninja.Contract;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed class NinjaItemTwo(string name) : CachedNinjaItem<NinjaItemTwoContract>(name)
{
    public override NinjaItemTwoContract GetJson() => Json;
}
