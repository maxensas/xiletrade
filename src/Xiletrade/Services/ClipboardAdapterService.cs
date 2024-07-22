using System.Windows;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.Services;

internal sealed class ClipboardAdapterService : IClipboardAdapterService
{
    public void Clear() => Clipboard.Clear();

    public void SetClipboard(object data) => Clipboard.SetDataObject(data);

    public bool ContainsTextData()
    {
        return Clipboard.ContainsData(DataFormats.Text);
    }

    public bool ContainsUnicodeTextData()
    {
        return Clipboard.ContainsData(DataFormats.UnicodeText);
    }

    public string GetClipboard(bool clear)
    {
        IDataObject iData = Clipboard.GetDataObject();
        var cb = ContainsUnicodeTextData() 
            ? (string)iData.GetData(DataFormats.UnicodeText) 
            : (string)iData.GetData(DataFormats.Text);
        if (clear)
        {
            Clear();
        }
        return cb;
    }
}
