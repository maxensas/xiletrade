namespace Xiletrade.Library.Models;

internal sealed class StatFetch
{
    internal int Begin { get; set; } = 0;
    internal int ResultLoaded { get; set; } = 0;
    internal int ResultCount { get; set; } = 0;
    internal int Unpriced { get; set; } = 0;
    internal int Total { get; set; } = 0;
}
