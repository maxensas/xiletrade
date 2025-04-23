using System.Threading;
using System.Threading.Tasks;

namespace Xiletrade.Library.Models;

/// <summary>
/// Class used by Xiletrade to handle and launch task objects.
/// </summary>
internal sealed class TaskManager
{
    private static CancellationTokenSource PriceCts { get; set; } = null;
    private static CancellationTokenSource MainUpdaterCts { get; set; } = null;

    internal Task PriceTask { get; set; } = null;
    internal Task NinjaTask { get; set; } = null;
    internal Task MainUpdaterTask { get; set; } = null;

    /// <summary>
    /// Avoid price check spam, previous threads need to end properly
    /// </summary>
    internal void CancelPreviousTasks()
    {
        if (MainUpdaterTask is not null && !MainUpdaterTask.IsCompleted)
        {
            MainUpdaterCts?.Cancel();
        }
        if (PriceTask is not null && !PriceTask.IsCompleted)
        {
            PriceCts?.Cancel();
        }
        MainUpdaterTask?.Wait();
        PriceTask?.Wait();
        MainUpdaterCts?.Dispose();
        PriceCts?.Dispose();
    }

    internal CancellationToken GetMainUpdaterToken(bool initCts = false)
    {
        if (initCts)
        {
            MainUpdaterCts = new();
        }
        return MainUpdaterCts.Token;
    }

    internal CancellationToken GetPriceToken(bool initCts = false)
    {
        if (initCts)
        {
            PriceCts = new();
        }
        return PriceCts.Token;
    }
}
