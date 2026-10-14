using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xiletrade.Library.Models.Application.Notification;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.UI.Avalonia.Util;

/// <summary>
/// Displays independent desktop toast windows above the taskbar, at the bottom-right of the working area.
/// </summary>
public sealed class NotificationHost : INotificationHost
{
    private readonly IUIService _ui;
    private readonly Action<string> _dismiss;

    private readonly Dictionary<string, Window> _windows = new(StringComparer.Ordinal);
    private readonly List<string> _order = new(); // oldest -> newest, keeps stacking deterministic
    private readonly HashSet<string> _closing = new(StringComparer.Ordinal);

    private readonly ToastOptions _toast = new();

    private bool _disposed;

    public NotificationHost(IUIService ui, Action<string> dismiss)
    {
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        _dismiss = dismiss ?? throw new ArgumentNullException(nameof(dismiss));
    }

    public void OnAdded(object sender, ToastNotification message) => _ui.Invoke(() =>
    {
        if (_windows.Remove(message.Id, out var previous))
        {
            _order.Remove(message.Id);
            previous.Close();
        }

        var body = new StackPanel { Spacing = 5 };

        body.Children.Add(new TextBlock
        {
            Text = message.Title,
            FontSize = _toast.TitleFontSize,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });

        body.Children.Add(new TextBlock
        {
            Text = message.Message,
            FontSize = _toast.MessageFontSize,
            TextWrapping = TextWrapping.Wrap
        });

        var card = new Border
        {
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            Child = body,
            Background = GetStatusBrush(message.Status),
            BoxShadow = new BoxShadows(
                new BoxShadow
                {
                    Blur = 12,
                    Color = Color.FromArgb(70, 0, 0, 0),
                    OffsetY = 3
                }),
            Cursor = new(StandardCursorType.Hand)
        };

        card.Tapped += (_, _) => _dismiss(message.Id);

        // Height is measured once and fixed: no SizeToContent, so no intermediate sizes
        // that could make the window jump while several toasts are opening.
        card.Measure(new Size(_toast.Width, double.PositiveInfinity));
        var height = Math.Ceiling(card.DesiredSize.Height);

        var window = new Window
        {
            Title = message.Title,
            Content = card,
            Width = _toast.Width,
            Height = height,
            SizeToContent = SizeToContent.Manual,
            WindowDecorations = WindowDecorations.None,
            CanResize = false,
            ShowInTaskbar = false,
            Topmost = true,
            ShowActivated = false,
            Background = Brushes.Transparent,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Opacity = 0
        };

        window.Opened += (_, _) =>
        {
            Reposition();
            _ = FadeAsync(window, 0, 1);
        };

        window.Closed += (_, _) =>
        {
            RemoveIfCurrent(message.Id, window);
            window.Content = null;
            Reposition();
        };

        _windows[message.Id] = window;
        _order.Add(message.Id);

        Reposition(); // final position is set before the window is shown
        window.Show();
    });

    public void OnRemoved(object sender, string id) => _ui.Invoke(() =>
    {
        if (!_windows.TryGetValue(id, out var window))
            return;

        if (!_closing.Add(id))
            return;

        _ = CloseWithFadeAsync(id, window);
    });

    private async Task FadeAsync(Window window, double from, double to, int durationMs = 0)
    {
        var duration = durationMs > 0 ? durationMs : _toast.FadeDuration;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        window.Opacity = from;

        while (stopwatch.ElapsedMilliseconds < duration)
        {
            if (!window.IsVisible)
                return;

            var progress = Math.Clamp(stopwatch.Elapsed.TotalMilliseconds / duration, 0.0, 1.0);

            window.Opacity = from + (to - from) * progress;

            await Task.Delay(16);
        }

        if (window.IsVisible)
            window.Opacity = to;
    }

    private async Task CloseWithFadeAsync(string id, Window window)
    {
        try
        {
            await FadeAsync(window, window.Opacity, 0);
        }
        finally
        {
            // window.Close() raises Closed, which removes the entry and repositions the others.
            _closing.Remove(id);
            window.Close();
        }
    }

    private void RemoveIfCurrent(string id, Window window)
    {
        if (_windows.TryGetValue(id, out var current) && ReferenceEquals(current, window))
        {
            _windows.Remove(id);
            _order.Remove(id);
        }
    }

    private void Reposition()
    {
        // Newest first = lowest on screen.
        var windows = Enumerable.Reverse(_order)
            .Select(id => _windows.GetValueOrDefault(id))
            .Where(w => w is not null)
            .Cast<Window>()
            .ToArray();

        if (windows.Length == 0)
            return;

        var screen = windows.Select(w => w.Screens.Primary).FirstOrDefault(s => s is not null);
        if (screen is null)
            return;

        var area = screen.WorkingArea;
        var scale = screen.Scaling;

        var margin = (int)Math.Ceiling(_toast.Margin * scale);
        var gap = (int)Math.Ceiling(_toast.Gap * scale);
        var width = (int)Math.Ceiling(_toast.Width * scale);

        var bottom = area.Bottom - margin;
        var right = area.Right - margin;

        foreach (var window in windows)
        {
            // Fixed values (Width/Height set at creation): independent from layout timing.
            var height = (int)Math.Ceiling(window.Height * scale);

            bottom -= height;

            window.Position = new PixelPoint(right - width, Math.Max(area.Y + margin, bottom));

            bottom -= gap;
        }
    }

