using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xiletrade.Library.Models.Application.Notification;
using Xiletrade.Library.Services;
using Xiletrade.Library.Services.Interface;
using Xiletrade.Library.Shared.Enum;
using Xiletrade.Library.Shared.Interop.Windows;

namespace Xiletrade.UI.WPF.Util;

/// <summary>
/// Shows independent desktop toasts, stacked by placement (bottom right above the Windows taskbar by default).
/// </summary>
/// <remarks>
/// The backgrounds take the opacity set by the user in the options.
/// The toasts are tool windows not activated by a click : not listed by Alt+Tab, the game keeps the focus.
/// </remarks>
public sealed class NotificationHost : INotificationHost
{
    private readonly IUIService _ui;
    private readonly DataManagerService _dm;
    private readonly Action<string> _dismiss;

    // Insertion order = stacking order (oldest first), guaranteed unlike Dictionary.
    private readonly OrderedDictionary<string, (Window Window, NotificationPlacement Placement)> _windows = new(StringComparer.Ordinal);
    private readonly HashSet<string> _closing = new(StringComparer.Ordinal);

    private readonly ToastOptions _toast = new();
    private readonly Duration _fadeDuration;

    private bool _disposed;

    /// <summary>
    /// Height kept free at the bottom of the screen by a window glued to it (buttons of the live searches) :
    /// the toasts placed at the bottom right are stacked above it.
    /// </summary>
    public static double ReservedBottom { get; set; }

    public NotificationHost(IUIService ui, DataManagerService dm, Action<string> dismiss)
    {
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        _dm = dm ?? throw new ArgumentNullException(nameof(dm));
        _dismiss = dismiss ?? throw new ArgumentNullException(nameof(dismiss));

        _fadeDuration = new(TimeSpan.FromMilliseconds(_toast.FadeDuration));
    }

    public void OnAdded(object sender, ToastNotification message) => _ui.Invoke(() =>
    {
        if (_windows.Remove(message.Id, out var old))
        {
            _closing.Remove(message.Id);
            old.Window.Close();
        }

        var opacity = _dm.Opacity;

        var panel = new DockPanel
        {
            Margin = new Thickness(16),
            Background = Brushes.Transparent
        };

        if (message.Content is not null)
        {
            // view model displayed by its template (application resources) : closed by its own buttons
            panel.Children.Add(new ContentControl { Content = message.Content, Focusable = false });
        }
        else
        {
            panel.Cursor = System.Windows.Input.Cursors.Hand;
            panel.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                _dismiss(message.Id);
            };
            panel.Children.Add(CreateText(message));
        }

        var border = new Border
        {
            Background = GetStatusBrush(message.Status, opacity),
            CornerRadius = new CornerRadius(8),
            ClipToBounds = true,
            Child = panel
        };
        if (message.Content is not null)
        {
            // same background and border as the window of Xiletrade
            border.Background = GetThemeBrush("WindowBackgroundBrush", opacity) ?? border.Background;
            border.BorderBrush = GetThemeBrush("WindowBorderBrush", opacity);
            border.BorderThickness = new Thickness(2);
        }
        if (message.IsPinned)
        {
            border.BorderBrush = GetThemeBrush("PinnedBorderBrush", opacity);
            border.BorderThickness = new Thickness(2);
        }

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
                && ReferenceEquals(current.Window, window))
            {
                _windows.Remove(message.Id);
                _closing.Remove(message.Id);
            }

            window.Content = null;
            Reposition();
        };

        window.SourceInitialized += OnSourceInitialized; // before it is visible
        _windows[message.Id] = (window, message.Placement);

        window.Show(); // synchronous layout: ActualWidth/ActualHeight are valid right after
        Reposition();  // placed before the fade-in starts, so nothing is visible at a wrong position

        window.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, _fadeDuration));
        if (message.PlaySound)
        {
            System.Media.SystemSounds.Hand.Play();
        }
    });

    // not listed by Alt+Tab, not activated by a click
    private static void OnSourceInitialized(object sender, EventArgs e)
        => Native.AddExtendedStyle(new WindowInteropHelper((Window)sender).Handle, Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE);

    // title and message of a toast without content
    private StackPanel CreateText(ToastNotification message)
    {
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

        return text;
    }

    public void OnRemoved(object sender, string id) => _ui.Invoke(() =>
    {
        if (!_windows.TryGetValue(id, out var toast))
            return;

        var window = toast.Window;

        // Already fading out (e.g. click then timer expiry): ignore.
        if (!_closing.Add(id))
            return;

        // The toast stays in the stack while fading, so the others don't jump early.
        var animation = new DoubleAnimation(window.Opacity, 0, _fadeDuration);

        animation.Completed += (_, _) =>
        {
            // Skip if the window was replaced or closed in the meantime.
            if (!_windows.TryGetValue(id, out var current) || !ReferenceEquals(current.Window, window))
                return;

            // Closed handler removes the entry and repositions the remaining toasts.
            window.Close();
        };

        window.BeginAnimation(UIElement.OpacityProperty, animation);
    });

    private void Reposition()
    {
        var area = SystemParameters.WorkArea;
        var minTop = area.Top + _toast.Margin;
        var maxBottom = area.Bottom - _toast.Margin;

        // one stack by placement : bottom of the stacks growing upwards, top of the stack growing downwards
        var bottom = Math.Min(area.Bottom, SystemParameters.PrimaryScreenHeight - ReservedBottom) - _toast.Margin;
        var quarter = SystemParameters.PrimaryScreenHeight * 0.75;
        var top = minTop;

        // Newest first = nearest to the start of its stack.
        foreach (var (window, placement) in _windows.Values.Reverse().ToArray())
        {
            if (!window.IsVisible)
                continue;

            window.Left = area.Right - window.ActualWidth - _toast.Margin;
            switch (placement)
            {
                case NotificationPlacement.TopRight:
                    window.Top = Math.Min(maxBottom - window.ActualHeight, top);
                    top += window.ActualHeight + _toast.Gap;
                    break;
                case NotificationPlacement.LowerQuarterRight:
                    quarter -= window.ActualHeight;
                    window.Top = Math.Max(minTop, quarter);
                    quarter -= _toast.Gap;
                    break;
                default:
                    bottom -= window.ActualHeight;
                    window.Top = Math.Max(minTop, bottom);
                    bottom -= _toast.Gap;
                    break;
            }
        }
    }

    // brush of the theme (application resources) with the opacity of the options
    private static Brush GetThemeBrush(string key, double opacity)
    {
        if (Application.Current.TryFindResource(key) is not Brush theme)
            return null;

        var brush = theme.CloneCurrentValue();
        brush.Opacity = opacity;
        brush.Freeze();
        return brush;
    }

    private SolidColorBrush GetStatusBrush(NotificationStatus status, double opacity)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_toast.GetStatusColor(status)))
        {
            Opacity = opacity
        };
        brush.Freeze();
        return brush;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _ui.Invoke(() =>
        {
            foreach (var (window, _) in _windows.Values.ToArray())
                window.Close();

            _windows.Clear();
            _closing.Clear();
        });
    }
}