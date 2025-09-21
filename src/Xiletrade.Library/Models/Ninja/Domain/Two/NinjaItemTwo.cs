using System;
using Xiletrade.Library.Models.Ninja.Contract.Two;

namespace Xiletrade.Library.Models.Ninja.Domain.Two;

internal class NinjaItemTwo(string name)
{
    internal string Name { get; } = name;
    internal DateTime Creation { get; set; } = new();
    internal NinjaItemApi Json { get; set; } = new();
}
