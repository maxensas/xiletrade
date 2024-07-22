using System;

namespace Xiletrade.Library.Models;

internal sealed class PricingWatch
{
    private static readonly System.Diagnostics.Stopwatch Stopwatch = new();

    internal string StopAndGetTimeString()
    {
        Stopwatch.Stop();
        double time = Stopwatch.ElapsedMilliseconds / 10;
        time = Math.Truncate(time) / 100;
        return time + "s";
    }

    internal void Restart()
    {
        Stopwatch.Restart();
    }

    internal bool IsPriceTimeRunning()
    {
        return Stopwatch.IsRunning;
    }

    internal long GetElapsedTimeMs()
    {
        return Stopwatch.ElapsedMilliseconds;
    }
}
