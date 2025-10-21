namespace Xiletrade.Library.Services.Interface;

public interface IClipboardAdapterService
{
    public void Clear();
    public void SetClipboard(string data);
    public string GetClipboard(bool clear);
    public bool ContainsUnicodeTextData();
    public bool ContainsTextData();
}
