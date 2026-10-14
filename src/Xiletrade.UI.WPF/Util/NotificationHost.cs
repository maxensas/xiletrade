using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xiletrade.Library.Models.Application.Notification;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;

namespace Xiletrade.UI.WPF.Util;

/// <summary>
/// Shows independent desktop toasts stacked above the Windows taskbar, at the bottom-right of the work area.
/// </summary>
public sealed class NotificationHost : INotificationHost
{
    private readonly IUIService _ui;
    private readonly Action<string> _dismiss;

    // Insertion order = stacking order (oldest first), guaranteed unlike Dictionary.
    private readonly OrderedDictionary<string, Window> _windows = new(StringComparer.Ordinal);
    private readonly HashSet<string> _closing = new(StringComparer.Ordinal);

    private readonly ToastOptions _toast = new();
    private readonly Duration _fadeDuration;

    private bool _disposed;

    public NotificationHost(IUIService ui, Action<string> dismiss)
    {
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        _dismiss = dismiss ?? throw new ArgumentNullException(nameof(dismiss));

        _fadeDuration = new(TimeSpan.FromMilliseconds(_toast.FadeDuration));
    }

    public void OnAdded(object sender, ToastNotification message) => _ui.Invoke(() =>
    {
        if (_windows.Remove(message.Id, out var old))
        {
            _closing.Remove(message.Id);
            old.Close();
        }

        var panel = new DockPanel
        {
            Margin = new Thickness(16),
            Background = Brushes.Transparent,
            Cursor = System.Windows.Input.Cursors.Hand
        };

        panel.MouseLeftButtonUp += (_, e) =>
        {
            e.Handled = true;
            _dismiss(message.Id);
        };

        var text = new StackPanel();

        text.Children.Add(new TextBlock
        {
            Text = message.Title,
            FontWeight = FontWeights.SemiBold,
            FontSize = _toast.TitleFontSize,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap
        });

        text.Children.Add(new TextBlock
        {
            Text = message.Message,
            FontSize = _toast.MessageFontSize,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0),
            Foreground = Brushes.White
        });

        panel.Children.Add(text);

        var border = new Border
        {
            Background = GetStatusBrush(message.Status),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
            Child = panel
        };

        var window = new Window
        {
            Title = message.Title,
            Content = border,
            Width = _toast.Width,
            SizeToContent = SizeToContent.Height,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            Topmost = true,
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            AllowsTransparency = true,
            WindowStartupLocation = WindowStartupLocation.Manual,
            ShowActivated = false,
            Opacity = 0
        };

        window.Closed += (_, _) =>
        {
            if (_windows.TryGetValue(message.Id, out var current)
                && ReferenceEquals(current, window))
            {
                _windows.Remove(message.Id);
                _closing.Remove(message.Id);
            }

            window.Content = null;
            Reposition();
        };

        _windows[message.Id] = window;

        window.Show(); // synchronous layout: ActualWidth/ActualHeight are valid right after
        Reposition();  // placed before the fade-in starts, so nothing is visible at a wrong position

        window.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, _fadeDuration));
    });

    public void OnRemoved(object sender, string id) => _ui.Invoke(() =>
    {
        if (!_windows.TryGetValue(id, out var window))
            return;

        // Already fading out (e.g. click then timer expiry): ignore.
        if (!_closing.Add(id))
            return;

        // The toast stays in the stack while fading, so the others don't jump early.
        var animation = new DoubleAnimation(window.Opacity, 0, _fadeDuration);

        animation.Completed += (_, _) =>
        {
            // Skip if the window was replaced or closed in the meantime.
            if (!_windows.TryGetValue(id, out var current) || !ReferenceEquals(current, window))
                return;

            // Closed handler removes the entry and repositions the remaining toasts.
            window.Close();
        };

        window.BeginAnimation(UIElement.OpacityProperty, animation);
    });

    private void Reposition()
    {
        var area = SystemParameters.WorkArea;
        var bottom = area.Bottom - _toast.Margin;

        // Newest first = lowest on screen.
        foreach (var window in _windows.Values.Reverse().ToArray())
        {
            if (!window.IsVisible)
                continue;

            window.Left = area.Right - window.ActualWidth - _toast.Margin;
            bottom -= window.ActualHeight;
            window.Top = Math.Max(area.Top + _toast.Margin, bottom);
            bottom -= _toast.Gap;
        }
    }

    private SolidColorBrush GetStatusBrush(NotificationStatus status)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_toast.GetStatusColor(status)));
        brush.Freeze();
        return brush;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _ui.Invoke(() =>
        {
            foreach (var w in _windows.Values.ToArray())
                w.Close();

            _windows.Clear();
            _closing.Clear();
        });
    }
}