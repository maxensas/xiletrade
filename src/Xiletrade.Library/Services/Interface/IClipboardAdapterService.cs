namespace Xiletrade.Library.Services.Interface;

/// <summary>
/// Provides a platform-specific interface for clipboard operations.
/// </summary>
public interface IClipboardAdapterService
{
    public void Clear();
    public void SetClipboard(string data);
    public string GetClipboard(bool clear);
    public bool ContainsUnicodeTextData();
    public bool ContainsTextData();
}
