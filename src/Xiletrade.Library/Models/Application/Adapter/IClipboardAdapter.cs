namespace Xiletrade.Library.Models.Application.Adapter;

/// <summary>
/// Provides a platform-specific interface for clipboard operations.
/// </summary>
public interface IClipboardAdapter
{
    public void Clear();
    public void SetClipboard(string data);
    public string GetClipboard(bool clear);
    public bool ContainsUnicodeTextData();
    public bool ContainsTextData();
}
