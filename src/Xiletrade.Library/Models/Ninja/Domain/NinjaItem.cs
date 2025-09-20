using System;
using Xiletrade.Library.Models.Ninja.Contract;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed class NinjaItem
{
    internal DateTime Creation { get; set; } = new();
    internal NinjaItemContract Json { get; set; } = new();
}
