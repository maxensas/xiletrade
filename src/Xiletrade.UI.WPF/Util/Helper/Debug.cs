using System.Windows;

namespace Xiletrade.UI.WPF.Util.Helper;

/// <summary>Static class used to debug specific Xiletrade code.</summary>
public static class Debug //obsolete
{
    private static System.Diagnostics.Stopwatch CodeWatchUi { get; set; }
    private static System.Diagnostics.Stopwatch CodeWatch { get; set; }

    public static readonly bool DEBUG_TIMERS = false;

    // old code
    public static void StartTimer()
    {
#if DEBUG
        if (Application.Current.Dispatcher.CheckAccess())
        {
            if (DEBUG_TIMERS)
            {
                if (CodeWatchUi == null)
                {
                    CodeWatchUi = System.Diagnostics.Stopwatch.StartNew();
                }
                else
                {
                    CodeWatchUi.Restart();
                }
            }
        }
        else
        {
            if (DEBUG_TIMERS)
            {
                if (CodeWatch == null)
                {
                    CodeWatch = System.Diagnostics.Stopwatch.StartNew();
                }
                else
                {
                    CodeWatch.Restart();
                }
            }
        }

#endif
    }

    public static void StopTimer(string message)
    {
#if DEBUG
        if (Application.Current.Dispatcher.CheckAccess())
        {
            if (DEBUG_TIMERS)
            {
                if (CodeWatchUi != null)
                {
                    if (CodeWatchUi.IsRunning)
                    {
                        CodeWatchUi.Stop();
                        /*if (Price.IsPriceTimeRunning())
                        {
                            Trace("Total elapsed : " + Price.GetElapsedMilliseconds() + "ms | " + message + " : " + CodeWatchUi.ElapsedMilliseconds + "ms");
                        }
                        else
                        {
                            Trace(message + " : " + CodeWatchUi.ElapsedMilliseconds + "ms");
                        }*/
                    }
                }
            }
        }
        else
        {
            if (DEBUG_TIMERS)
            {
                if (CodeWatch != null)
                {
                    if (CodeWatch.IsRunning)
                    {
                        CodeWatch.Stop();
                        /*if (Price.IsPriceTimeRunning())
                        {
                            Trace("Total elapsed : " + Price.GetElapsedMilliseconds() + "ms | " + message + " : " + CodeWatch.ElapsedMilliseconds + "ms");
                        }
                        else
                        {
                            Trace(message + " : " + CodeWatch.ElapsedMilliseconds + "ms");
                        }*/
                    }
                }
            }
        }

#endif
    }

    public static void Trace(string message)
    {
#if DEBUG
        System.Diagnostics.Trace.WriteLine(message);
#endif
    }
}
