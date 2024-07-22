using System;
using Xiletrade.Library.Models.Serializable;

namespace Xiletrade.Library.Models;

internal sealed class NinjaItem
{
    internal DateTime Creation { get; set; } = new();
    internal NinjaItemContract Json { get; set; } = new();
}
