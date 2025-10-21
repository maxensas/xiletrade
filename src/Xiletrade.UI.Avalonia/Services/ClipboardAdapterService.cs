using Avalonia.Input.Platform;
using System;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.Avalonia.Services;

public sealed class ClipboardAdapterService : IClipboardAdapterService
{
    private readonly IClipboard _clipboard;

    public ClipboardAdapterService(IClipboard clipboard)
    {
        _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
    }

    public void Clear()
    {
        _clipboard.ClearAsync().GetAwaiter().GetResult();
    }

    public void SetClipboard(string data)
    {
        _clipboard.SetTextAsync(data).GetAwaiter().GetResult();
    }

    public string GetClipboard(bool clear)
    {
        var text = _clipboard.GetTextAsync().GetAwaiter().GetResult() ?? string.Empty;

        if (clear)
        {
            Clear();
        }

        return text;
    }

    public bool ContainsTextData()
    {
        var text = _clipboard.GetTextAsync().GetAwaiter().GetResult();
        return !string.IsNullOrEmpty(text);
    }

    public bool ContainsUnicodeTextData()
    {
        // Treated the same as ContainsTextData in this Avalonia implementation
        return ContainsTextData();
    }
}
