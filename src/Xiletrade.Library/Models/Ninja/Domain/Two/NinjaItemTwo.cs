using Xiletrade.Library.Models.Ninja.Contract.Two;

namespace Xiletrade.Library.Models.Ninja.Domain.Two;

internal sealed class NinjaItemTwo(string name) : CachedNinjaItem<NinjaItemApi>
{
    internal string Name { get; } = name;

    public override object GetJson() => Json;
}
