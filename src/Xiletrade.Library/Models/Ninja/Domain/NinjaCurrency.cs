using System;
using Xiletrade.Library.Models.Ninja.Contract;

namespace Xiletrade.Library.Models.Ninja.Domain;

internal sealed class NinjaCurrency
{
    internal DateTime Creation { get; set; } = new();
    internal NinjaCurrencyContract Json { get; set; } = new();
}
