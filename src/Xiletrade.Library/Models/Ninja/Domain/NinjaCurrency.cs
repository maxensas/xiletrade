using Xiletrade.Library.Models.Ninja.Contract;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed class NinjaCurrency(string name) : CachedNinjaItem<NinjaCurrencyContract>
{
    internal string Name { get; } = name;

    public override object GetJson() => Json;
}