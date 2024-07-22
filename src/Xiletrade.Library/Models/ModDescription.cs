namespace Xiletrade.Library.Models;

internal sealed class ModDescription
{
    internal string Kind { get; set; } = string.Empty;
    internal string Tags { get; set; } = string.Empty;
    internal string Name { get; set; } = string.Empty;
    internal string Quality { get; set; } = string.Empty;
    internal int Tier { get; set; } = -1;
}
