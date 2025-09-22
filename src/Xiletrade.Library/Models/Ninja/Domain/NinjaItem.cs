using Xiletrade.Library.Models.Ninja.Contract;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed class NinjaItem(string name) : CachedNinjaItem<NinjaItemContract>
{
    internal string Name { get; } = name;

    public override object GetJson() => Json;
}