    private SolidColorBrush GetStatusBrush(NotificationStatus status)
        => new(Color.Parse(_toast.GetStatusColor(status)));

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _ui.Invoke(() =>
        {
            foreach (var window in _windows.Values.ToArray())
                window.Close();

            _windows.Clear();
            _order.Clear();
            _closing.Clear();
        });
    }
}


/*
public sealed class NotificationHost : INotificationHost
{
    private readonly IUIService _ui;
    private readonly Action<string> _dismiss;

    private readonly Dictionary<string, Window> _windows = new(StringComparer.Ordinal);
    private readonly HashSet<string> _closing = new(StringComparer.Ordinal);
    private readonly ToastOptions _toast = new();
    
    private bool _disposed;

    public NotificationHost(IUIService ui, Action<string> dismiss)
    {
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        _dismiss = dismiss ?? throw new ArgumentNullException(nameof(dismiss));
    }

    public void OnAdded(object sender, ToastNotification message) => _ui.Invoke(() =>
    {
        if (_windows.Remove(message.Id, out var previous))
            previous.Close();

        var body = new StackPanel { Spacing = 5 };

        body.Children.Add(new TextBlock
        {
            Text = message.Title,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });

        body.Children.Add(new TextBlock
        {
            Text = message.Message,
            TextWrapping = TextWrapping.Wrap
        });

        var card = new Border
        {
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(8),
            Child = body,
            Background = GetStatusBrush(message.Status),
            BoxShadow = new BoxShadows(
                new BoxShadow
                {
                    Blur = 12,
                    Color = Color.FromArgb(70, 0, 0, 0),
                    OffsetY = 3
                }),
            Cursor = new(StandardCursorType.Hand)
        };

        card.Tapped += (_, _) => _dismiss(message.Id);

        var window = new Window
        {
            Title = message.Title,
            Content = card,
            Width = _toast.Width,
            SizeToContent = SizeToContent.Height,
            WindowDecorations = WindowDecorations.None,
            CanResize = false,
            ShowInTaskbar = false,
            Topmost = true,
            ShowActivated = false,
            Background = Brushes.Transparent,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Opacity = 0
        };

        window.PropertyChanged += (_, e) =>
        {
            if (e.Property == Visual.BoundsProperty)
                Reposition();
        };

        window.Opened += (_, _) => Reposition();

        window.Closed += (_, _) =>
        {
            if (_windows.TryGetValue(message.Id, out var current) && ReferenceEquals(current, window))
            {
                _windows.Remove(message.Id);
            }
            window.Content = null;
            Reposition();
        };

        _windows[message.Id] = window;
        window.Show();
        _ = FadeAsync(window, 0, 1);
    });

    private async Task FadeAsync(Window window, double from, double to, int durationMs = 0)
    {
        var duration = durationMs > 0 ? durationMs : _toast.FadeDuration;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        window.Opacity = from;

        while (stopwatch.ElapsedMilliseconds < duration)
        {
            var progress = Math.Clamp(stopwatch.Elapsed.TotalMilliseconds / duration, 0.0, 1.0);

            window.Opacity = from + (to - from) * progress;

            await Task.Delay(16);
        }

        window.Opacity = to;
    }

    private void Reposition()
    {
        var screen = _windows.Values.FirstOrDefault(w => w.IsVisible)?.Screens.Primary;
        if (screen is null)
            return;

        var area = screen.WorkingArea;
        var scale = screen.Scaling;

        var margin = (int)Math.Ceiling(_toast.Margin * scale);
        var gap = (int)Math.Ceiling(_toast.Gap * scale);

        var windows = _windows.Values.Where(w => w.IsVisible).Reverse().ToArray();

        var bottom = area.Bottom - margin;
        var right = area.Right - margin;

        foreach (var window in windows)
        {
            var width = (int)Math.Ceiling(window.Bounds.Width * scale);
            var height = (int)Math.Ceiling(window.Bounds.Height * scale);

            if (width <= 0 || height <= 0)
                continue;

            bottom -= height;

            window.Position = new PixelPoint(right - width, Math.Max(area.Y + margin, bottom));

            bottom -= gap;
        }
    }

    public void OnRemoved(object sender, string id) => _ui.Invoke(() =>
    {
        if (!_windows.TryGetValue(id, out var window))
            return;

        if (!_closing.Add(id))
            return;

        _ = CloseWithFadeAsync(id, window);
    });

    private async Task CloseWithFadeAsync(string id, Window window)
    {
        try
        {
            await FadeAsync(window, window.Opacity, 0);
        }
        finally
        {
            if (_windows.TryGetValue(id, out var current) && ReferenceEquals(current, window))
            {
                _windows.Remove(id);
                window.Close();
            }

            _closing.Remove(id);
            Reposition();
        }
    }
    
    private SolidColorBrush GetStatusBrush(NotificationStatus status) 
        => new(Color.Parse(_toast.GetStatusColor(status)));

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _ui.Invoke(() =>
        {
            foreach (var window in _windows.Values.ToArray())
                window.Close();

            _windows.Clear();
            _closing.Clear();
        });
    }
}
*/