using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xiletrade.Library.Services.Interface;

namespace Xiletrade.UI.Avalonia.Services;

// not async for now
internal sealed class ClipboardAdapterService : IClipboardAdapterService
{
    private static readonly Lock _clipboardLock = new();

    private static IClipboard Clipboard => GetClipboard();

    private static IClipboard GetClipboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainView = desktop.MainWindow;
            if (mainView is not null)
                return mainView.Clipboard;
        }

        throw new InvalidOperationException("Clipboard not available. Make sure the main window is initialized.");
    }

    public bool ContainsTextData()
    {
        lock (_clipboardLock)
        {
            var text = Clipboard.TryGetTextAsync().GetAwaiter().GetResult();
            return !string.IsNullOrEmpty(text);
        }
    }

    public bool ContainsUnicodeTextData() => ContainsTextData();

    public void Clear()
    {
        lock (_clipboardLock)
        {
            Clipboard.ClearAsync().GetAwaiter().GetResult();
        }
    }

    public void SetClipboard(string data)
    {
        if (data is null)
            return;

        lock (_clipboardLock)
        {
            Clipboard.SetTextAsync(data).GetAwaiter().GetResult();
        }
    }

    public string GetClipboard(bool clear)
    {
        lock (_clipboardLock)
        {
            var text = Clipboard.TryGetTextAsync().GetAwaiter().GetResult() ?? string.Empty;

            if (clear)
            {
                Clipboard.ClearAsync().GetAwaiter().GetResult();
            }

            return text;
        }
    }
}