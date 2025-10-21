using System.Threading;
using System.Windows;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.WPF.Services;

/// <summary>
/// Service used to access System.Windows.Clipboard with STA constraint.
/// </summary>
internal sealed class ClipboardAdapterService : IClipboardAdapterService
{
    private static readonly Lock _clipboardLock = new();

    public bool ContainsTextData()
    {
        lock (_clipboardLock)
        {
            return Clipboard.ContainsData(DataFormats.Text);
        }
    }

    public bool ContainsUnicodeTextData()
    {
        lock (_clipboardLock)
        {
            return Clipboard.ContainsData(DataFormats.UnicodeText);
        }
    }

    public void Clear()
    {
        lock (_clipboardLock)
        {
            Clipboard.Clear();
        }
    }

    public void SetClipboard(string data)
    {
        lock (_clipboardLock)
        {
            Clipboard.SetText(data);
        }
    }

    public string GetClipboard(bool clear)
    {
        lock (_clipboardLock)
        {
            IDataObject iData = Clipboard.GetDataObject();

            string cb = Clipboard.ContainsData(DataFormats.UnicodeText)
                ? (string)iData.GetData(DataFormats.UnicodeText)
                : (string)iData.GetData(DataFormats.Text);

            if (clear)
            {
                Clipboard.Clear();
            }

            return cb;
        }
    }
}
