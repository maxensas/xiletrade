using System;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.Models;

internal sealed class NinjaCurrency
{
    internal DateTime Creation { get; set; } = new();
    internal NinjaCurrencyContract Json { get; set; } = new();
}
