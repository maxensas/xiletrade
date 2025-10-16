using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.WPF.Services;

/// <summary>
/// Service used to access System.Windows.Clipboard with STA Threads
/// </summary>
internal sealed class ClipboardAdapterService : IClipboardAdapterService
{
    private const int MaxAttempts = 15;
    private const int DelayMs = 5;

    public bool ContainsTextData() => RunSta(() => Clipboard.ContainsData(DataFormats.Text));

    public bool ContainsUnicodeTextData() 
        => RunSta(() => Clipboard.ContainsData(DataFormats.UnicodeText));

    public void Clear() => RunSta(() => Retry(() => Clipboard.Clear()));

    public void SetClipboard(string data) => RunSta(() => Retry(() => Clipboard.SetText(data)));

    public string GetClipboard(bool clear)
    {
        return RunSta(() =>
        {
            IDataObject iData = Retry(() => Clipboard.GetDataObject());

            string cb = ContainsUnicodeTextData()
                ? (string)iData.GetData(DataFormats.UnicodeText)
                : (string)iData.GetData(DataFormats.Text);

            if (clear)
            {
                Clipboard.Clear(); // or RunSta(() => Retry(() => Clipboard.Clear()));
            }

            return cb;
        });
    }

    // === UTILITY ===
    private static void RunSta(Action action)
    {
        Exception exception = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            throw exception;
        }
    }

    private static T RunSta<T>(Func<T> func)
    {
        T result = default;
        Exception exception = null;

        var thread = new Thread(() =>
        {
            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception is not null)
        {
            throw exception;
        }

        return result;
    }

    private static void Retry(Action action)
    {
        for (int i = 0; i < MaxAttempts; i++)
        {
            try
            {
                action();
                return;
            }
            catch (ExternalException)
            {
                Thread.Sleep(DelayMs);
            }
        }

        throw new Exception("Clipboard operation failed after multiple attempts.");
    }

    private static T Retry<T>(Func<T> func)
    {
        for (int i = 0; i < MaxAttempts; i++)
        {
            try
            {
                return func();
            }
            catch (ExternalException)
            {
                Thread.Sleep(DelayMs);
            }
        }

        throw new Exception("Clipboard operation failed after multiple attempts.");
    }
}
