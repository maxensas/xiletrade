using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.WPF.Services;

/// <summary>
/// Service used to access System.Windows.Clipboard
/// </summary>
internal sealed class ClipboardAdapterService : IClipboardAdapterService
{
    private const int MaxAttempts = 15;
    private const int DelayMs = 5;

    public bool ContainsTextData() => Retry(() => Clipboard.ContainsData(DataFormats.Text));

    public bool ContainsUnicodeTextData()
        => Retry(() => Clipboard.ContainsData(DataFormats.UnicodeText));

    public void Clear() => Retry(() => Clipboard.Clear());

    public void SetClipboard(string data) =>  Retry(() => Clipboard.SetText(data));

    public string GetClipboard(bool clear)
    {
        IDataObject iData = Retry(() => Clipboard.GetDataObject());

        string cb = ContainsUnicodeTextData()
            ? (string)iData.GetData(DataFormats.UnicodeText)
            : (string)iData.GetData(DataFormats.Text);

        if (clear)
        {
            Clear();
        }
        return cb;
    }

    // === UTILITY ===
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
